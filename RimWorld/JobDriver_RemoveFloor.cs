using Verse;

namespace RimWorld
{
	public class JobDriver_RemoveFloor : JobDriver_AffectFloor
	{
		protected override int BaseWorkAmount => 200;

		protected override DesignationDef DesDef => DesignationDefOf.RemoveFloor;

		protected override StatDef SpeedStat => StatDefOf.ConstructionSpeed;

		public JobDriver_RemoveFloor()
		{
			clearSnow = true;
		}

		protected override void DoEffect(IntVec3 c)
		{
			if (base.Map.terrainGrid.CanRemoveTopLayerAt(c))
			{
				base.Map.terrainGrid.RemoveTopLayer(base.TargetLocA);
				FilthMaker.RemoveAllFilth(c, base.Map);
			}
		}
	}
}
