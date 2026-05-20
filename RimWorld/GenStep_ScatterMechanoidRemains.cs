using System;
using Verse;

namespace RimWorld;

[Obsolete]
public class GenStep_ScatterMechanoidRemains : GenStep_Scatterer
{
	private const int ShellScatterRadius = 10;

	private static readonly IntRange MaxShellsRange = new IntRange(1, 2);

	public override int SeedPart => 344678634;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckIdeology("Scatter mechanoid remains"))
		{
			count = 1;
			allowInWaterBiome = false;
			base.Generate(map, parms);
		}
	}

	protected override bool CanScatterAt(IntVec3 loc, Map map)
	{
		if (!base.CanScatterAt(loc, map))
		{
			return false;
		}
		if (!CanPlaceDebrisAt(loc, map))
		{
			return false;
		}
		return true;
	}

	private bool CanPlaceDebrisAt(IntVec3 loc, Map map)
	{
		if (!loc.InBounds(map))
		{
			return false;
		}
		if (loc.Roofed(map))
		{
			return false;
		}
		if (!loc.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
		{
			return false;
		}
		if (map.terrainGrid.TerrainAt(loc).IsWater)
		{
			return false;
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		foreach (IntVec3 item in GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientMechDropBeacon), loc, map, Rot4.North).OccupiedRect())
		{
			if (Rand.Bool && item.InBounds(map) && !item.Impassable(map))
			{
				FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_MachineBits);
			}
		}
		FilthMaker.TryMakeFilth(loc, map, ThingDefOf.Filth_RubbleBuilding);
		CellRect cellRect = CellRect.CenteredOn(loc, 10);
		int randomInRange = MaxShellsRange.RandomInRange;
		int num = 0;
		foreach (IntVec3 item2 in cellRect.InRandomOrder())
		{
			if (!GenSight.LineOfSight(item2, loc, map, skipFirstCell: true))
			{
				continue;
			}
			ThingDef chunkMechanoidSlag = ThingDefOf.ChunkMechanoidSlag;
			Rot4 rot = (chunkMechanoidSlag.rotatable ? Rot4.Random : Rot4.North);
			bool flag = true;
			foreach (IntVec3 item3 in GenAdj.OccupiedRect(item2, rot, chunkMechanoidSlag.size))
			{
				if (!CanPlaceDebrisAt(item3, map))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(chunkMechanoidSlag), item2, map, rot);
				FilthMaker.TryMakeFilth(item2, map, ThingDefOf.Filth_Ash);
				num++;
				if (num >= randomInRange)
				{
					break;
				}
			}
		}
	}
}
