using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_GameEndedWanderersJoin : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		return CanSpawnJoiner(map);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!CanSpawnJoiner(map))
		{
			return false;
		}
		TryFindEntryCell(map, out var cell);
		List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;
		foreach (Pawn item in startingAndOptionalPawns)
		{
			GenSpawn.Spawn(item, cell, map);
			foreach (ThingDefCount item2 in Find.GameInitData.startingPossessions[item])
			{
				ThingWithComps thingWithComps = StartingPawnUtility.GenerateStartingPossession(item2) as ThingWithComps;
				if (thingWithComps.HasComp<CompEquippable>())
				{
					item.equipment.AddEquipment(thingWithComps);
				}
				else if (thingWithComps is Apparel newApparel)
				{
					item.apparel.Wear(newApparel);
				}
				else
				{
					item.inventory.innerContainer.TryAdd(thingWithComps);
				}
			}
		}
		SendStandardLetter(def.letterLabel, def.letterText, LetterDefOf.PositiveEvent, parms, startingAndOptionalPawns);
		return true;
	}

	public virtual bool CanSpawnJoiner(Map map)
	{
		IntVec3 cell;
		return TryFindEntryCell(map, out cell);
	}

	private bool TryFindEntryCell(Map map, out IntVec3 cell)
	{
		return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out cell);
	}
}
