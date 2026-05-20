using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_AnimalHabitat : TileMutatorWorker
{
	private const float AnimalCommonalityFactor = 10f;

	private const float MinAnimalCommonality = 0.1f;

	private readonly List<PawnKindDef> tmpAnimals = new List<PawnKindDef>();

	public TileMutatorWorker_AnimalHabitat(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		PawnKindDef animalKind = GetAnimalKind(tile);
		return "AnimalHabitat".Translate(NamedArgumentUtility.Named(animalKind, "ANIMALKIND"));
	}

	public override string GetDescription(PlanetTile tile)
	{
		PawnKindDef animalKind = GetAnimalKind(tile);
		return def.description.Formatted(NamedArgumentUtility.Named(animalKind, "ANIMALKIND"));
	}

	public override float AnimalCommonalityFactorFor(PawnKindDef animal, PlanetTile tile)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return 1f;
		}
		if (animal != GetAnimalKind(tile))
		{
			return 1f;
		}
		return 10f;
	}

	public PawnKindDef GetAnimalKind(PlanetTile tile)
	{
		BiomeDef primaryBiome = Find.WorldGrid[tile].PrimaryBiome;
		foreach (PawnKindDef allWildAnimal in primaryBiome.AllWildAnimals)
		{
			if (primaryBiome.CommonalityOfAnimal(allWildAnimal) > 0.1f)
			{
				tmpAnimals.Add(allWildAnimal);
			}
			else if (tile.Tile.IsCoastal && primaryBiome.CommonalityOfCoastalAnimal(allWildAnimal) > 0.1f)
			{
				tmpAnimals.Add(allWildAnimal);
			}
		}
		if (tmpAnimals.Empty())
		{
			Log.ErrorOnce($"TileMutatorWorker_AnimalHabitat: Could not find animal in biome with commonality > {0.1f}, either increase some commonalities, or decrease animalDensity in the biomeDef below this mutators min of {def.animalDensityRange.min}", 123498);
			return null;
		}
		Rand.PushState();
		Rand.Seed = tile.GetHashCode();
		PawnKindDef result = tmpAnimals.RandomElement();
		Rand.PopState();
		tmpAnimals.Clear();
		return result;
	}
}
