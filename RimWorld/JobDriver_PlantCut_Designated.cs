using Verse;

namespace RimWorld
{
	public class JobDriver_PlantCut_Designated : JobDriver_PlantCut
	{
		protected override DesignationDef RequiredDesignation => DesignationDefOf.CutPlant;
	}
}
