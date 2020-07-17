using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace zzLib.Comp
{
    public class CompProperties_UseResearchTechBlueprint: CompProperties_UseEffect
    {

        public CompProperties_UseResearchTechBlueprint()
        {
            compClass = typeof(Comp_UseResearchTechBlueprint);
        }

        public ResearchProjectDef research = null;
        public bool allowRandomResearch = false;
        public float chance = 1;

    }
}
