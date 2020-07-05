using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using zzLib.Util;

namespace zzLib.AI
{
        public class JobDriver_InstallUpgrade : JobDriver
        {
            public override bool TryMakePreToilReservations(bool errorOnFailed)
            {
                return pawn.Reserve(job.targetA, job);
            }

            protected override IEnumerable<Toil> MakeNewToils()
            {
                var upgrade = TargetThingA.FirstUpgradeableComp();
                if (upgrade == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                    yield break;
                }
                AddFailCondition(() => {
                    var comp = TargetThingA.FirstUpgradeableComp();
                    return comp == null || !comp.parent.Spawned || !comp.WantsWork;
                });
                this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
                var gotoUpgradeable = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                var lookForIngredient = new Toil
                {
                    initAction = () => {
                        var missingIngredient = upgrade.TryGetNextMissingIngredient();
                        job.count = missingIngredient.Count;
                        if (missingIngredient.Count > 0)
                        {
                            bool SearchPredicate(Thing thing) => !thing.IsForbidden(pawn) && pawn.CanReserve(thing);
                            var t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(missingIngredient.ThingDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 999,
                                SearchPredicate);
                            if (t == null)
                            {
                                EndJobWith(JobCondition.Incompletable);
                            }
                            else
                            {
                                job.SetTarget(TargetIndex.B, t);
                            }
                        }
                        else
                        {
                            JumpToToil(gotoUpgradeable);
                        }
                    }
                };
                yield return lookForIngredient;
                yield return Toils_Reserve.Reserve(TargetIndex.B, 1, job.count);
                yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch).FailOnDestroyedNullOrForbidden(TargetIndex.B);
                yield return Toils_Haul.StartCarryThing(TargetIndex.B);
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
                yield return new Toil
                {
                    initAction = () => {
                        if (pawn.carryTracker.CarriedThing != null)
                        {
                            pawn.carryTracker.innerContainer.TryTransferToContainer(pawn.carryTracker.CarriedThing, upgrade.GetDirectlyHeldThings(), pawn.carryTracker.CarriedThing.stackCount);
                            pawn.Map.reservationManager.ReleaseAllForTarget(TargetThingB);
                            job.SetTarget(TargetIndex.B, null);
                            JumpToToil(lookForIngredient);
                        }
                    }
                };
                yield return gotoUpgradeable;
                yield return new Toil
                {
                    tickAction = () => {
                        upgrade.DoWork(GetActor().GetStatValue(StatDefOf.ConstructionSpeed));
                        if (upgrade.curComplete)
                        {
                            EndJobWith(JobCondition.Succeeded);
                        }
                    },
                    defaultCompleteMode = ToilCompleteMode.Never
                }.WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A)
                    .WithProgressBar(TargetIndex.A, () => upgrade.WorkProgress);
            }
        }
    public class WorkGiver_InstallUpgrade : WorkGiver_Scanner
    {
        private const int maxIngredientSearchDist = 999;

        public override ThingRequest PotentialWorkThingRequest
        {
            get { return ThingRequest.ForGroup(ThingRequestGroup.Undefined); }
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var thing = t as ThingWithComps;
            if (thing == null) return null;
            var comp = t.FirstUpgradeableComp();
            if (comp == null) return null;
            var missingIngredient = comp.TryGetNextMissingIngredient();

            var status =
                !pawn.Dead && !pawn.Downed && !pawn.IsBurning()
                && !comp.parent.Destroyed && !comp.parent.IsBurning()
                && pawn.CanReserveAndReach(t, PathEndMode.InteractionCell, Danger.Deadly);

            if (status && missingIngredient.Count > 0 && TryFindHaulableOfDef(pawn, missingIngredient.ThingDef) == null)
            {
                JobFailReason.Is("Upgrade_missingMaterials".Translate(missingIngredient.Count, missingIngredient.ThingDef.LabelCap));
                status = false;
            }
            if (status && !comp.PawnMeetsSkillRequirement(pawn))
            {
                JobFailReason.Is("SkillTooLowForConstruction".Translate(SkillDefOf.Construction.LabelCap));
                status = false;
            }
            return status ? JobMaker.MakeJob(Resources.Job.zzInstallUpgrade, t) : null;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var candidates = pawn.Map.designationManager.SpawnedDesignationsOfDef(Resources.Designation.zzInstallUpgrade);
            foreach (var des in candidates)
            {
                var designatedThing = des.target.Thing;
                var comp = designatedThing.FirstUpgradeableComp();
                if (comp != null && comp.WantsWork)
                {
                    yield return designatedThing;
                }
            }
        }

        private Thing TryFindHaulableOfDef(Pawn pawn, ThingDef haulableDef)
        {
            bool SearchPredicate(Thing thing) => !thing.IsForbidden(pawn) && pawn.CanReserve(thing);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(haulableDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), maxIngredientSearchDist, SearchPredicate);
        }
    }
}
