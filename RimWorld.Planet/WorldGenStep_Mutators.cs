using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Mutators : WorldGenStep
{
	private const float MixedBiomeChance = 0.2f;

	private const float CaveChanceMountainous = 0.5f;

	private const float CaveChanceLargeHills = 0.25f;

	private const float RiverIslandChance = 0.05f;

	public override int SeedPart => 897213645;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		using (ProfilerBlock.Scope("Generate Mutators"))
		{
			AddMutatorsFromTile(layer);
		}
	}

	public static void AddMutatorsFromTile(PlanetLayer layer)
	{
		List<TileMutatorDef> list = DefDatabase<TileMutatorDef>.AllDefsListForReading.Where((TileMutatorDef m) => m.chanceOnNonLandmarkTile > 0f).ToList();
		foreach (Tile tile2 in layer.Tiles)
		{
			PlanetTile tile = tile2.tile;
			if (tile2.hilliness == Hilliness.Mountainous || tile2.hilliness == Hilliness.Impassable)
			{
				TryAddMutator(tile2, layer, TileMutatorDefOf.Mountain);
			}
			if (ModsConfig.OdysseyActive && Find.World.LakeDirectionAt(tile).IsValid)
			{
				TryAddMutator(tile2, layer, TileMutatorDefOf.Lakeshore);
			}
			else if (Find.World.CoastDirectionAt(tile).IsValid)
			{
				TryAddMutator(tile2, layer, TileMutatorDefOf.Coast);
			}
			if (tile2 is SurfaceTile surfaceTile && !surfaceTile.Rivers.NullOrEmpty())
			{
				if (!ModsConfig.OdysseyActive)
				{
					TryAddMutator(tile2, layer, TileMutatorDefOf.River);
				}
				else if ((!surfaceTile.IsCoastal || surfaceTile.Rivers.Count <= 2 || !TryAddMutator(tile2, layer, TileMutatorDefOf.RiverDelta)) && (surfaceTile.Rivers.Count != 1 || !TryAddMutator(tile2, layer, TileMutatorDefOf.Headwater)) && (surfaceTile.Rivers.Count <= 2 || !TryAddMutator(tile2, layer, TileMutatorDefOf.RiverConfluence)) && (!Rand.Chance(0.05f) || !TryAddMutator(tile2, layer, TileMutatorDefOf.RiverIsland)))
				{
					TryAddMutator(tile2, layer, TileMutatorDefOf.River);
				}
			}
			if ((tile2.hilliness == Hilliness.Mountainous && Rand.Chance(0.5f)) || (tile2.hilliness == Hilliness.LargeHills && Rand.Chance(0.25f)))
			{
				TryAddMutator(tile2, layer, TileMutatorDefOf.Caves);
			}
			if (tile2.Landmark == null)
			{
				foreach (TileMutatorDef item in list)
				{
					if (Rand.Chance(item.chanceOnNonLandmarkTile))
					{
						TryAddMutator(tile2, layer, item);
					}
				}
			}
			if (ModsConfig.OdysseyActive && Rand.Chance(0.2f))
			{
				TryAddMutator(tile2, layer, TileMutatorDefOf.MixedBiome);
			}
		}
	}

	private static bool TryAddMutator(Tile tile, PlanetLayer layer, TileMutatorDef mutator)
	{
		if (tile.Mutators.Contains(mutator))
		{
			return false;
		}
		if (!mutator.IsValidTile(tile.tile, layer))
		{
			return false;
		}
		try
		{
			tile.AddMutator(mutator);
		}
		catch (Exception ex)
		{
			Log.Error("Error adding mutator " + mutator?.ToString() + " to tile " + tile?.ToString() + ": " + ex);
			return false;
		}
		return true;
	}
}
