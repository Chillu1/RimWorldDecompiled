using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PrefabUtility
	{
		private const int MaximumDepth = 15;

		private static readonly List<(PrefabThingData, IntVec3)> tmpAttachments = new List<(PrefabThingData, IntVec3)>();

		public static bool CanSpawnPrefab(PrefabDef prefab, Map map, IntVec3 pos, Rot4 rot, bool canWipeEdifices = true, int recursionDepth = 0)
		{
			if (recursionDepth >= 15)
			{
				return false;
			}
			rot = ValidateRotation(prefab, rot);
			IntVec3 root = GetRoot(prefab, pos, rot);
			foreach (var thing in prefab.GetThings())
			{
				PrefabThingData item = thing.data;
				IntVec3 item2 = thing.cell;
				IntVec3 adjustedThingLocalPosition = GetAdjustedThingLocalPosition(item, rot, item2);
				if ((item.def.building == null || !item.def.building.isAttachment) && !CanSpawnThing(item, map, root + adjustedThingLocalPosition, rot.Rotated(item.relativeRotation), canWipeEdifices))
				{
					return false;
				}
			}
			foreach (var prefab2 in prefab.GetPrefabs())
			{
				SubPrefabData item3 = prefab2.data;
				IntVec3 item4 = prefab2.cell;
				IntVec3 adjustedPrefabLocalPosition = GetAdjustedPrefabLocalPosition(item3, rot, item4);
				if (!CanSpawnPrefab(item3.def, map, root + adjustedPrefabLocalPosition, rot.Rotated(item3.relativeRotation), canWipeEdifices, ++recursionDepth))
				{
					return false;
				}
			}
			return true;
		}

		public static void SpawnPrefab(PrefabDef prefab, Map map, IntVec3 pos, Rot4 rot, Faction faction = null, List<Thing> spawned = null, Func<PrefabThingData, Tuple<ThingDef, ThingDef>> overrideSpawnData = null, Action<Thing> onSpawned = null, bool blueprint = false)
		{
			rot = ValidateRotation(prefab, rot);
			IntVec3 root = GetRoot(prefab, pos, rot);
			foreach (var (prefabTerrainData, local) in prefab.GetTerrain())
			{
				if (Rand.Chance(prefabTerrainData.chance))
				{
					IntVec3 adjustedLocalPosition = GetAdjustedLocalPosition(local, rot);
					IntVec3 c = root + adjustedLocalPosition;
					map.terrainGrid.SetTerrain(c, prefabTerrainData.def);
					if (prefabTerrainData.color != null)
					{
						map.terrainGrid.SetTerrainColor(c, prefabTerrainData.color);
					}
				}
			}
			foreach (var (prefabThingData, cell) in prefab.GetThings())
			{
				if (Rand.Chance(prefabThingData.chance))
				{
					IntVec3 adjustedThingLocalPosition = GetAdjustedThingLocalPosition(prefabThingData, rot, cell);
					IntVec3 intVec = root + adjustedThingLocalPosition;
					BuildingProperties building = prefabThingData.def.building;
					if (building != null && building.isAttachment)
					{
						tmpAttachments.Add((prefabThingData, intVec));
					}
					else
					{
						SpawnThing(prefabThingData, map, intVec, rot.Rotated(prefabThingData.relativeRotation), faction, spawned, overrideSpawnData, onSpawned, blueprint);
					}
				}
			}
			foreach (var tmpAttachment in tmpAttachments)
			{
				PrefabThingData item = tmpAttachment.Item1;
				IntVec3 item2 = tmpAttachment.Item2;
				Rot4 rotation = rot.Rotated(item.relativeRotation);
				if (item2.InBounds(map) && (RoomGenUtility.CanPlace(item.def, item2, map, rotation) || RoomGenUtility.CanPlaceWallAttachment(item.def, item2, map, out rotation, avoidEdifices: false)))
				{
					SpawnThing(item, map, item2, rotation, faction, spawned, overrideSpawnData, onSpawned);
				}
			}
			tmpAttachments.Clear();
			foreach (var (subPrefabData, cell2) in prefab.GetPrefabs())
			{
				if (Rand.Chance(subPrefabData.chance))
				{
					IntVec3 adjustedPrefabLocalPosition = GetAdjustedPrefabLocalPosition(subPrefabData, rot, cell2);
					SpawnPrefab(subPrefabData.def, map, root + adjustedPrefabLocalPosition, rot.Rotated(subPrefabData.relativeRotation), faction, spawned, overrideSpawnData, onSpawned, blueprint);
				}
			}
		}

		public static IntVec3 GetAdjustedThingLocalPosition(ThingDef def, Rot4 rot, IntVec3 cell)
		{
			IntVec3 center = GetAdjustedLocalPosition(cell, rot);
			IntVec2 size = def.size;
			if (!def.rotatable)
			{
				GenAdj.AdjustForRotation(ref center, ref size, def.defaultPlacingRot, rot);
			}
			return center;
		}

		public static IntVec3 GetAdjustedThingLocalPosition(PrefabThingData data, Rot4 rot, IntVec3 cell)
		{
			return GetAdjustedThingLocalPosition(data.def, rot, cell);
		}

		public static IntVec3 GetAdjustedPrefabLocalPosition(SubPrefabData data, Rot4 rot, IntVec3 cell)
		{
			IntVec3 center = GetAdjustedLocalPosition(cell, rot);
			IntVec2 size = data.def.size;
			GenAdj.AdjustForRotation(ref center, ref size, rot);
			return center;
		}

		public static IEnumerable<(PrefabThingData data, IntVec3, Rot4)> GetThings(PrefabDef prefab, IntVec3 pos, Rot4 rot)
		{
			rot = ValidateRotation(prefab, rot);
			IntVec3 root = GetRoot(prefab, pos, rot);
			foreach (var thing in prefab.GetThings())
			{
				PrefabThingData item = thing.data;
				IntVec3 item2 = thing.cell;
				IntVec3 adjustedThingLocalPosition = GetAdjustedThingLocalPosition(item, rot, item2);
				yield return (data: item, root + adjustedThingLocalPosition, (!item.def.rotatable) ? Rot4.North : rot.Rotated(item.relativeRotation));
			}
			foreach (var prefab2 in prefab.GetPrefabs())
			{
				SubPrefabData item3 = prefab2.data;
				IntVec3 item4 = prefab2.cell;
				IntVec3 adjustedPrefabLocalPosition = GetAdjustedPrefabLocalPosition(item3, rot, item4);
				foreach (var thing2 in GetThings(item3.def, root + adjustedPrefabLocalPosition, rot.Rotated(item3.relativeRotation)))
				{
					yield return thing2;
				}
			}
		}

		public static IntVec3 GetRoot(PrefabDef prefab, IntVec3 pos, Rot4 rot)
		{
			return GetRoot(pos, prefab.size, rot);
		}

		public static IntVec3 GetRoot(IntVec3 pos, IntVec2 size, Rot4 rot)
		{
			return GetRoot(GenAdj.OccupiedRect(pos, rot, size), rot);
		}

		public static IntVec3 GetRoot(CellRect rect, Rot4 rot)
		{
			rect.GetInternalCorners(out var BL, out var TL, out var TR, out var BR);
			IntVec3 result = BL;
			if (rot == Rot4.East)
			{
				result = TL;
			}
			else if (rot == Rot4.South)
			{
				result = TR;
			}
			else if (rot == Rot4.West)
			{
				result = BR;
			}
			return result;
		}

		private static void SpawnThing(PrefabThingData data, Map map, IntVec3 cell, Rot4 rot, Faction faction = null, List<Thing> spawned = null, Func<PrefabThingData, Tuple<ThingDef, ThingDef>> overrideSpawnData = null, Action<Thing> onSpawned = null, bool blueprint = false)
		{
			bool flag = data.def.IsBlueprint || blueprint;
			ThingDef thingDef = (data.def.IsBlueprint ? ((ThingDef)data.def.entityDefToBuild) : data.def);
			ThingDef thingDef2 = (data.def.IsBlueprint ? data.def : data.def.blueprintDef);
			rot = (thingDef.rotatable ? rot : thingDef.defaultPlacingRot);
			if ((flag && thingDef2 == null) || (flag && !GenConstruct.CanPlaceBlueprintAt(thingDef, cell, rot, map, godMode: false, null, null, data.stuff)) || (!flag && !GenSpawn.CanSpawnAt(thingDef, cell, map, rot)))
			{
				return;
			}
			ThingDef stuff = data.stuff;
			if (data.canOverrideData && overrideSpawnData != null)
			{
				Tuple<ThingDef, ThingDef> tuple = overrideSpawnData(data);
				if (tuple != null)
				{
					thingDef = tuple.Item1;
					stuff = tuple.Item2;
				}
			}
			Thing thing = null;
			if (flag)
			{
				if (GenSpawn.CanSpawnAt(thingDef.blueprintDef, cell, map, rot, canWipeEdifices: false))
				{
					thing = GenConstruct.PlaceBlueprintForBuild(thingDef, cell, map, rot, Faction.OfPlayer, stuff);
				}
			}
			else
			{
				thing = ThingMaker.MakeThing(thingDef, stuff);
				if (thing.def.CanHaveFaction)
				{
					thing.SetFaction(faction);
				}
				thing.stackCount = data.stackCountRange.RandomInRange;
				if (data.hp > 0)
				{
					thing.HitPoints = data.hp;
				}
				CompColorable comp;
				if (data.colorDef != null && thing is Building building)
				{
					building.ChangePaint(data.colorDef);
				}
				else if (data.color != default(Color) && thing.TryGetComp(out comp))
				{
					comp.SetColor(data.color);
				}
				if (data.quality.HasValue && thing.TryGetComp(out CompQuality comp2))
				{
					comp2.SetQuality(data.quality.Value, ArtGenerationContext.Outsider);
				}
				GenSpawn.Spawn(thing, cell, map, rot);
			}
			if (thing != null)
			{
				spawned?.Add(thing);
				onSpawned?.Invoke(thing);
			}
		}

		private static bool CanSpawnThing(PrefabThingData data, Map map, IntVec3 cell, Rot4 rot, bool canWipeEdifices = true)
		{
			rot = (data.def.rotatable ? rot : Rot4.North);
			return GenSpawn.CanSpawnAt(data.def, cell, map, rot, canWipeEdifices);
		}

		public static IntVec3 GetAdjustedLocalPosition(IntVec3 local, Rot4 rot)
		{
			if (rot == Rot4.East)
			{
				ref int x = ref local.x;
				ref int z = ref local.z;
				int z2 = local.z;
				int num = -local.x;
				x = z2;
				z = num;
			}
			else if (rot == Rot4.South)
			{
				ref int x = ref local.x;
				ref int z3 = ref local.z;
				int num = -local.x;
				int z2 = -local.z;
				x = num;
				z3 = z2;
			}
			else if (rot == Rot4.West)
			{
				ref int x = ref local.x;
				ref int z4 = ref local.z;
				int z2 = -local.z;
				int num = local.x;
				x = z2;
				z4 = num;
			}
			return local;
		}

		public static Rot4 ValidateRotation(PrefabDef prefab, Rot4 rot)
		{
			if (!prefab.rotations.HasFlag((RotEnum)rot))
			{
				for (int i = 0; i < 4; i++)
				{
					if (prefab.rotations.HasFlag((RotEnum)i))
					{
						rot = new Rot4(i);
						break;
					}
				}
			}
			return rot;
		}

		public static PrefabDef CreatePrefab(CellRect rect, bool copyAllThings = false, bool copyTerrain = false)
		{
			HashSet<Thing> hashSet = new HashSet<Thing>();
			HashSet<Thing> group = new HashSet<Thing>();
			PrefabDef prefabDef = new PrefabDef();
			List<Thing> list = (from t in rect.Cells.SelectMany((IntVec3 c) => from thing3 in c.GetThingList(Find.CurrentMap)
					where thing3 is Building || copyAllThings
					select thing3)
				orderby t is Building descending, t.def.shortHash descending
				select t).ToList();
			if (list.Count > 0)
			{
				for (int num = 0; num < list.Count; num++)
				{
					Thing thing = list[num];
					group.Clear();
					if (!hashSet.Add(thing))
					{
						continue;
					}
					for (int num2 = num + 1; num2 < list.Count; num2++)
					{
						Thing thing2 = list[num2];
						if (!hashSet.Contains(thing2) && !group.Contains(thing2) && thing2.def == thing.def && thing2.stackCount == thing.stackCount && thing2.Rotation == thing.Rotation && thing2.HitPoints == thing.HitPoints && thing2.Stuff == thing.Stuff && (!(thing2 is Building building) || !(thing is Building building2) || building.PaintColorDef == building2.PaintColorDef) && (!thing2.TryGetComp(out CompColorable comp) || !thing.TryGetComp(out CompColorable comp2) || comp == comp2) && (!thing2.TryGetQuality(out var qc) || !thing.TryGetQuality(out var qc2) || qc == qc2))
						{
							if (!group.Any())
							{
								group.Add(thing);
							}
							hashSet.Add(thing2);
							group.Add(thing2);
						}
					}
					IntVec3 intVec = thing.Position - rect.Min;
					RotationDirection relativeRotation = RotationDirection.None;
					if (thing.Rotation == Rot4.East)
					{
						relativeRotation = RotationDirection.Clockwise;
					}
					else if (thing.Rotation == Rot4.South)
					{
						relativeRotation = RotationDirection.Opposite;
					}
					else if (thing.Rotation == Rot4.West)
					{
						relativeRotation = RotationDirection.Counterclockwise;
					}
					PrefabThingData prefabThingData = new PrefabThingData
					{
						def = thing.def,
						relativeRotation = relativeRotation,
						stuff = thing.Stuff
					};
					if (thing.TryGetComp(out CompQuality comp3))
					{
						prefabThingData.quality = comp3.Quality;
					}
					CompColorable comp4;
					if (thing is Building building3)
					{
						prefabThingData.colorDef = building3.PaintColorDef;
					}
					else if (thing.TryGetComp(out comp4) && prefabThingData.color != Color.white)
					{
						prefabThingData.color = comp4.Color;
					}
					if (group.Any())
					{
						if (thing.def.Size == IntVec2.One)
						{
							prefabThingData.rects = new List<CellRect>();
							foreach (CellRect item3 in rect.EnumerateRectanglesCovering(Validator))
							{
								prefabThingData.rects.Add(item3.MovedBy(-rect.Min));
							}
						}
						else
						{
							prefabThingData.positions = new List<IntVec3>();
							foreach (Thing item4 in group)
							{
								prefabThingData.positions.Add(item4.Position - rect.Min);
							}
						}
					}
					else if (intVec != IntVec3.Zero)
					{
						prefabThingData.position = intVec;
					}
					if (thing.def.useHitPoints && thing.HitPoints != thing.MaxHitPoints)
					{
						prefabThingData.hp = thing.HitPoints;
					}
					if (thing.stackCount != 1)
					{
						prefabThingData.stackCountRange = new IntRange(thing.stackCount, thing.stackCount);
					}
					prefabDef.things.Add(prefabThingData);
				}
			}
			if (copyTerrain)
			{
				Dictionary<(TerrainDef, ColorDef), List<IntVec3>> dictionary = new Dictionary<(TerrainDef, ColorDef), List<IntVec3>>();
				TerrainGrid terrainGrid = Find.CurrentMap.terrainGrid;
				foreach (IntVec3 cell in rect.Cells)
				{
					TerrainDef terrain = cell.GetTerrain(Find.CurrentMap);
					(TerrainDef, ColorDef) key = (terrain, terrainGrid.ColorAt(cell));
					if (!dictionary.TryGetValue(key, out var value))
					{
						value = (dictionary[key] = new List<IntVec3>());
					}
					value.Add(cell);
				}
				foreach (KeyValuePair<(TerrainDef, ColorDef), List<IntVec3>> item5 in dictionary)
				{
					item5.Deconstruct(out var key2, out var value2);
					(TerrainDef, ColorDef) tuple = key2;
					TerrainDef item = tuple.Item1;
					ColorDef item2 = tuple.Item2;
					List<IntVec3> list3 = value2;
					PrefabTerrainData prefabTerrainData = new PrefabTerrainData
					{
						def = item,
						color = item2,
						rects = new List<CellRect>()
					};
					foreach (CellRect item6 in rect.EnumerateRectanglesCovering(Validator2))
					{
						prefabTerrainData.rects.Add(item6.MovedBy(-rect.Min));
					}
					prefabDef.terrain.Add(prefabTerrainData);
					bool Validator2(IntVec3 index)
					{
						return list3.Contains(index);
					}
				}
			}
			return prefabDef;
			bool Validator(IntVec3 index)
			{
				return group.Any((Thing t) => t.Position == index);
			}
		}
	}
}
