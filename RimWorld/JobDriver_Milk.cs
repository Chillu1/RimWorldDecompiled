using Verse;

namespace RimWorld
{
	public class JobDriver_Milk : JobDriver_GatherAnimalBodyResources
	{
		protected override float WorkTotal => 400f;

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompMilkable>();
		}
	}
}
