using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompTerrainPumpDry : CompTerrainPump
{
	private Sustainer sustainer;

	private CompProperties_TerrainPumpDry Props => (CompProperties_TerrainPumpDry)props;

	protected override void AffectCell(IntVec3 c)
	{
		AffectCell(parent.Map, c);
	}

	public static void AffectCell(Map map, IntVec3 c)
	{
		TerrainDef terrainDef = map.terrainGrid.TopTerrainAt(c);
		TerrainDef terrainToDryTo = GetTerrainToDryTo(map, terrainDef);
		if (terrainToDryTo != null)
		{
			map.terrainGrid.SetTerrain(c, terrainToDryTo);
		}
		TerrainDef terrainDef2 = map.terrainGrid.UnderTerrainAt(c);
		if (terrainDef2 != null)
		{
			TerrainDef terrainToDryTo2 = GetTerrainToDryTo(map, terrainDef2);
			if (terrainToDryTo2 != null)
			{
				map.terrainGrid.SetUnderTerrain(c, terrainToDryTo2);
			}
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		if (sustainer != null && !sustainer.Ended)
		{
			sustainer.End();
		}
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		if (!Props.soundWorking.NullOrUndefined() && base.Working && base.CurrentRadius < Props.radius - 0.0001f)
		{
			if (sustainer == null || sustainer.Ended)
			{
				sustainer = Props.soundWorking.TrySpawnSustainer(SoundInfo.InMap(parent));
			}
			sustainer.Maintain();
		}
		else if (sustainer != null && !sustainer.Ended)
		{
			sustainer.End();
		}
	}

	private static TerrainDef GetTerrainToDryTo(Map map, TerrainDef terrainDef)
	{
		if (terrainDef.driesTo == null)
		{
			return null;
		}
		if (map.Biome == BiomeDefOf.SeaIce)
		{
			return TerrainDefOf.Ice;
		}
		return terrainDef.driesTo;
	}
}
