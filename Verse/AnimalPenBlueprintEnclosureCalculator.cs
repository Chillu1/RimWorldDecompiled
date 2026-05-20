using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class AnimalPenBlueprintEnclosureCalculator
{
	private readonly Predicate<IntVec3> passCheck;

	private readonly Func<IntVec3, bool> cellProcessor;

	public bool isEnclosed;

	public List<IntVec3> cellsFound = new List<IntVec3>();

	private Map map;

	private IntVec3 last_position;

	private int last_stateHash;

	public AnimalPenBlueprintEnclosureCalculator()
	{
		passCheck = PassCheck;
		cellProcessor = CellProcessor;
	}

	public void VisitPen(IntVec3 position, Map map)
	{
		int num = Gen.HashCombineInt(map.listerThings.StateHashOfGroup(ThingRequestGroup.Blueprint), map.listerThings.StateHashOfGroup(ThingRequestGroup.BuildingFrame), map.listerThings.StateHashOfGroup(ThingRequestGroup.BuildingArtificial), 42);
		if (this.map == null || this.map != map || !last_position.Equals(position) || last_stateHash != num)
		{
			this.map = map;
			last_position = position;
			last_stateHash = num;
			isEnclosed = true;
			cellsFound.Clear();
			FloodFill(position);
		}
	}

	private void FloodFill(IntVec3 position)
	{
		map.floodFiller.FloodFill(position, passCheck, cellProcessor);
	}

	private bool CellProcessor(IntVec3 c)
	{
		cellsFound.Add(c);
		if (c.OnEdge(map))
		{
			isEnclosed = false;
			return true;
		}
		return false;
	}

	private bool PassCheck(IntVec3 c)
	{
		if (!c.WalkableByFenceBlocked(map))
		{
			return false;
		}
		foreach (Thing thing in c.GetThingList(map))
		{
			ThingDef def = thing.def;
			if (def.passability == Traversability.Impassable)
			{
				return false;
			}
			if (thing is Building_Door door)
			{
				if (AnimalPenEnclosureCalculator.RoamerCanPass(door))
				{
					return true;
				}
				return false;
			}
			if ((!def.IsBlueprint && !def.IsFrame) || !(def.entityDefToBuild is ThingDef thingDef))
			{
				continue;
			}
			if (thingDef.IsFence || thingDef.passability == Traversability.Impassable)
			{
				return false;
			}
			if (thingDef.IsDoor)
			{
				if (AnimalPenEnclosureCalculator.RoamerCanPass(thingDef))
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}
}
