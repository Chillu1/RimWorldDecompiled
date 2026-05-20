using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_WildPlants : TileMutatorWorker
{
	private const float PlantCommonality = 0.1f;

	public TileMutatorWorker_WildPlants(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		return "WildPlants".Translate(GetPlantKind(tile).label);
	}

	public override string GetDescription(PlanetTile tile)
	{
		return def.description.Formatted(GetPlantKind(tile).label);
	}

	public override IEnumerable<BiomePlantRecord> AdditionalWildPlants(PlanetTile tile)
	{
		if (ModsConfig.OdysseyActive)
		{
			yield return new BiomePlantRecord
			{
				plant = GetPlantKind(tile),
				commonality = 0.1f
			};
		}
	}

	private ThingDef GetPlantKind(PlanetTile tile)
	{
		Rand.PushState();
		Rand.Seed = tile.GetHashCode();
		ThingDef result = def.plantKinds.RandomElement();
		Rand.PopState();
		return result;
	}
}
