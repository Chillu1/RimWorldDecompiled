using Verse;

namespace RimWorld
{
	public class WorkGiver_Milk : WorkGiver_GatherAnimalBodyResources
	{
		protected override JobDef JobDef => JobDefOf.Milk;

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompMilkable>();
		}
	}
}
