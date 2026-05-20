using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public abstract class TileMutatorWorker
{
	public TileMutatorDef def;

	public TileMutatorWorker(TileMutatorDef def)
	{
		this.def = def;
	}

	public virtual bool IsValidTile(PlanetTile tile, PlanetLayer layer)
	{
		return true;
	}

	public virtual void OnAddedToTile(PlanetTile tile)
	{
	}

	public virtual void Init(Map map)
	{
	}

	public virtual void Tick(Map map)
	{
	}

	public virtual string GetLabel(PlanetTile tile)
	{
		return def.label;
	}

	public virtual string GetDescription(PlanetTile tile)
	{
		return def.description;
	}

	public virtual void GeneratePostElevationFertility(Map map)
	{
	}

	public virtual void GeneratePostTerrain(Map map)
	{
	}

	public virtual void GenerateCriticalStructures(Map map)
	{
	}

	public virtual void GenerateNonCriticalStructures(Map map)
	{
	}

	public virtual void GeneratePostFog(Map map)
	{
	}

	public virtual void MutateWeatherCommonalityFor(WeatherDef weather, PlanetTile tile, ref float commonality)
	{
	}

	public virtual float AnimalCommonalityFactorFor(PawnKindDef animal, PlanetTile tile)
	{
		return 1f;
	}

	public virtual float PlantCommonalityFactorFor(ThingDef plant, PlanetTile tile)
	{
		return 1f;
	}

	public virtual IEnumerable<BiomePlantRecord> AdditionalWildPlants(PlanetTile tile)
	{
		return def.additionalWildPlants;
	}
}
