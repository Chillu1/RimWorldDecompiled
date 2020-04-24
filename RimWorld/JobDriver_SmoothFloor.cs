using Verse;

namespace RimWorld
{
	public class JobDriver_SmoothFloor : JobDriver_AffectFloor
	{
		protected override int BaseWorkAmount => 2800;

		protected override DesignationDef DesDef => DesignationDefOf.SmoothFloor;

		protected override StatDef SpeedStat => StatDefOf.SmoothingSpeed;

		public JobDriver_SmoothFloor()
		{
			clearSnow = true;
		}

		protected override void DoEffect(IntVec3 c)
		{
			TerrainDef smoothedTerrain = base.TargetLocA.GetTerrain(base.Map).smoothedTerrain;
			base.Map.terrainGrid.SetTerrain(base.TargetLocA, smoothedTerrain);
			FilthMaker.RemoveAllFilth(base.TargetLocA, base.Map);
		}
	}
}
