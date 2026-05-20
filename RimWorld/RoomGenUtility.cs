using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class RoomGenUtility
	{
		public static void FillWithPadding(ThingDef thing, FloatRange countPer100Cells, LayoutRoom room, Map map, Rot4? fixedRot = null, Func<IntVec3, Rot4, CellRect, bool> validator = null, List<Thing> spawned = null, int contractedBy = 1, ThingDef stuff = null, bool alignWithRect = false, bool snapToGrid = false, Func<IntVec3, Rot4, Map, Thing> spawnAction = null, Faction faction = null)
		{
			float num = (float)room.rects.Sum((CellRect r) => r.Area) / 100f;
			int count = Mathf.Max(Mathf.RoundToInt(countPer100Cells.RandomInRange * num), 1);
			FillWithPadding(thing, count, room, map, fixedRot, validator, spawned, contractedBy, stuff, alignWithRect, snapToGrid, spawnAction, faction);
		}

		public static void FillWithPadding(ThingDef thing, int count, LayoutRoom room, Map map, Rot4? fixedRot = null, Func<IntVec3, Rot4, CellRect, bool> validator = null, List<Thing> spawned = null, int contractedBy = 1, ThingDef stuff = null, bool alignWithRect = false, bool snapToGrid = false, Func<IntVec3, Rot4, Map, Thing> spawnAction = null, Faction faction = null)
		{
			IntVec2 size = thing.Size;
			int num = 0;
			foreach (IntVec3 item in room.Cells.InRandomOrder())
			{
				if (num >= count)
				{
					break;
				}
				if (!room.Contains(item, contractedBy))
				{
					continue;
				}
				foreach (Rot4 item2 in Rot4.AllRotations.InRandomOrder())
				{
					if (num >= count)
					{
						break;
					}
					if ((fixedRot.HasValue && item2 != fixedRot.Value) || (!thing.rotatable && item2 != Rot4.North))
					{
						continue;
					}
					room.TryGetRectContainingCell(item, out var rect);
					if (alignWithRect)
					{
						Rot4 rot = ((rect.Width >= rect.Height) ? Rot4.East : Rot4.North);
						if (item2 != rot)
						{
							continue;
						}
					}
					if (snapToGrid)
					{
						int num2 = item.x - rect.minX;
						int num3 = item.z - rect.minZ;
						if (num2 % (size.x + 1) != 0 || num3 % (size.z + 1) != 0)
						{
							continue;
						}
					}
					CellRect arg = item.RectAbout(size, item2);
					bool flag = true;
					if (validator != null && !validator(item, item2, arg))
					{
						continue;
					}
					foreach (IntVec3 cell in arg.Cells)
					{
						if (!IsFillValidCell(thing, cell, room, map, contractedBy))
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
					Thing thing2;
					if (spawnAction != null)
					{
						thing2 = spawnAction(item, item2, map);
						if (thing2.def.Size.x > size.x || thing2.def.Size.z > size.z)
						{
							Log.ErrorOnce($"Spawned thing {thing2.def.defName} with size {thing2.def.Size} during room edge filling with larger size as root def {thing.defName} with size {thing.Size}. Size must be the same or smaller.", thing2.def.GetHashCode() ^ 0x2CF2CD);
						}
					}
					else
					{
						thing2 = ThingMaker.MakeThing(thing, stuff);
						thing2.SetFactionDirect(faction);
						GenSpawn.Spawn(thing2, item, map, item2);
					}
					num++;
					spawned?.Add(thing2);
				}
			}
		}

		public static void FillPrefabs(LayoutPrefabParms parms, int count, LayoutRoom room, Map map, Func<IntVec3, Rot4, bool> validator = null, int contractedBy = 1, List<Thing> spawned = null, bool alignWithRect = false, bool snapToGrid = false, Faction faction = null, Func<PrefabThingData, Tuple<ThingDef, ThingDef>> overrideSpawnData = null)
		{
			IntVec2 size = parms.def.size;
			int num = 0;
			foreach (IntVec3 item in room.Cells.InRandomOrder())
			{
				if (num >= count)
				{
					break;
				}
				if (!room.Contains(item, contractedBy))
				{
					continue;
				}
				foreach (Rot4 item2 in Rot4.AllRotations.InRandomOrder())
				{
					if (num >= count)
					{
						break;
					}
					if (!parms.def.rotations.HasFlag((RotEnum)item2))
					{
						continue;
					}
					if (alignWithRect && room.TryGetRectContainingCell(item, out var rect))
					{
						Rot4 rot = ((rect.Width >= rect.Height) ? Rot4.East : Rot4.North);
						if (item2 != rot)
						{
							continue;
						}
					}
					if ((snapToGrid && (item.x % (size.x + 1) != 0 || item.z % (size.z + 1) != 0)) || (validator != null && !validator(item, item2)))
					{
						continue;
					}
					bool flag = true;
					CellRect ignore = default(CellRect);
					if (!parms.ensureNoBlocks)
					{
						ignore = map.BoundsRect();
					}
					foreach (var (prefabThingData, center, rot2) in PrefabUtility.GetThings(parms.def, item, item2))
					{
						if (prefabThingData.def.passability == Traversability.Standable)
						{
							continue;
						}
						foreach (IntVec3 item3 in center.RectAbout(prefabThingData.def.Size, rot2))
						{
							if (!IsFillValidCell(prefabThingData.def, item3, room, map, contractedBy, ignore))
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						PrefabUtility.SpawnPrefab(parms.def, map, item, item2, faction, spawned, overrideSpawnData);
						num++;
					}
				}
			}
		}

		public static void GenerateRows(ThingDef thingDef, LayoutRoom room, Map map, ThingDef stuff = null)
		{
			CellRect rect = room.Boundary.ContractedBy(2);
			if (stuff == null)
			{
				stuff = GenStuff.DefaultStuffFor(thingDef);
			}
			bool flag = Rand.Bool;
			if (rect.Height < rect.Width)
			{
				bool flag2 = false;
				for (int i = 0; i < rect.Width; i++)
				{
					if (!flag2)
					{
						for (int j = 0; j < rect.Height; j++)
						{
							IntVec3 cell = rect.Min + new IntVec3(i, 0, j);
							Rot4 rot = (flag ? Rot4.West : Rot4.East);
							CheckAndSpawnShelf(thingDef, map, stuff, cell, rot, rect);
						}
					}
					flag2 = !flag2;
				}
				return;
			}
			bool flag3 = false;
			for (int k = 0; k < rect.Height; k++)
			{
				if (!flag3)
				{
					for (int l = 0; l < rect.Width; l++)
					{
						IntVec3 cell2 = rect.Min + new IntVec3(l, 0, k);
						Rot4 rot2 = (flag ? Rot4.South : Rot4.North);
						CheckAndSpawnShelf(thingDef, map, stuff, cell2, rot2, rect);
					}
				}
				flag3 = !flag3;
			}
		}

		private static void CheckAndSpawnShelf(ThingDef thingDef, Map map, ThingDef stuff, IntVec3 cell, Rot4 rot, CellRect rect)
		{
			if (GenAdj.OccupiedRect(cell, rot, thingDef.size).FullyContainedWithin(rect) && !GenAdj.OccupiedRect(cell, rot, thingDef.size).Any((IntVec3 c) => c.GetEdifice(map) != null) && GenAdj.OccupiedRect(cell, rot, thingDef.size).All((IntVec3 c) => c.GetFirstThing(map, thingDef) == null))
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(thingDef, stuff), cell, map, rot);
			}
		}

		private static bool IsFillValidCell(ThingDef placing, IntVec3 cell, LayoutRoom room, Map map, int contractedBy, CellRect ignore = default(CellRect))
		{
			if (!room.Contains(cell, contractedBy))
			{
				return false;
			}
			if (cell.GetEdifice(map) != null)
			{
				return false;
			}
			if (placing.passability != Traversability.Standable)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = cell + GenAdj.AdjacentCellsAround[i];
					Building edifice = c.GetEdifice(map);
					if (edifice != null && edifice.def.passability != Traversability.Standable && (ignore == default(CellRect) || !ignore.Contains(c)))
					{
						return false;
					}
				}
			}
			return true;
		}

		public static void SpawnInCorners(ThingDef thing, LayoutRoom room, Map map, ThingDef stuff = null, float spawnChance = 1f, Rot4 rot = default(Rot4))
		{
			foreach (CellRect rect in room.rects)
			{
				foreach (IntVec3 corner in rect.ContractedBy(1).Corners)
				{
					bool flag = true;
					foreach (CellRect rect2 in room.rects)
					{
						if (rect2 != rect && rect2.Contains(corner))
						{
							flag = false;
							break;
						}
					}
					int num = 0;
					for (int i = 0; i < 4; i++)
					{
						if ((corner + GenAdj.CardinalDirections[i]).GetEdifice(map) != null)
						{
							num++;
						}
					}
					if (num >= 2 && flag && Rand.Chance(spawnChance))
					{
						GenSpawn.Spawn(ThingMaker.MakeThing(thing, stuff), corner, map, rot);
					}
				}
			}
		}

		public static void FillAroundEdges(ThingDef thing, FloatRange groupsPerTenEdgeCells, IntRange groupCountRange, LayoutRoom room, Map map, Func<IntVec3, Rot4, CellRect, bool> validator = null, List<Thing> spawned = null, int contractedBy = 1, int padding = 0, ThingDef stuff = null, bool avoidDoors = true, RotationDirection rotationDirectionOffset = RotationDirection.Opposite, Func<IntVec3, Rot4, Map, Thing> spawnAction = null, Faction faction = null)
		{
			float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
			int count = Mathf.Max(Mathf.RoundToInt(groupsPerTenEdgeCells.RandomInRange * num), 1);
			FillAroundEdges(thing, count, groupCountRange, room, map, validator, spawned, contractedBy, padding, stuff, avoidDoors, rotationDirectionOffset, spawnAction, faction);
		}

		public static void FillAroundEdges(ThingDef thing, int count, IntRange groupCountRange, LayoutRoom room, Map map, Func<IntVec3, Rot4, CellRect, bool> validator = null, List<Thing> spawned = null, int contractedBy = 1, int padding = 0, ThingDef stuff = null, bool avoidDoors = true, RotationDirection rotationDirectionOffset = RotationDirection.Opposite, Func<IntVec3, Rot4, Map, Thing> spawnAction = null, Faction faction = null)
		{
			IntVec2 size = thing.Size;
			Dictionary<IntVec3, (Rot4, CellRect)> edgeSpawnOptions = GetEdgeSpawnOptions(room, map, contractedBy);
			if (edgeSpawnOptions.Count == 0 || edgeSpawnOptions.Count == 0)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (num >= count)
				{
					break;
				}
				KeyValuePair<IntVec3, (Rot4, CellRect)> keyValuePair = edgeSpawnOptions.RandomElement();
				edgeSpawnOptions.Remove(keyValuePair.Key);
				IntVec3 key = keyValuePair.Key;
				(Rot4, CellRect) value = keyValuePair.Value;
				Rot4 item = value.Item1;
				CellRect item2 = value.Item2;
				Rot4 rot = item.Rotated(rotationDirectionOffset);
				int randomInRange = groupCountRange.RandomInRange;
				int num2 = 0;
				bool flag = rotationDirectionOffset == RotationDirection.Clockwise || rotationDirectionOffset == RotationDirection.Counterclockwise;
				for (int j = 0; j < randomInRange; j++)
				{
					if (num >= count)
					{
						break;
					}
					int num3 = num2;
					if (j % 2 == 1)
					{
						num3 = -num3;
					}
					else
					{
						num2 += (flag ? size.z : size.x) + padding;
					}
					IntVec3 intVec = key + item.RighthandCell * num3;
					CellRect cellRect = intVec.RectAbout(size, rot);
					bool flag2 = false;
					foreach (CellRect rect in room.rects)
					{
						if (cellRect.FullyContainedWithin(rect.ContractedBy(1)))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						continue;
					}
					cellRect = cellRect.ExpandedBy(padding);
					flag2 = true;
					if (validator != null && !validator(key, rot, cellRect))
					{
						continue;
					}
					foreach (IntVec3 cell in cellRect.Cells)
					{
						if (room.Contains(cell, 1))
						{
							if (avoidDoors && IsDoorAdjacentTo(cell, map))
							{
								flag2 = false;
								break;
							}
							if (!IsFillValidCell(thing, cell, room, map, contractedBy, item2))
							{
								flag2 = false;
								break;
							}
						}
					}
					if (!flag2)
					{
						break;
					}
					Thing thing2;
					if (spawnAction != null)
					{
						thing2 = spawnAction(intVec, rot, map);
						if (thing2.def.Size != size)
						{
							Log.ErrorOnce($"Spawned thing {thing2.def.defName} with size {thing2.def.Size} during room edge filling which did not have same size as root def {thing.defName} with size {thing.Size}. Size must match.", thing2.def.GetHashCode() ^ 0x2CF2CD);
						}
					}
					else
					{
						thing2 = ThingMaker.MakeThing(thing, stuff);
						thing2.SetFaction(faction);
						GenSpawn.Spawn(thing2, intVec, map, rot);
					}
					num++;
					foreach (IntVec3 cell2 in cellRect.Cells)
					{
						edgeSpawnOptions.Remove(cell2);
					}
					spawned?.Add(thing2);
				}
			}
		}

		public static void FillPrefabsAroundEdges(LayoutPrefabParms parms, int count, LayoutRoom room, Map map, Func<IntVec3, Rot4, bool> validator = null, List<Thing> spawned = null, int contractedBy = 1, bool avoidDoors = true, Faction faction = null, Func<PrefabThingData, Tuple<ThingDef, ThingDef>> overrideSpawnData = null)
		{
			Dictionary<IntVec3, (Rot4, CellRect)> edgeSpawnOptions = GetEdgeSpawnOptions(room, map, contractedBy);
			if (edgeSpawnOptions.Count == 0)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (num >= count)
				{
					break;
				}
				if (edgeSpawnOptions.Count == 0)
				{
					break;
				}
				edgeSpawnOptions.RandomElement().Deconstruct(out var key, out var value);
				(Rot4, CellRect) tuple = value;
				IntVec3 intVec = key;
				var (dir, ignore) = tuple;
				edgeSpawnOptions.Remove(intVec);
				Rot4 rot = dir.Rotated(parms.rotOffset);
				if (!parms.def.rotations.HasFlag((RotEnum)rot))
				{
					continue;
				}
				CellRect cellRect = intVec.RectAbout(parms.def.size, rot);
				IntVec3 offset = -dir.FacingCell;
				int num2 = room.Boundary.GetSideLength(dir.Rotated(RotationDirection.Clockwise));
				while (!cellRect.GetEdgeCells(dir).Contains(intVec) && --num2 > 0)
				{
					cellRect = cellRect.MovedBy(offset);
				}
				bool flag = false;
				foreach (CellRect rect in room.rects)
				{
					if (cellRect.FullyContainedWithin(rect.ContractedBy(1)))
					{
						flag = true;
						break;
					}
				}
				if (!flag || (validator != null && !validator(cellRect.CenterCell, rot)))
				{
					continue;
				}
				foreach (var (prefabThingData, center, rot2) in PrefabUtility.GetThings(parms.def, cellRect.CenterCell, rot))
				{
					if (prefabThingData.def.passability == Traversability.Standable)
					{
						continue;
					}
					foreach (IntVec3 item in center.RectAbout(prefabThingData.def.Size, rot2))
					{
						if (avoidDoors && IsDoorAdjacentTo(item, map))
						{
							flag = false;
							break;
						}
						if (!IsFillValidCell(prefabThingData.def, item, room, map, contractedBy, ignore))
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					continue;
				}
				PrefabUtility.SpawnPrefab(parms.def, map, cellRect.CenterCell, rot, faction, spawned, overrideSpawnData);
				num++;
				foreach (IntVec3 cell in cellRect.Cells)
				{
					edgeSpawnOptions.Remove(cell);
				}
			}
		}

		private static Dictionary<IntVec3, (Rot4 rot, CellRect wall)> GetEdgeSpawnOptions(LayoutRoom room, Map map, int contractedBy)
		{
			Dictionary<IntVec3, (Rot4, CellRect)> dictionary = new Dictionary<IntVec3, (Rot4, CellRect)>();
			foreach (CellRect rect in room.rects)
			{
				CellRect cellRect = rect.ContractedBy(contractedBy);
				for (int i = 0; i < 4; i++)
				{
					Rot4 rot = new Rot4(i);
					CellRect edgeRect = rect.GetEdgeRect(rot);
					foreach (IntVec3 edgeCell in cellRect.GetEdgeCells(rot))
					{
						bool flag = true;
						if (dictionary.ContainsKey(edgeCell) || (edgeCell + rot.FacingCell).GetEdifice(map) == null)
						{
							continue;
						}
						foreach (CellRect rect2 in room.rects)
						{
							if (!(rect2 == rect) && rect2.Contains(edgeCell))
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							dictionary[edgeCell] = (rot, edgeRect);
						}
					}
				}
			}
			return dictionary;
		}

		public static bool IsDoorAdjacentTo(IntVec3 cell, Map map, bool cardinalOnly = true)
		{
			return IsAdjacentTo(cell, (IntVec3 p) => p.GetDoor(map) != null, cardinalOnly);
		}

		public static bool IsAdjacentTo(IntVec3 cell, Predicate<IntVec3> predicate, bool cardinalOnly = false)
		{
			IntVec3[] array = (cardinalOnly ? GenAdj.CardinalDirections : GenAdj.AdjacentCellsAround);
			for (int i = 0; i < array.Length; i++)
			{
				if (predicate(cell + array[i]))
				{
					return true;
				}
			}
			return false;
		}

		public static Building_AncientCryptosleepCasket SpawnCryptoCasket(IntVec3 cell, Map map, Rot4 rot, int groupID, PodContentsType type, ThingSetMakerDef thingSetMakerDef)
		{
			Building_AncientCryptosleepCasket building_AncientCryptosleepCasket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket);
			building_AncientCryptosleepCasket.groupID = groupID;
			ThingSetMakerParams parms = new ThingSetMakerParams
			{
				podContentsType = type
			};
			List<Thing> list = thingSetMakerDef.root.Generate(parms);
			for (int i = 0; i < list.Count; i++)
			{
				if (!building_AncientCryptosleepCasket.TryAcceptThing(list[i], allowSpecialEffects: false))
				{
					if (list[i] is Pawn pawn)
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
					else
					{
						list[i].Destroy();
					}
				}
			}
			GenSpawn.Spawn(building_AncientCryptosleepCasket, cell, map, rot);
			return building_AncientCryptosleepCasket;
		}

		public static SignalAction_OpenCasket SpawnOpenCryptoCasketSignal(Building_AncientCryptosleepCasket pod, Map map, string signalTag)
		{
			SignalAction_OpenCasket obj = (SignalAction_OpenCasket)ThingMaker.MakeThing(ThingDefOf.SignalAction_OpenCasket);
			obj.signalTag = signalTag;
			obj.caskets.Add(pod);
			GenSpawn.Spawn(obj, pod.Position, map);
			return obj;
		}

		public static void SpawnDormantThreatCluster(LayoutRoom room, Map map, float threatPoints, string signalTag = null)
		{
			if (Rand.Chance(0.5f) && Faction.OfInsects != null)
			{
				SpawnDormantInsectCluster(room, map, threatPoints, signalTag);
			}
			else if (Faction.OfMechanoids != null)
			{
				SpawnDormantMechCluster(room, map, threatPoints, signalTag);
			}
		}

		public static void SpawnDormantInsectCluster(LayoutRoom room, Map map, float threatPoints, string signalTag = null)
		{
			SpawnDormantThreatCluster(room, map, Faction.OfInsects, (PawnKindDef kind) => kind.RaceProps.Insect, threatPoints, signalTag);
		}

		public static void SpawnDormantMechCluster(LayoutRoom room, Map map, float threatPoints, string signalTag = null)
		{
			SpawnDormantThreatCluster(room, map, Faction.OfMechanoids, MechClusterGenerator.MechKindSuitableForCluster, threatPoints, signalTag);
		}

		public static void SpawnDormantThreatCluster(LayoutRoom room, Map map, Faction faction, Func<PawnKindDef, bool> selector, float threatPoints, string signalTag = null)
		{
			if (faction == null)
			{
				Log.Warning("Tried to spawn dormant threat cluster with null faction.");
				return;
			}
			int num = room.rects.Max((CellRect r) => Mathf.Max(r.Width, r.Height));
			LordJob_StructureThreatCluster lordJob = new LordJob_StructureThreatCluster(faction, room.rects[0].CenterCell, Mathf.Min(12f, num * 2), sendWokenUpMessage: true, awakeOnClamor: true);
			Lord lord = LordMaker.MakeNewLord(faction, lordJob, map);
			foreach (PawnKindDef combatPawnKindsForPoint in PawnUtility.GetCombatPawnKindsForPoints(selector, threatPoints, (PawnKindDef pk) => 1f / pk.combatPower))
			{
				Pawn pawn = PawnGenerator.GeneratePawn(combatPawnKindsForPoint, faction);
				if (room.TryGetRandomCellInRoom(out var cell, 1, Validator))
				{
					GenSpawn.Spawn(pawn, cell, map);
					lord.AddPawn(pawn);
				}
			}
			if (signalTag != null)
			{
				SignalAction_DormancyWakeUp obj = (SignalAction_DormancyWakeUp)ThingMaker.MakeThing(ThingDefOf.SignalAction_DormancyWakeUp);
				obj.signalTag = signalTag;
				obj.lord = lord;
				GenSpawn.Spawn(obj, map.Center, map);
			}
			bool Validator(IntVec3 c)
			{
				if (c.GetEdifice(map) == null)
				{
					return c.Standable(map);
				}
				return false;
			}
		}

		public static void SpawnWallAttatchments(ThingDef thingDef, Map map, LayoutRoom room, float chance = 1f, float thingsPerSide10Cells = 1f, Predicate<IntVec3, Rot4> validator = null, ThingDef stuff = null, Faction faction = null)
		{
			SpawnWallAttatchments(thingDef, map, room.rects, chance, thingsPerSide10Cells, validator, stuff, faction);
		}

		public static void SpawnWallAttatchments(ThingDef thingDef, Map map, List<CellRect> rects, float chance = 1f, float thingsPerSide10Cells = 1f, Predicate<IntVec3, Rot4> validator = null, ThingDef stuff = null, Faction faction = null)
		{
			foreach (IntVec3 item in IdealWallAttatchmentPositions(thingDef, map, rects, thingsPerSide10Cells, validator))
			{
				if (Rand.Chance(chance))
				{
					TrySpawnWallAttatchment(thingDef, item, map, out var _, avoidEdifices: true, validator, stuff, faction);
				}
			}
		}

		public static void SpawnWallAttatchments(ThingDef thingDef, Map map, LayoutRoom room, IntRange count, Predicate<IntVec3, Rot4> validator = null, ThingDef stuff = null, Faction faction = null)
		{
			SpawnWallAttatchments(thingDef, map, room.rects, count, validator, stuff, faction);
		}

		public static void SpawnWallAttatchments(ThingDef thingDef, Map map, List<CellRect> rects, IntRange count, Predicate<IntVec3, Rot4> validator = null, ThingDef stuff = null, Faction faction = null)
		{
			List<IntVec3> list = IdealWallAttatchmentPositions(thingDef, map, rects, 1f, validator);
			int randomInRange = count.RandomInRange;
			int num = 0;
			Thing spawned;
			for (int i = 0; i < randomInRange; i++)
			{
				if (!list.Any())
				{
					break;
				}
				IntVec3 intVec = list.RandomElement();
				list.Remove(intVec);
				if (TrySpawnWallAttatchment(thingDef, intVec, map, out spawned, avoidEdifices: true, validator, stuff, faction))
				{
					num++;
				}
			}
			for (int j = num; j < count.TrueMin; j++)
			{
				if (num >= count.TrueMin)
				{
					break;
				}
				foreach (CellRect rect in rects)
				{
					foreach (IntVec3 item in rect.ContractedBy(1).EdgeCells.InRandomOrder())
					{
						if (TrySpawnWallAttatchment(thingDef, item, map, out spawned, avoidEdifices: true, validator, stuff, faction))
						{
							num++;
						}
						if (num >= count.TrueMin)
						{
							break;
						}
					}
					if (num >= count.TrueMin)
					{
						break;
					}
				}
			}
		}

		private static List<IntVec3> IdealWallAttatchmentPositions(ThingDef thingDef, Map map, List<CellRect> rects, float thingsPerSide10Cells = 1f, Predicate<IntVec3, Rot4> validator = null)
		{
			List<IntVec3> list = new List<IntVec3>();
			foreach (CellRect rect in rects)
			{
				CellRect cellRect = rect.ContractedBy(1);
				for (int i = 0; i < 2; i++)
				{
					Rot4 rot = ((i == 0) ? Rot4.North : Rot4.East);
					float num = (float)cellRect.GetSideLength(rot) / 2f;
					int b = Mathf.RoundToInt(num / 3f);
					int num2 = Mathf.Min(Mathf.RoundToInt(num / 10f * thingsPerSide10Cells), b);
					if (num2 <= 0)
					{
						continue;
					}
					int num3 = Mathf.Max(3, Mathf.RoundToInt(num / (float)(num2 + 1)));
					int num4 = num3;
					for (int j = 0; j < num2; j++)
					{
						for (int k = 0; k < 2; k++)
						{
							int num5 = num4;
							if (cellRect.Width % 2 == 0 && k == 0 && i == 0)
							{
								num5--;
							}
							else if (cellRect.Height % 2 == 0 && k == 0 && i == 1)
							{
								num5--;
							}
							for (int l = 0; l < 2; l++)
							{
								Rot4 rot2 = ((l == 0) ? rot : rot.Opposite);
								IntVec3 centerCellOnEdge = cellRect.GetCenterCellOnEdge(rot2, (k == 0) ? num5 : (-num5));
								if (cellRect.Contains(centerCellOnEdge) && !list.Contains(centerCellOnEdge) && CanPlaceWallAttachment(thingDef, centerCellOnEdge, map, out var _, avoidEdifices: true, validator))
								{
									list.Add(centerCellOnEdge);
								}
							}
						}
						num4 += num3;
					}
				}
			}
			return list;
		}

		public static bool TrySpawnWallAttatchment(ThingDef thingDef, IntVec3 cell, Map map, out Thing spawned, bool avoidEdifices = true, Predicate<IntVec3, Rot4> validator = null, ThingDef stuff = null, Faction faction = null)
		{
			if (CanPlaceWallAttachment(thingDef, cell, map, out var rotation, avoidEdifices, validator))
			{
				spawned = ThingMaker.MakeThing(thingDef, stuff);
				spawned.SetFactionDirect(faction);
				GenSpawn.Spawn(spawned, cell, map, rotation);
				return true;
			}
			spawned = null;
			return false;
		}

		public static bool CanPlaceWallAttachment(ThingDef thingDef, IntVec3 cell, Map map, out Rot4 rotation, bool avoidEdifices = true, Predicate<IntVec3, Rot4> validator = null)
		{
			rotation = default(Rot4);
			if (avoidEdifices && cell.GetEdifice(map) != null)
			{
				return false;
			}
			foreach (Rot4 allRotation in Rot4.AllRotations)
			{
				if (CanPlace(thingDef, cell, map, allRotation) && (validator == null || validator(cell, allRotation)))
				{
					rotation = allRotation;
					return true;
				}
			}
			return false;
		}

		public static bool CanPlace(ThingDef thingDef, IntVec3 cell, Map map, Rot4 rot)
		{
			if (thingDef.IsEdifice() && cell.GetEdifice(map) != null)
			{
				return false;
			}
			foreach (PlaceWorker placeWorker in thingDef.PlaceWorkers)
			{
				if (!placeWorker.AllowsPlacing(thingDef, cell, rot, map))
				{
					return false;
				}
			}
			return true;
		}

		public static void SpawnCorpses(LayoutRoom room, Map map, IntRange countRange, PawnKindDef kind, DamageDef damageType, ThingDef killerThing = null, Tool toolUsed = null, FloatRange? ageDaysRange = null, IntRange? bloodFilthRange = null, int? fixedAge = null, bool forceNoGear = false)
		{
			FloatRange floatRange = ageDaysRange ?? new FloatRange(3f, 600f);
			int randomInRange = countRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				int deadTicks = Mathf.RoundToInt(floatRange.RandomInRange * 60000f);
				if (room.TryGetRandomCellInRoom(out var cell, 1))
				{
					SpawnCorpse(cell, kind, deadTicks, map, damageType, fixedAge, forceNoGear, killerThing, toolUsed, bloodFilthRange);
				}
			}
		}

		public static Corpse SpawnCorpse(IntVec3 cell, PawnKindDef kind, int deadTicks, Map map, DamageDef damageType, float? fixedAge = null, bool forceNoGear = false, ThingDef killerThing = null, Tool toolUsed = null, IntRange? bloodFilthRange = null, BodyPartDef idealPart = null)
		{
			IntRange countRange = bloodFilthRange ?? new IntRange(1, 5);
			bool forceNoGear2 = forceNoGear;
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: true, allowDowned: true, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: false, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedAge, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: false, dontGiveWeapon: false, onlyUseForcedBackstories: false, -1, 0, forceNoGear2));
			if (!pawn.Dead)
			{
				HealthUtility.SimulateKilled(pawn, damageType, killerThing, toolUsed, idealPart);
			}
			pawn.Corpse.Age = deadTicks;
			pawn.relations.hidePawnRelations = true;
			pawn.Corpse.GetComp<CompRottable>().RotProgress += pawn.Corpse.Age;
			Corpse obj = (Corpse)GenSpawn.Spawn(pawn.Corpse, cell, map);
			if (pawn.RaceProps.BloodDef != null)
			{
				GenSpawn.SpawnIrregularLump(pawn.RaceProps.BloodDef, cell, map, countRange, new IntRange(2, 4));
			}
			obj.TrySetForbidden(value: true);
			return obj;
		}

		public static bool IsGoodForDoor(IntVec3 p, Map map)
		{
			if (!IsGoodForHorizontalDoor(p, map))
			{
				return IsGoodForVerticalDoor(p, map);
			}
			return true;
		}

		public static bool IsGoodForHorizontalDoor(IntVec3 p, Map map)
		{
			if (IsWallAt(p + IntVec3.West, map) && IsWallAt(p + IntVec3.East, map) && !IsWallAt(p + IntVec3.North, map))
			{
				return !IsWallAt(p + IntVec3.South, map);
			}
			return false;
		}

		public static bool IsGoodForVerticalDoor(IntVec3 p, Map map)
		{
			if (IsWallAt(p + IntVec3.North, map) && IsWallAt(p + IntVec3.South, map) && !IsWallAt(p + IntVec3.East, map))
			{
				return !IsWallAt(p + IntVec3.West, map);
			}
			return false;
		}

		public static bool IsWallAt(IntVec3 position, Map map)
		{
			return position.GetEdifice(map)?.def.IsWall ?? false;
		}

		[Obsolete]
		public static Queue<CellRect> SubdividedIntoRooms(Map map, CellRect rect, int innerWallsContract = 3, int minRoomSize = 5, int wallMoveRange = 1, int maxRooms = 2, ThingDef wall = null, ThingDef wallStuff = null)
		{
			return SubdividedIntoRooms_NewTemp(map, rect.ContractedBy(innerWallsContract), minRoomSize, wallMoveRange, maxRooms, wall, wallStuff);
		}

		public static Queue<CellRect> SubdividedIntoRooms_NewTemp(Map map, CellRect rect, int minRoomSize = 5, int wallMoveRange = 1, int maxRooms = 2, ThingDef wall = null, ThingDef wallStuff = null)
		{
			int a = Mathf.FloorToInt(((float)rect.Width - 2f) / (float)minRoomSize);
			int a2 = Mathf.FloorToInt(((float)rect.Height - 2f) / (float)minRoomSize);
			int num = Rand.Range(0, Mathf.Min(a, maxRooms));
			int num2 = Rand.Range(0, Mathf.Min(a2, maxRooms));
			Queue<CellRect> queue = new Queue<CellRect>();
			queue.Enqueue(rect);
			int num3 = -1;
			int num4 = -1;
			for (int i = 0; i < num; i++)
			{
				(CellRect left, CellRect right) tuple = queue.Dequeue().SplitHorizontal();
				CellRect item = tuple.left;
				CellRect item2 = tuple.right;
				int num5 = Rand.RangeInclusive(-wallMoveRange, wallMoveRange);
				num3 = item.maxX + num5;
				item.maxX += num5 - 1;
				item2.minX += num5 + 1;
				queue.Enqueue(item);
				queue.Enqueue(item2);
			}
			for (int j = 0; j < num2; j++)
			{
				int count = queue.Count;
				for (int k = 0; k < count; k++)
				{
					(CellRect bottom, CellRect up) tuple2 = queue.Dequeue().SplitVertical();
					CellRect item3 = tuple2.bottom;
					CellRect item4 = tuple2.up;
					int num6 = Rand.RangeInclusive(-wallMoveRange, wallMoveRange);
					num4 = item3.maxZ + num6;
					item3.maxZ += num6 - 1;
					item4.minZ += num6 + 1;
					queue.Enqueue(item3);
					queue.Enqueue(item4);
				}
			}
			if (num3 >= 0)
			{
				for (int l = rect.minZ; l <= rect.maxZ; l++)
				{
					ThingDef obj = wall ?? ThingDefOf.Wall;
					GenSpawn.Spawn(ThingMaker.MakeThing(obj, obj.MadeFromStuff ? (wallStuff ?? ThingDefOf.Steel) : null), new IntVec3(num3, 0, l), map);
				}
			}
			if (num4 >= 0)
			{
				for (int m = rect.minX; m <= rect.maxX; m++)
				{
					ThingDef obj2 = wall ?? ThingDefOf.Wall;
					GenSpawn.Spawn(ThingMaker.MakeThing(obj2, obj2.MadeFromStuff ? (wallStuff ?? ThingDefOf.Steel) : null), new IntVec3(m, 0, num4), map);
				}
			}
			return queue;
		}

		public static Building_Crate SpawnHermeticCrate(IntVec3 cell, Map map, ThingSetMakerDef setMaker, bool addRewards = true, string signal = null)
		{
			return SpawnCrate(ThingDefOf.AncientHermeticCrate, cell, map, Rot4.South, setMaker, addRewards, signal);
		}

		public static Building_Crate SpawnHermeticCrate(IntVec3 cell, Map map, Rot4 rot, ThingSetMakerDef setMaker, bool addRewards = true, string signal = null)
		{
			return SpawnCrate(ThingDefOf.AncientHermeticCrate, cell, map, rot, setMaker, addRewards, signal);
		}

		public static Building_Crate SpawnCrate(ThingDef crateDef, IntVec3 cell, Map map, ThingSetMakerDef rewardMaker, bool addRewards = true, string signal = null)
		{
			return SpawnCrate(crateDef, cell, map, Rot4.South, rewardMaker, addRewards, signal);
		}

		public static Building_Crate SpawnCrate(ThingDef crateDef, IntVec3 cell, Map map, Rot4 rot, ThingSetMakerDef rewardMaker, bool addRewards = true, string signal = null)
		{
			Building_Crate building_Crate = (Building_Crate)GenSpawn.Spawn(ThingMaker.MakeThing(crateDef), cell, map, rot);
			if (addRewards)
			{
				List<Thing> list = rewardMaker.root.Generate(default(ThingSetMakerParams));
				for (int num = list.Count - 1; num >= 0; num--)
				{
					Thing thing = list[num];
					if (!building_Crate.TryAcceptThing(thing, allowSpecialEffects: false))
					{
						thing.Destroy();
					}
				}
				if (!string.IsNullOrEmpty(signal))
				{
					building_Crate.openedSignal = signal;
				}
			}
			else
			{
				building_Crate.Open();
			}
			return building_Crate;
		}

		public static void SpawnHermeticCrateInRoom(LayoutRoom room, Map map, ThingSetMakerDef rewardMaker, bool addRewards = true, string signal = null, Rot4? rot = null)
		{
			SpawnCratesInRoom(ThingDefOf.AncientHermeticCrate, room, map, rewardMaker, IntRange.One, addRewards, signal, rot);
		}

		public static void SpawnHermeticCratesInRoom(LayoutRoom room, Map map, ThingSetMakerDef rewardMaker, IntRange countRange, bool addRewards = true, string signal = null, Rot4? rot = null)
		{
			SpawnCratesInRoom(ThingDefOf.AncientHermeticCrate, room, map, rewardMaker, countRange, addRewards, signal, rot);
		}

		public static void SpawnCrateInRoom(ThingDef crateDef, LayoutRoom room, Map map, ThingSetMakerDef rewardMaker, bool addRewards = true, string signal = null, Rot4? rot = null)
		{
			SpawnCratesInRoom(crateDef, room, map, rewardMaker, IntRange.One, addRewards, signal, rot);
		}

		public static void SpawnCratesInRoom(ThingDef crateDef, LayoutRoom room, Map map, ThingSetMakerDef rewardMaker, IntRange countRange, bool addRewards = true, string signal = null, Rot4? rot = null)
		{
			int num = countRange.RandomInRange;
			Rot4 rotation = rot ?? Rot4.South;
			IntVec3 cell;
			while (num > 0 && room.TryGetRandomCellInRoom(out cell, 1, Validator))
			{
				SpawnCrate(crateDef, cell, map, rotation, rewardMaker, addRewards, signal);
				num--;
			}
			bool Validator(IntVec3 pos)
			{
				return IsValidCrateCell(crateDef, pos, map, rotation);
			}
		}

		public static bool IsValidCrateCell(ThingDef crate, IntVec3 pos, Map map, Rot4 rot)
		{
			CellRect cellRect = pos.RectAbout(crate.Size, rot);
			foreach (IntVec3 cell in cellRect.Cells)
			{
				if (cell.GetEdifice(map) != null)
				{
					return false;
				}
			}
			foreach (IntVec3 edgeCell in cellRect.EdgeCells)
			{
				foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(edgeCell))
				{
					if (!cellRect.Contains(item) && item.GetEdifice(map) != null)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static bool IsClearAndNotAdjacentToDoor(ThingDef def, IntVec3 pos, Map map, Rot4 rot)
		{
			return IsClearAndNotAdjacentToDoor(pos.RectAbout(def.Size, rot), map);
		}

		public static bool IsClearAndNotAdjacentToDoor(CellRect rect, Map map)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				if (cell.GetEdifice(map) != null)
				{
					return false;
				}
			}
			foreach (IntVec3 edgeCell in rect.EdgeCells)
			{
				foreach (IntVec3 item in GenAdjFast.AdjacentCellsCardinal(edgeCell))
				{
					if (!rect.Contains(item) && item.GetDoor(map) != null)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static void SpawnDoorBarricades(ThingDef def, LayoutRoom room, Map map, float chancePerDoor = 0.75f, ThingDef stuff = null, int steps = 2, int offset = 2)
		{
			List<(IntVec3, Rot4)> list = new List<(IntVec3, Rot4)>();
			foreach (CellRect rect in room.rects)
			{
				foreach (IntVec3 edgeCell in rect.EdgeCells)
				{
					if (edgeCell.GetDoor(map) != null)
					{
						list.Add((edgeCell, rect.GetEdgeCellRot(edgeCell)));
					}
				}
			}
			foreach (var (intVec, rot) in list)
			{
				if (Rand.Chance(chancePerDoor))
				{
					IntVec3 intVec2 = intVec + rot.Opposite.FacingCell * offset;
					SpawnBarricade(intVec2);
					for (int i = 1; i < steps; i++)
					{
						Rot4 rot2 = rot.Rotated(RotationDirection.Clockwise);
						SpawnBarricade(intVec2 + rot2.FacingCell * i);
						SpawnBarricade(intVec2 + rot2.FacingCell * -i);
					}
				}
			}
			void SpawnBarricade(IntVec3 pos)
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(def, stuff), pos, map);
			}
		}

		public static void TryPlaceInRandomCorner(Map map, LayoutRoom room, ThingDef thingDef, Faction faction, ThingDef stuff = null)
		{
			List<IntVec3> list = room.rects.SelectMany((CellRect r) => r.ContractedBy(1).Corners).Where(Predicate).ToList();
			if (list.Count != 0)
			{
				IntVec3 loc = list.RandomElement();
				Thing thing = ThingMaker.MakeThing(thingDef, stuff);
				thing.SetFactionDirect(faction);
				GenSpawn.Spawn(thing, loc, map);
			}
			bool Predicate(IntVec3 c)
			{
				if (c.GetEdifice(map) != null)
				{
					return false;
				}
				for (int i = 0; i < 4; i++)
				{
					if ((c + GenAdj.AdjacentCells[i]).GetFirstThing<Building_Trap>(map) != null)
					{
						return false;
					}
				}
				return true;
			}
		}
	}
}
