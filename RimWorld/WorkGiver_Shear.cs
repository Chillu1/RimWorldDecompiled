using Verse;

namespace RimWorld
{
	public class WorkGiver_Shear : WorkGiver_GatherAnimalBodyResources
	{
		protected override JobDef JobDef => JobDefOf.Shear;

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompShearable>();
		}
	}
}
