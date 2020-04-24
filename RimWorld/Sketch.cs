using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Sketch : IExposable
	{
		public enum SpawnPosType
		{
			Unchanged,
			OccupiedCenter,
			OccupiedBotLeft
		}

		public enum SpawnMode
		{
			Blueprint,
			Normal,
			TransportPod
		}

		private List<SketchEntity> entities = new List<SketchEntity>();

		private List<SketchThing> cachedThings = new List<SketchThing>();

		private List<SketchTerrain> cachedTerrain = new List<SketchTerrain>();

		private List<SketchBuildable> cachedBuildables = new List<SketchBuildable>();

		private Dictionary<IntVec3, SketchTerrain> terrainAt = new Dictionary<IntVec3, SketchTerrain>();

		private Dictionary<IntVec3, SketchThing> edificeAt = new Dictionary<IntVec3, SketchThing>();

		private Dictionary<IntVec3, SketchThing> thingsAt_single = new Dictionary<IntVec3, SketchThing>();

		private Dictionary<IntVec3, List<SketchThing>> thingsAt_multiple = new Dictionary<IntVec3, List<SketchThing>>();

		private bool occupiedRectDirty = true;

		private CellRect cachedOccupiedRect;

		private bool floodFillWorking;

		private Queue<IntVec3> floodFillOpenSet;

		private Dictionary<IntVec3, int> floodFillTraversalDistance;

		public const float SpawnOrder_Terrain = 1f;

		public const float SpawnOrder_Thing = 2f;

		private static readonly List<SketchThing> EmptySketchThingList = new List<SketchThing>();

		private static readonly Color GhostColor = new Color(0.7f, 0.7f, 0.7f, 0.35f);

		private static readonly Color BlockedColor = new Color(0.8f, 0.2f, 0.2f, 0.35f);

		private static HashSet<IntVec3> tmpSuggestedRoofCellsVisited = new HashSet<IntVec3>();

		private static List<IntVec3> tmpSuggestedRoofCells = new List<IntVec3>();

		private static HashSet<IntVec3> tmpYieldedSuggestedRoofCells = new HashSet<IntVec3>();

		private static List<SketchThing> tmpToRemove = new List<SketchThing>();

		public List<SketchEntity> Entities => entities;

		public List<SketchThing> Things => cachedThings;

		public List<SketchTerrain> Terrain => cachedTerrain;

		public List<SketchBuildable> Buildables => cachedBuildables;

		public CellRect OccupiedRect
		{
			get
			{
				if (occupiedRectDirty)
				{
					cachedOccupiedRect = CellRect.Empty;
					bool flag = false;
					for (int i = 0; i < entities.Count; i++)
					{
						if (!flag)
						{
							cachedOccupiedRect = entities[i].OccupiedRect;
							flag = true;
						}
						else
						{
							CellRect occupiedRect = entities[i].OccupiedRect;
							cachedOccupiedRect = CellRect.FromLimits(Mathf.Min(cachedOccupiedRect.minX, occupiedRect.minX), Mathf.Min(cachedOccupiedRect.minZ, occupiedRect.minZ), Mathf.Max(cachedOccupiedRect.maxX, occupiedRect.maxX), Mathf.Max(cachedOccupiedRect.maxZ, occupiedRect.maxZ));
						}
					}
					occupiedRectDirty = false;
				}
				return cachedOccupiedRect;
			}
		}

		public IntVec2 OccupiedSize => new IntVec2(OccupiedRect.Width, OccupiedRect.Height);

		public IntVec3 OccupiedCenter => OccupiedRect.CenterCell;

		public bool Empty => !Entities.Any();

		public bool Add(SketchEntity entity, bool wipeIfCollides = true)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}
			if (entities.Contains(entity))
			{
				return true;
			}
			if (wipeIfCollides)
			{
				WipeColliding(entity);
			}
			else if (WouldCollide(entity))
			{
				return false;
			}
			SketchTerrain sketchTerrain = entity as SketchTerrain;
			if (sketchTerrain != null && terrainAt.TryGetValue(sketchTerrain.pos, out SketchTerrain value))
			{
				Remove(value);
			}
			SketchBuildable sketchBuildable = entity as SketchBuildable;
			if (sketchBuildable != null)
			{
				for (int num = cachedBuildables.Count - 1; num >= 0; num--)
				{
					if (sketchBuildable.OccupiedRect.Overlaps(cachedBuildables[num].OccupiedRect) && GenSpawn.SpawningWipes(sketchBuildable.Buildable, cachedBuildables[num].Buildable))
					{
						Remove(cachedBuildables[num]);
					}
				}
			}
			entities.Add(entity);
			AddToCache(entity);
			return true;
		}

		public bool AddThing(ThingDef def, IntVec3 pos, Rot4 rot, ThingDef stuff = null, int stackCount = 1, QualityCategory? quality = null, int? hitPoints = null, bool wipeIfCollides = true)
		{
			SketchThing sketchThing = new SketchThing();
			sketchThing.def = def;
			sketchThing.stuff = stuff;
			sketchThing.pos = pos;
			sketchThing.rot = rot;
			sketchThing.stackCount = stackCount;
			sketchThing.quality = quality;
			sketchThing.hitPoints = hitPoints;
			return Add(sketchThing, wipeIfCollides);
		}

		public bool AddTerrain(TerrainDef def, IntVec3 pos, bool wipeIfCollides = true)
		{
			SketchTerrain sketchTerrain = new SketchTerrain();
			sketchTerrain.def = def;
			sketchTerrain.pos = pos;
			return Add(sketchTerrain, wipeIfCollides);
		}

		public bool Remove(SketchEntity entity)
		{
			if (entity == null)
			{
				return false;
			}
			if (!entities.Contains(entity))
			{
				return false;
			}
			entities.Remove(entity);
			RemoveFromCache(entity);
			return true;
		}

		public bool RemoveTerrain(IntVec3 cell)
		{
			if (terrainAt.TryGetValue(cell, out SketchTerrain value))
			{
				return Remove(value);
			}
			return false;
		}

		public void Clear()
		{
			entities.Clear();
			RecacheAll();
		}

		public TerrainDef TerrainAt(IntVec3 pos)
		{
			return SketchTerrainAt(pos)?.def;
		}

		public SketchTerrain SketchTerrainAt(IntVec3 pos)
		{
			if (!terrainAt.TryGetValue(pos, out SketchTerrain value))
			{
				return null;
			}
			return value;
		}

		public bool AnyTerrainAt(int x, int z)
		{
			return AnyTerrainAt(new IntVec3(x, 0, z));
		}

		public bool AnyTerrainAt(IntVec3 pos)
		{
			return TerrainAt(pos) != null;
		}

		public IEnumerable<SketchThing> ThingsAt(IntVec3 pos)
		{
			if (thingsAt_single.TryGetValue(pos, out SketchThing value))
			{
				return Gen.YieldSingle(value);
			}
			if (thingsAt_multiple.TryGetValue(pos, out List<SketchThing> value2))
			{
				return value2;
			}
			return EmptySketchThingList;
		}

		public void ThingsAt(IntVec3 pos, out SketchThing singleResult, out List<SketchThing> multipleResults)
		{
			List<SketchThing> value2;
			if (thingsAt_single.TryGetValue(pos, out SketchThing value))
			{
				singleResult = value;
				multipleResults = null;
			}
			else if (thingsAt_multiple.TryGetValue(pos, out value2))
			{
				singleResult = null;
				multipleResults = value2;
			}
			else
			{
				singleResult = null;
				multipleResults = null;
			}
		}

		public SketchThing EdificeAt(IntVec3 pos)
		{
			if (edificeAt.TryGetValue(pos, out SketchThing value))
			{
				return value;
			}
			return null;
		}

		public bool WouldCollide(SketchEntity entity)
		{
			if (entities.Contains(entity))
			{
				return false;
			}
			SketchThing sketchThing = entity as SketchThing;
			if (sketchThing != null)
			{
				return WouldCollide(sketchThing.def, sketchThing.pos, sketchThing.rot);
			}
			SketchTerrain sketchTerrain = entity as SketchTerrain;
			if (sketchTerrain != null)
			{
				return WouldCollide(sketchTerrain.def, sketchTerrain.pos);
			}
			return false;
		}

		public bool WouldCollide(ThingDef def, IntVec3 pos, Rot4 rot)
		{
			CellRect cellRect = GenAdj.OccupiedRect(pos, rot, def.size);
			if (def.terrainAffordanceNeeded != null)
			{
				foreach (IntVec3 item in cellRect)
				{
					TerrainDef terrainDef = TerrainAt(item);
					if (terrainDef != null && !terrainDef.affordances.Contains(def.terrainAffordanceNeeded))
					{
						return true;
					}
				}
			}
			for (int i = 0; i < cachedThings.Count; i++)
			{
				if (!cellRect.Overlaps(cachedThings[i].OccupiedRect))
				{
					continue;
				}
				if (def.race != null)
				{
					if (cachedThings[i].def.passability == Traversability.Impassable)
					{
						return true;
					}
				}
				else if (!GenConstruct.CanPlaceBlueprintOver(def, cachedThings[i].def))
				{
					return true;
				}
			}
			return false;
		}

		public bool WouldCollide(TerrainDef def, IntVec3 pos)
		{
			if (!def.layerable && TerrainAt(pos) != null)
			{
				return true;
			}
			for (int i = 0; i < cachedThings.Count; i++)
			{
				if (cachedThings[i].OccupiedRect.Contains(pos) && cachedThings[i].def.terrainAffordanceNeeded != null && !def.affordances.Contains(cachedThings[i].def.terrainAffordanceNeeded))
				{
					return true;
				}
			}
			return false;
		}

		public void WipeColliding(SketchEntity entity)
		{
			if (!WouldCollide(entity))
			{
				return;
			}
			SketchThing sketchThing = entity as SketchThing;
			if (sketchThing != null)
			{
				WipeColliding(sketchThing.def, sketchThing.pos, sketchThing.rot);
				return;
			}
			SketchTerrain sketchTerrain = entity as SketchTerrain;
			if (sketchTerrain != null)
			{
				WipeColliding(sketchTerrain.def, sketchTerrain.pos);
			}
		}

		public void WipeColliding(ThingDef def, IntVec3 pos, Rot4 rot)
		{
			if (!WouldCollide(def, pos, rot))
			{
				return;
			}
			CellRect cellRect = GenAdj.OccupiedRect(pos, rot, def.size);
			if (def.terrainAffordanceNeeded != null)
			{
				foreach (IntVec3 item in cellRect)
				{
					TerrainDef terrainDef = TerrainAt(item);
					if (terrainDef != null && !terrainDef.affordances.Contains(def.terrainAffordanceNeeded))
					{
						RemoveTerrain(item);
					}
				}
			}
			for (int num = cachedThings.Count - 1; num >= 0; num--)
			{
				if (cellRect.Overlaps(cachedThings[num].OccupiedRect) && !GenConstruct.CanPlaceBlueprintOver(def, cachedThings[num].def))
				{
					Remove(cachedThings[num]);
				}
			}
		}

		public void WipeColliding(TerrainDef def, IntVec3 pos)
		{
			if (!WouldCollide(def, pos))
			{
				return;
			}
			if (!def.layerable && TerrainAt(pos) != null)
			{
				RemoveTerrain(pos);
			}
			for (int num = cachedThings.Count - 1; num >= 0; num--)
			{
				if (cachedThings[num].OccupiedRect.Contains(pos) && cachedThings[num].def.terrainAffordanceNeeded != null && !def.affordances.Contains(cachedThings[num].def.terrainAffordanceNeeded))
				{
					Remove(cachedThings[num]);
				}
			}
		}

		public bool IsSpawningBlocked(Map map, IntVec3 pos, Faction faction, SpawnPosType posType = SpawnPosType.Unchanged)
		{
			IntVec3 offset = GetOffset(pos, posType);
			for (int i = 0; i < entities.Count; i++)
			{
				if (entities[i].IsSpawningBlocked(entities[i].pos + offset, map))
				{
					return true;
				}
			}
			return false;
		}

		public bool AnyThingOutOfBounds(Map map, IntVec3 pos, SpawnPosType posType = SpawnPosType.Unchanged)
		{
			IntVec3 offset = GetOffset(pos, posType);
			for (int i = 0; i < entities.Count; i++)
			{
				SketchThing sketchThing = entities[i] as SketchThing;
				if (sketchThing == null)
				{
					continue;
				}
				if (sketchThing.def.size == IntVec2.One)
				{
					if (!(entities[i].pos + offset).InBounds(map))
					{
						return true;
					}
				}
				else
				{
					foreach (IntVec3 item in sketchThing.OccupiedRect)
					{
						if (!(item + offset).InBounds(map))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public void Spawn(Map map, IntVec3 pos, Faction faction, SpawnPosType posType = SpawnPosType.Unchanged, SpawnMode spawnMode = SpawnMode.Normal, bool wipeIfCollides = false, bool clearEdificeWhereFloor = false, List<Thing> spawnedThings = null, bool dormant = false, bool buildRoofsInstantly = false, Action<IntVec3, SketchEntity> onFailedToSpawnThing = null)
		{
			IntVec3 offset = GetOffset(pos, posType);
			if (clearEdificeWhereFloor)
			{
				for (int i = 0; i < cachedTerrain.Count; i++)
				{
					if (cachedTerrain[i].def.layerable)
					{
						(cachedTerrain[i].pos + offset).GetEdifice(map)?.Destroy();
					}
				}
			}
			foreach (SketchEntity item in entities.OrderBy((SketchEntity x) => x.SpawnOrder))
			{
				IntVec3 intVec = item.pos + offset;
				if (!item.Spawn(intVec, map, faction, spawnMode, wipeIfCollides, spawnedThings, dormant))
				{
					onFailedToSpawnThing?.Invoke(intVec, item);
				}
			}
			if (spawnedThings != null && spawnMode == SpawnMode.TransportPod && !wipeIfCollides)
			{
				bool flag = false;
				for (int j = 0; j < spawnedThings.Count; j++)
				{
					for (int k = j + 1; k < spawnedThings.Count; k++)
					{
						CellRect cellRect = GenAdj.OccupiedRect(spawnedThings[j].Position, spawnedThings[j].Rotation, spawnedThings[j].def.size);
						CellRect other = GenAdj.OccupiedRect(spawnedThings[k].Position, spawnedThings[k].Rotation, spawnedThings[k].def.size);
						if (cellRect.Overlaps(other) && (GenSpawn.SpawningWipes(spawnedThings[j].def, spawnedThings[k].def) || GenSpawn.SpawningWipes(spawnedThings[k].def, spawnedThings[j].def)))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					for (int l = 0; l < spawnedThings.Count; l++)
					{
						ActiveDropPodInfo activeDropPodInfo;
						if ((activeDropPodInfo = (spawnedThings[l].ParentHolder as ActiveDropPodInfo)) != null)
						{
							activeDropPodInfo.spawnWipeMode = null;
						}
					}
				}
			}
			if (buildRoofsInstantly && spawnMode == SpawnMode.Normal)
			{
				foreach (IntVec3 suggestedRoofCell in GetSuggestedRoofCells())
				{
					IntVec3 c = suggestedRoofCell + offset;
					if (c.InBounds(map) && !c.Roofed(map))
					{
						map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
					}
				}
			}
		}

		public void Merge(Sketch other, bool wipeIfCollides = true)
		{
			foreach (SketchEntity item in other.entities.OrderBy((SketchEntity x) => x.SpawnOrder))
			{
				Add(item.DeepCopy(), wipeIfCollides);
			}
		}

		public void MergeAt(Sketch other, IntVec3 pos, SpawnPosType posType = SpawnPosType.Unchanged, bool wipeIfCollides = true)
		{
			Sketch sketch = other.DeepCopy();
			sketch.MoveBy(sketch.GetOffset(pos, posType));
			Merge(sketch, wipeIfCollides);
		}

		public void Subtract(Sketch other)
		{
			for (int i = 0; i < entities.Count; i++)
			{
				for (int j = 0; j < other.entities.Count; j++)
				{
					if (entities[i].SameForSubtracting(other.entities[j]))
					{
						Remove(entities[i]);
						i--;
						break;
					}
				}
			}
		}

		public void MoveBy(IntVec2 by)
		{
			MoveBy(by.ToIntVec3);
		}

		public void MoveBy(IntVec3 by)
		{
			foreach (SketchEntity entity in Entities)
			{
				entity.pos += by;
			}
			RecacheAll();
		}

		public void MoveOccupiedCenterToZero()
		{
			MoveBy(new IntVec3(-OccupiedCenter.x, 0, -OccupiedCenter.z));
		}

		public Sketch DeepCopy()
		{
			Sketch sketch = new Sketch();
			foreach (SketchEntity item in entities.OrderBy((SketchEntity x) => x.SpawnOrder))
			{
				sketch.Add(item.DeepCopy());
			}
			return sketch;
		}

		private void AddToCache(SketchEntity entity)
		{
			occupiedRectDirty = true;
			SketchBuildable sketchBuildable = entity as SketchBuildable;
			if (sketchBuildable != null)
			{
				cachedBuildables.Add(sketchBuildable);
			}
			SketchThing sketchThing = entity as SketchThing;
			if (sketchThing != null)
			{
				if (sketchThing.def.building != null && sketchThing.def.building.isEdifice)
				{
					foreach (IntVec3 item in sketchThing.OccupiedRect)
					{
						edificeAt[item] = sketchThing;
					}
				}
				foreach (IntVec3 item2 in sketchThing.OccupiedRect)
				{
					SketchThing value2;
					if (thingsAt_multiple.TryGetValue(item2, out List<SketchThing> value))
					{
						value.Add(sketchThing);
					}
					else if (thingsAt_single.TryGetValue(item2, out value2))
					{
						thingsAt_single.Remove(item2);
						List<SketchThing> list = new List<SketchThing>();
						list.Add(value2);
						list.Add(sketchThing);
						thingsAt_multiple.Add(item2, list);
					}
					else
					{
						thingsAt_single.Add(item2, sketchThing);
					}
				}
				cachedThings.Add(sketchThing);
			}
			else
			{
				SketchTerrain sketchTerrain = entity as SketchTerrain;
				if (sketchTerrain != null)
				{
					terrainAt[sketchTerrain.pos] = sketchTerrain;
					cachedTerrain.Add(sketchTerrain);
				}
			}
		}

		private void RemoveFromCache(SketchEntity entity)
		{
			occupiedRectDirty = true;
			SketchBuildable sketchBuildable = entity as SketchBuildable;
			if (sketchBuildable != null)
			{
				cachedBuildables.Remove(sketchBuildable);
			}
			SketchThing sketchThing = entity as SketchThing;
			if (sketchThing != null)
			{
				if (sketchThing.def.building != null && sketchThing.def.building.isEdifice)
				{
					foreach (IntVec3 item in sketchThing.OccupiedRect)
					{
						if (edificeAt.TryGetValue(item, out SketchThing value) && value == sketchThing)
						{
							edificeAt.Remove(item);
						}
					}
				}
				foreach (IntVec3 item2 in sketchThing.OccupiedRect)
				{
					SketchThing value3;
					if (thingsAt_multiple.TryGetValue(item2, out List<SketchThing> value2))
					{
						value2.Remove(sketchThing);
					}
					else if (thingsAt_single.TryGetValue(item2, out value3) && value3 == sketchThing)
					{
						thingsAt_single.Remove(item2);
					}
				}
				cachedThings.Remove(sketchThing);
				return;
			}
			SketchTerrain sketchTerrain = entity as SketchTerrain;
			if (sketchTerrain != null)
			{
				if (terrainAt.TryGetValue(sketchTerrain.pos, out SketchTerrain value4) && value4 == sketchTerrain)
				{
					terrainAt.Remove(sketchTerrain.pos);
				}
				cachedTerrain.Remove(sketchTerrain);
			}
		}

		private void Recache(SketchEntity entity)
		{
			RemoveFromCache(entity);
			AddToCache(entity);
		}

		public void RecacheAll()
		{
			terrainAt.Clear();
			edificeAt.Clear();
			thingsAt_single.Clear();
			cachedThings.Clear();
			cachedTerrain.Clear();
			cachedBuildables.Clear();
			occupiedRectDirty = true;
			foreach (KeyValuePair<IntVec3, List<SketchThing>> item in thingsAt_multiple)
			{
				item.Value.Clear();
			}
			foreach (SketchEntity item2 in entities.OrderBy((SketchEntity x) => x.SpawnOrder))
			{
				AddToCache(item2);
			}
		}

		public bool LineOfSight(IntVec3 start, IntVec3 end, bool skipFirstCell = false, Func<IntVec3, bool> validator = null)
		{
			foreach (IntVec3 item in GenSight.PointsOnLineOfSight(start, end))
			{
				if (!skipFirstCell || !(item == start))
				{
					if (!CanBeSeenOver(item))
					{
						return false;
					}
					if (validator != null && !validator(item))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool LineOfSight(IntVec3 start, IntVec3 end, CellRect startRect, CellRect endRect, Func<IntVec3, bool> validator = null)
		{
			foreach (IntVec3 item in GenSight.PointsOnLineOfSight(start, end))
			{
				if (endRect.Contains(item))
				{
					return true;
				}
				if (!startRect.Contains(item))
				{
					if (!CanBeSeenOver(item))
					{
						return false;
					}
					if (validator != null && !validator(item))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool CanBeSeenOver(IntVec3 c)
		{
			SketchThing sketchThing = EdificeAt(c);
			if (sketchThing != null)
			{
				return sketchThing.def.Fillage != FillCategory.Full;
			}
			return true;
		}

		public bool Passable(int x, int z)
		{
			return Passable(new IntVec3(x, 0, z));
		}

		public bool Passable(IntVec3 pos)
		{
			TerrainDef terrainDef = TerrainAt(pos);
			if (terrainDef != null && terrainDef.passability == Traversability.Impassable)
			{
				return false;
			}
			foreach (SketchThing item in ThingsAt(pos))
			{
				if (item.def.passability == Traversability.Impassable)
				{
					return false;
				}
			}
			return true;
		}

		public void DrawGhost(IntVec3 pos, SpawnPosType posType = SpawnPosType.Unchanged, bool placingMode = false, Thing thingToIgnore = null)
		{
			IntVec3 offset = GetOffset(pos, posType);
			Map currentMap = Find.CurrentMap;
			bool flag = false;
			foreach (SketchEntity entity in Entities)
			{
				if (!entity.OccupiedRect.MovedBy(offset).InBounds(currentMap))
				{
					flag = true;
					break;
				}
			}
			foreach (SketchEntity entity2 in Entities)
			{
				if ((placingMode || !entity2.IsSameSpawnedOrBlueprintOrFrame(entity2.pos + offset, currentMap)) && entity2.OccupiedRect.MovedBy(offset).InBounds(currentMap))
				{
					Color color = (flag || (entity2.IsSpawningBlocked(entity2.pos + offset, currentMap, thingToIgnore) && !entity2.IsSameSpawnedOrBlueprintOrFrame(entity2.pos + offset, currentMap))) ? BlockedColor : GhostColor;
					entity2.DrawGhost(entity2.pos + offset, color);
				}
			}
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Func<IntVec3, int, bool> processor, int maxCellsToProcess = int.MaxValue, CellRect? bounds = null, IEnumerable<IntVec3> extraRoots = null)
		{
			if (floodFillWorking)
			{
				Log.Error("Nested FloodFill calls are not allowed. This will cause bugs.");
			}
			floodFillWorking = true;
			if (floodFillOpenSet == null)
			{
				floodFillOpenSet = new Queue<IntVec3>();
			}
			if (floodFillTraversalDistance == null)
			{
				floodFillTraversalDistance = new Dictionary<IntVec3, int>();
			}
			floodFillTraversalDistance.Clear();
			floodFillOpenSet.Clear();
			if (root.IsValid && extraRoots == null && !passCheck(root))
			{
				floodFillWorking = false;
				return;
			}
			if (!bounds.HasValue)
			{
				bounds = OccupiedRect;
			}
			int area = bounds.Value.Area;
			IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
			int num = cardinalDirectionsAround.Length;
			int num2 = 0;
			if (root.IsValid)
			{
				floodFillTraversalDistance.Add(root, 0);
				floodFillOpenSet.Enqueue(root);
			}
			if (extraRoots != null)
			{
				IList<IntVec3> list = extraRoots as IList<IntVec3>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						floodFillTraversalDistance.SetOrAdd(list[i], 0);
						floodFillOpenSet.Enqueue(list[i]);
					}
				}
				else
				{
					foreach (IntVec3 extraRoot in extraRoots)
					{
						floodFillTraversalDistance.SetOrAdd(extraRoot, 0);
						floodFillOpenSet.Enqueue(extraRoot);
					}
				}
			}
			while (floodFillOpenSet.Count > 0)
			{
				IntVec3 intVec = floodFillOpenSet.Dequeue();
				int num3 = floodFillTraversalDistance[intVec];
				if (processor(intVec, num3))
				{
					break;
				}
				num2++;
				if (num2 == maxCellsToProcess)
				{
					break;
				}
				for (int j = 0; j < num; j++)
				{
					IntVec3 intVec2 = intVec + cardinalDirectionsAround[j];
					if (bounds.Value.Contains(intVec2) && !floodFillTraversalDistance.ContainsKey(intVec2) && passCheck(intVec2))
					{
						floodFillOpenSet.Enqueue(intVec2);
						floodFillTraversalDistance.Add(intVec2, num3 + 1);
					}
				}
				if (floodFillOpenSet.Count > area)
				{
					Log.Error("Overflow on flood fill (>" + area + " cells). Make sure we're not flooding over the same area after we check it.");
					floodFillWorking = false;
					return;
				}
			}
			floodFillWorking = false;
		}

		private IEnumerable<IntVec3> GetSuggestedRoofCells()
		{
			if (!Empty)
			{
				CellRect occupiedRect = OccupiedRect;
				tmpSuggestedRoofCellsVisited.Clear();
				tmpYieldedSuggestedRoofCells.Clear();
				foreach (IntVec3 item in OccupiedRect)
				{
					if (!tmpSuggestedRoofCellsVisited.Contains(item) && !AnyRoofHolderAt(item))
					{
						tmpSuggestedRoofCells.Clear();
						FloodFill(item, (IntVec3 c) => !AnyRoofHolderAt(c), delegate(IntVec3 c, int dist)
						{
							tmpSuggestedRoofCellsVisited.Add(c);
							tmpSuggestedRoofCells.Add(c);
							return false;
						});
						bool flag = false;
						for (int k = 0; k < tmpSuggestedRoofCells.Count; k++)
						{
							if (occupiedRect.IsOnEdge(tmpSuggestedRoofCells[k]))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							for (int j = 0; j < tmpSuggestedRoofCells.Count; j++)
							{
								for (int i = 0; i < 9; i++)
								{
									IntVec3 intVec = tmpSuggestedRoofCells[j] + GenAdj.AdjacentCellsAndInside[i];
									if (!tmpYieldedSuggestedRoofCells.Contains(intVec) && occupiedRect.Contains(intVec) && (i == 8 || AnyRoofHolderAt(intVec)))
									{
										tmpYieldedSuggestedRoofCells.Add(intVec);
										yield return intVec;
									}
								}
							}
						}
					}
				}
				tmpSuggestedRoofCellsVisited.Clear();
				tmpYieldedSuggestedRoofCells.Clear();
			}
			bool AnyRoofHolderAt(IntVec3 c)
			{
				return EdificeAt(c)?.def.holdsRoof ?? false;
			}
		}

		private IntVec3 GetOffset(IntVec3 pos, SpawnPosType posType)
		{
			IntVec3 a;
			switch (posType)
			{
			case SpawnPosType.Unchanged:
				a = IntVec3.Zero;
				break;
			case SpawnPosType.OccupiedCenter:
				a = new IntVec3(-OccupiedCenter.x, 0, -OccupiedCenter.z);
				break;
			case SpawnPosType.OccupiedBotLeft:
				a = new IntVec3(-OccupiedRect.minX, 0, -OccupiedRect.minZ);
				break;
			default:
				a = default(IntVec3);
				break;
			}
			return a + pos;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref entities, "entities", LookMode.Deep);
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (entities.RemoveAll((SketchEntity x) => x == null) != 0)
			{
				Log.Error("Some sketch entities were null after loading.");
			}
			if (entities.RemoveAll((SketchEntity x) => x.LostImportantReferences) != 0)
			{
				Log.Error("Some sketch entities had null defs after loading.");
			}
			RecacheAll();
			tmpToRemove.Clear();
			for (int i = 0; i < cachedThings.Count; i++)
			{
				if (!cachedThings[i].def.IsDoor)
				{
					continue;
				}
				for (int j = 0; j < cachedThings.Count; j++)
				{
					if (cachedThings[j].def == ThingDefOf.Wall && cachedThings[j].pos == cachedThings[i].pos)
					{
						tmpToRemove.Add(cachedThings[j]);
					}
				}
			}
			for (int k = 0; k < tmpToRemove.Count; k++)
			{
				Log.Error("Sketch has a wall and a door in the same cell. Fixing.");
				Remove(tmpToRemove[k]);
			}
			tmpToRemove.Clear();
		}
	}
}
