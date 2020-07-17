using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace zzLib.Comp
{
    public class Comp_UseResearchTechBlueprint :CompUseEffect
    {
        private CompProperties_UseResearchTechBlueprint prop;


        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            if (!Rand.Chance(prop.chance))
            {
                Messages.Message("FailedToGetResearchProject".Translate(), MessageTypeDefOf.PositiveEvent, true);
                return;
            }

            if (!prop.research.IsFinished)
            {
                FinishInstantly(prop.research, usedBy);
                return;
            }
            if (!prop.allowRandomResearch)
            {
                Messages.Message("CannotFindAnyMoreResearchProject".Translate(), MessageTypeDefOf.PositiveEvent, true);
                return;
            }
            ResearchProjectDef proj;
            if (TryRandomlyUnfinishedResearch(out proj))
            {
                FinishInstantly(proj, usedBy);
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (CompProperties_UseResearchTechBlueprint)props;
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {

            if (!prop.research.IsFinished)
            {
                failReason = null;
                return true;
            }
            if (!prop.allowRandomResearch)
            {
                failReason = "CannotFindAnyMoreResearchProject".Translate();
                return false;
            }
            ResearchProjectDef researchProjectDef;
            bool result = TryRandomlyUnfinishedResearch(out researchProjectDef);
            failReason = "CannotFindAnyMoreResearchProject".Translate();
            return result;
        }
        private bool TryRandomlyUnfinishedResearch(out ResearchProjectDef researchProj)
        {
            return (from x in DefDatabase<ResearchProjectDef>.AllDefs
                    where !x.IsFinished
                    select x).TryRandomElement(out researchProj);
        }

        private void FinishInstantly(ResearchProjectDef proj,Pawn usedBy)
        {
            Find.ResearchManager.FinishProject(proj, false, usedBy);
            Messages.Message("MessageResearchProjectFinishedByItem".Translate(proj.label), MessageTypeDefOf.PositiveEvent, true);
        }
    }
}
