using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_ScatterLayout : GenStep_Scatterer
{
	public List<LayoutThing> layout;

	public bool checkEveryLayoutCell = true;

	public override int SeedPart => 947153967;

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		for (int i = 0; i < layout.Count; i++)
		{
			Thing t = GenSpawn.Spawn(ThingMaker.MakeThing(layout[i].thing), loc + layout[i].offset, map, layout[i].rotation);
			if (layout[i].filthDef == null)
			{
				continue;
			}
			foreach (IntVec3 item in t.OccupiedRect().ExpandedBy(layout[i].filthExpandBy))
			{
				if (Rand.Chance(layout[i].filthChance) && item.InBounds(map))
				{
					FilthMaker.TryMakeFilth(item, map, layout[i].filthDef);
				}
			}
		}
	}

	protected override bool CanScatterAt(IntVec3 loc, Map map)
	{
		if (!base.CanScatterAt(loc, map))
		{
			return false;
		}
		for (int i = 0; i < layout.Count; i++)
		{
			if (!CanSpawn(layout[i].thing, map, loc + layout[i].offset, layout[i].rotation))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanSpawn(ThingDef def, Map map, IntVec3 cell, Rot4 rot)
	{
		CellRect cellRect = GenAdj.OccupiedRect(cell, rot, def.size);
		if (!cellRect.InBounds(map))
		{
			return false;
		}
		foreach (IntVec3 item in cellRect)
		{
			if (checkEveryLayoutCell && !base.CanScatterAt(item, map))
			{
				return false;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!thingList[i].def.destroyable || !thingList[i].def.canScatterOver)
				{
					return false;
				}
			}
		}
		return true;
	}
}
