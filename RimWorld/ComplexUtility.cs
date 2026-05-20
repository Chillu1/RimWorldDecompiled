using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class ComplexUtility
{
	public static bool TryFindRandomSpawnCell(ThingDef def, LayoutRoom room, Map map, out IntVec3 spawnPosition, int gap = 1, Rot4? rot = null)
	{
		ThingDef def2 = def;
		Map map2 = map;
		Func<IntVec3, bool> validator = Validator;
		return room.TryGetRandomCellInRoom(def2, map2, out spawnPosition, null, 1, 0, validator);
		bool Validator(IntVec3 cell)
		{
			CellRect cellRect = GenAdj.OccupiedRect(cell, rot ?? Rot4.North, def.Size).ExpandedBy(gap);
			bool result = true;
			foreach (IntVec3 item in cellRect)
			{
				if (item.InBounds(map) && (item.GetEdifice(map) != null || item.GetFirstPawn(map) != null))
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}

	public static string SpawnRoomEnteredTrigger(LayoutRoom room, Map map)
	{
		string text = "RoomEntered" + Find.UniqueIDsManager.GetNextSignalTagID();
		foreach (CellRect rect in room.rects)
		{
			RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
			obj.signalTag = text;
			obj.Rect = rect;
			GenSpawn.Spawn(obj, rect.CenterCell, map);
		}
		return text;
	}

	public static string SpawnRadialDistanceTrigger(IEnumerable<Thing> things, Map map, int radius)
	{
		string text = "RandomTrigger" + Find.UniqueIDsManager.GetNextSignalTagID();
		foreach (Thing thing in things)
		{
			RadialTrigger obj = (RadialTrigger)ThingMaker.MakeThing(ThingDefOf.RadialTrigger);
			obj.signalTag = text;
			obj.maxRadius = radius;
			obj.lineOfSight = true;
			GenSpawn.Spawn(obj, thing.Position, map);
		}
		return text;
	}
}
