using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

public class LayoutWorkerComplex : LayoutWorker_Structure
{
	private static readonly FloatRange ThreatPointsFactorRange = new FloatRange(0.25f, 0.35f);

	private static readonly List<Thing> tmpSpawnedThreatThings = new List<Thing>();

	private static readonly IntRange MergeRange = new IntRange(1, 4);

	private static readonly List<ComplexThreat> useableThreats = new List<ComplexThreat>();

	private static readonly Dictionary<int, List<ComplexThreatDef>> usedThreatsByRoom = new Dictionary<int, List<ComplexThreatDef>>();

	public new ComplexLayoutDef Def => (ComplexLayoutDef)base.Def;

	public LayoutWorkerComplex(LayoutDef def)
		: base(def)
	{
	}

	public virtual Faction GetFixedHostileFactionForThreats()
	{
		return null;
	}

	protected virtual void PreSpawnThreats(List<LayoutRoom> rooms, Map map, List<Thing> allSpawnedThings)
	{
	}

	protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
	{
		LayoutStructureSketch sketch = parms.sketch;
		float areaPrunePercent = Def.areaPrunePercent;
		int minRoomHeight = Def.minRoomHeight;
		return RoomLayoutGenerator.GenerateRandomLayout(minRoomWidth: Def.minRoomWidth, minRoomHeight: minRoomHeight, areaPrunePercent: areaPrunePercent, canRemoveRooms: true, generateDoors: false, maxMergeRoomsRange: MergeRange, sketch: sketch, container: rect, corridor: Def.corridorDef, corridorExpansion: 2, corridorShapes: Def.corridorShapes, canDisconnectRooms: Def.canDisconnectRooms);
	}

	public override void Spawn(LayoutStructureSketch layoutStructureSketch, Map map, IntVec3 pos, float? threatPoints = null, List<Thing> allSpawnedThings = null, bool roofs = true, bool canReuseSketch = false, Faction faction = null)
	{
		List<Thing> list = allSpawnedThings ?? new List<Thing>();
		base.Spawn(layoutStructureSketch, map, pos, threatPoints, list, roofs, canReuseSketch, faction);
		List<LayoutRoom> rooms = layoutStructureSketch.structureLayout.Rooms;
		SpawnThings(layoutStructureSketch, map, rooms, list);
		if (threatPoints.HasValue && !Def.threats.NullOrEmpty())
		{
			PreSpawnThreats(rooms, map, list);
			SpawnThreats(layoutStructureSketch, map, pos, threatPoints.Value, list, rooms);
		}
		PostSpawnStructure(rooms, map, list);
		tmpSpawnedThreatThings.Clear();
	}

	private static void SpawnThings(LayoutStructureSketch layoutStructureSketch, Map map, List<LayoutRoom> rooms, List<Thing> spawnedThings)
	{
		if (layoutStructureSketch.thingsToSpawn.NullOrEmpty())
		{
			return;
		}
		HashSet<LayoutRoom> usedRooms = new HashSet<LayoutRoom>();
		for (int num = layoutStructureSketch.thingsToSpawn.Count - 1; num >= 0; num--)
		{
			Thing thing = layoutStructureSketch.thingsToSpawn[num];
			LayoutRoom roomUsed;
			Rot4 rotUsed;
			IntVec3 loc = LayoutWorker.FindBestSpawnLocation(rooms, thing.def, map, out roomUsed, out rotUsed, usedRooms);
			if (!loc.IsValid)
			{
				loc = LayoutWorker.FindBestSpawnLocation(rooms, thing.def, map, out roomUsed, out rotUsed);
			}
			if (!loc.IsValid)
			{
				thing.Destroy();
				layoutStructureSketch.thingsToSpawn.RemoveAt(num);
			}
			else
			{
				GenSpawn.Spawn(thing, loc, map, rotUsed);
				spawnedThings.Add(thing);
				layoutStructureSketch.thingsToSpawn.RemoveAt(num);
				if (!layoutStructureSketch.thingDiscoveredMessage.NullOrEmpty())
				{
					string signalTag = "ThingDiscovered" + Find.UniqueIDsManager.GetNextSignalTagID();
					foreach (CellRect rect in roomUsed.rects)
					{
						RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
						obj.signalTag = signalTag;
						obj.Rect = rect;
						GenSpawn.Spawn(obj, rect.CenterCell, map);
					}
					SignalAction_Message obj2 = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
					obj2.signalTag = signalTag;
					obj2.message = layoutStructureSketch.thingDiscoveredMessage;
					obj2.messageType = MessageTypeDefOf.PositiveEvent;
					obj2.lookTargets = thing;
					GenSpawn.Spawn(obj2, loc, map);
				}
			}
		}
	}

	protected virtual void PostSpawnStructure(List<LayoutRoom> rooms, Map map, List<Thing> allSpawnedThings)
	{
		if (ModsConfig.IdeologyActive)
		{
			SpawnRoomRewards(rooms, map, allSpawnedThings);
			SpawnCommsConsole(rooms, map);
		}
	}

	private static void SpawnCommsConsole(List<LayoutRoom> rooms, Map map)
	{
		foreach (LayoutRoom item in rooms.InRandomOrder())
		{
			if (item.TryGetRandomCellInRoom(ThingDefOf.CommsConsole, map, out var cell, null, 1, 0, (IntVec3 c) => CanPlaceCommsConsoleAt(c, map)))
			{
				GenSpawn.Spawn(ThingDefOf.AncientCommsConsole, cell, map);
				break;
			}
		}
	}

	private static bool CanPlaceCommsConsoleAt(IntVec3 cell, Map map)
	{
		foreach (IntVec3 item in GenAdj.OccupiedRect(cell, Rot4.North, ThingDefOf.AncientCommsConsole.Size).ExpandedBy(1))
		{
			if (item.GetEdifice(map) != null)
			{
				return false;
			}
		}
		return true;
	}

	private void SpawnRoomRewards(List<LayoutRoom> rooms, Map map, List<Thing> allSpawnedThings)
	{
		if (Def.roomRewardCrateFactor <= 0f)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < allSpawnedThings.Count; i++)
		{
			if (allSpawnedThings[i] is Building_Crate)
			{
				num++;
			}
		}
		int num2 = Mathf.RoundToInt((float)rooms.Count * Def.roomRewardCrateFactor) - num;
		if (num2 <= 0)
		{
			return;
		}
		ThingSetMakerDef thingSetMakerDef = Def.rewardThingSetMakerDef ?? ThingSetMakerDefOf.Reward_ItemsStandard;
		foreach (LayoutRoom item in rooms.InRandomOrder())
		{
			if (item.requiredDef != null)
			{
				continue;
			}
			ThingDef ancientHermeticCrate = ThingDefOf.AncientHermeticCrate;
			Map map2 = map;
			Func<IntVec3, bool> validator = CanSpawnAt;
			if (item.TryGetRandomCellInRoom(ancientHermeticCrate, map2, out var cell, null, 2, 0, validator))
			{
				Building_Crate building_Crate = (Building_Crate)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientHermeticCrate), cell, map, Rot4.South);
				List<Thing> list = thingSetMakerDef.root.Generate(default(ThingSetMakerParams));
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					Thing thing = list[num3];
					if (!building_Crate.TryAcceptThing(thing, allowSpecialEffects: false))
					{
						thing.Destroy();
					}
				}
				num2--;
			}
			if (num2 <= 0)
			{
				break;
			}
		}
		bool CanSpawnAt(IntVec3 c)
		{
			return GenSpawn.CanSpawnAt(ThingDefOf.AncientHermeticCrate, c, map, Rot4.South, canWipeEdifices: false);
		}
	}

	private static List<ComplexThreatDef> GetUsedThreats(LayoutRoom room)
	{
		int roomMergedLowestID = GetRoomMergedLowestID(room);
		return usedThreatsByRoom[roomMergedLowestID];
	}

	private static bool TryGetRoomUsedThreats(LayoutRoom room, out List<ComplexThreatDef> threats)
	{
		return usedThreatsByRoom.TryGetValue(GetRoomMergedLowestID(room), out threats);
	}

	private static void AddUsedThreat(LayoutRoom room, ComplexThreatDef threat)
	{
		int roomMergedLowestID = GetRoomMergedLowestID(room);
		if (!usedThreatsByRoom.TryGetValue(roomMergedLowestID, out var value))
		{
			value = (usedThreatsByRoom[roomMergedLowestID] = new List<ComplexThreatDef>());
		}
		value.Add(threat);
	}

	private static int GetRoomMergedLowestID(LayoutRoom room)
	{
		int loadId = room.loadId;
		for (int i = 0; i < room.merged.Count; i++)
		{
			if (room.merged[i].loadId < loadId)
			{
				loadId = room.merged[i].loadId;
			}
		}
		return loadId;
	}

	private void SpawnThreats(LayoutStructureSketch structureSketch, Map map, IntVec3 center, float threatPoints, List<Thing> spawnedThings, List<LayoutRoom> rooms)
	{
		ComplexResolveParams threatParams = new ComplexResolveParams
		{
			map = map,
			complexRect = structureSketch.layoutSketch.OccupiedRect.MovedBy(center),
			hostileFaction = GetFixedHostileFactionForThreats(),
			allRooms = rooms,
			points = threatPoints
		};
		StringBuilder stringBuilder = null;
		if (DebugViewSettings.logComplexGenPoints)
		{
			stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("----- Logging points for " + Def.defName + ". -----");
			stringBuilder.AppendLine($"Total threat points: {threatPoints}");
			stringBuilder.AppendLine($"Room count: {rooms.Count}");
			stringBuilder.AppendLine($"Approx points per room: {threatParams.points}");
			if (threatParams.hostileFaction != null)
			{
				stringBuilder.AppendLine($"Faction: {threatParams.hostileFaction}");
			}
		}
		useableThreats.Clear();
		usedThreatsByRoom.Clear();
		useableThreats.AddRange(Def.threats.Where((ComplexThreat t) => Rand.Chance(t.chancePerComplex)));
		float num = 0f;
		int num2 = 100;
		while (num < threatPoints && num2 > 0)
		{
			num2--;
			LayoutRoom room = rooms.RandomElement();
			threatParams.room = room;
			threatParams.spawnedThings = spawnedThings;
			float b = threatPoints - num;
			threatParams.points = Mathf.Min(ThreatPointsFactorRange.RandomInRange * threatPoints, b);
			if (useableThreats.Where(delegate(ComplexThreat t)
			{
				int num3 = 0;
				foreach (KeyValuePair<int, List<ComplexThreatDef>> item in usedThreatsByRoom)
				{
					item.Deconstruct(out var _, out var value);
					List<ComplexThreatDef> list = value;
					num3 += list.Count((ComplexThreatDef td) => td == t.def);
				}
				if (num3 >= t.maxPerComplex)
				{
					return false;
				}
				List<ComplexThreatDef> threats;
				return (!TryGetRoomUsedThreats(room, out threats) || threats.Count((ComplexThreatDef td) => td == t.def) < t.maxPerRoom) && t.def.Worker.CanResolve(threatParams);
			}).TryRandomElementByWeight((ComplexThreat t) => t.selectionWeight, out var result))
			{
				if (stringBuilder != null)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("-> Resolving threat " + result.def.defName);
				}
				float threatPointsUsed = 0f;
				result.def.Worker.Resolve(threatParams, ref threatPointsUsed, tmpSpawnedThreatThings, stringBuilder);
				num += threatPointsUsed;
				AddUsedThreat(room, result.def);
			}
		}
		if (stringBuilder != null)
		{
			stringBuilder.AppendLine($"Total threat points spent: {num}");
			Log.Message(stringBuilder.ToString());
		}
		useableThreats.Clear();
		usedThreatsByRoom.Clear();
	}
}
