using Verse;

namespace RimWorld;

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
			TerrainDef terrainDef = base.Map.terrainGrid.TempTerrainAt(c);
			if (terrainDef != null && terrainDef.Removable)
			{
				base.Map.terrainGrid.RemoveTempTerrain(c);
			}
			else
			{
				base.Map.terrainGrid.RemoveTopLayer(base.TargetLocA);
			}
			FilthMaker.RemoveAllFilth(c, base.Map);
		}
	}
}
