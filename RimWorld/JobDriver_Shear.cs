using Verse;

namespace RimWorld
{
	public class JobDriver_Shear : JobDriver_GatherAnimalBodyResources
	{
		protected override float WorkTotal => 1700f;

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompShearable>();
		}
	}
}
