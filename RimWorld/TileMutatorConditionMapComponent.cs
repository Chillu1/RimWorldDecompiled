using Verse;

namespace RimWorld;

public class TileMutatorConditionMapComponent : MapComponent
{
	public TileMutatorConditionMapComponent(Map map)
		: base(map)
	{
	}

	public override void MapGenerated()
	{
		if (map.TileInfo.Mutators.EnumerableNullOrEmpty())
		{
			return;
		}
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			if (mutator.additionalGameConditions.NullOrEmpty())
			{
				continue;
			}
			foreach (GameConditionDef additionalGameCondition in mutator.additionalGameConditions)
			{
				map.GameConditionManager.RegisterCondition(GameConditionMaker.MakeConditionPermanent(additionalGameCondition));
			}
		}
	}
}
