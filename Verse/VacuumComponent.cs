using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class VacuumComponent : MapComponent, ICellBoolGiver, IDisposable
{
	private class GroupData : IFullPoolable
	{
		public readonly Dictionary<Room, HashSet<IntVec3>> adjacent = new Dictionary<Room, HashSet<IntVec3>>();

		public HashSet<IntVec3> directWarnings;

		public bool hasDirectPathToVacuum;

		public void Reset()
		{
			foreach (var (_, hashSet2) in adjacent)
			{
				hashSet2.Clear();
				SimplePool<HashSet<IntVec3>>.Return(hashSet2);
			}
			adjacent.Clear();
			directWarnings = null;
			hasDirectPathToVacuum = false;
		}
	}

	private CellBoolDrawer drawerInt;

	private bool dirty = true;

	private const int UpdateInterval = 250;

	private const float VacuumDeltaPerSecondFactor = 1.0416666f;

	private const float VacuumFactorPerHundredCells = 0.5f;

	private const float DirectVacuumPenaltyPerHundredCells = 0.1f;

	private const float WarningLevel = 0.5f;

	private readonly Dictionary<Room, GroupData> roomGroups = new Dictionary<Room, GroupData>();

	private static readonly Material IntWarningMat = MatLoader.LoadMat("Map/OxygenOverlay/Warning");

	private static readonly Material IntVacuumOverlayMat0 = MatLoader.LoadMat("Map/OxygenOverlay/VacuumOverlay");

	private static readonly Material IntVacuumOverlayTransitionMat0 = MatLoader.LoadMat("Map/OxygenOverlay/VacuumOverlayTransition");

	private static readonly Material IntVacuumOverlayMat1 = MatLoader.LoadMat("Map/OxygenOverlay/VacuumOverlay2");

	private static readonly Material IntVacuumOverlayTransitionMat1 = MatLoader.LoadMat("Map/OxygenOverlay/VacuumOverlayTransition2");

	private static readonly int VacuumPercent = Shader.PropertyToID("_VacuumPercent");

	private readonly List<Matrix4x4> vacuumBoxMatrices = new List<Matrix4x4>();

	private readonly List<float> vacuumBoxIntensities = new List<float>();

	private MaterialPropertyBlock vacuumBoxPropBlock;

	private readonly Dictionary<IntVec3, float> vacuumedCells = new Dictionary<IntVec3, float>();

	private readonly List<Matrix4x4> vacuumTransitionMatrices = new List<Matrix4x4>();

	private readonly List<float> vacuumTransitionIntensities = new List<float>();

	private readonly Dictionary<Room, float> roomVacuums = new Dictionary<Room, float>();

	private readonly HashSet<IntVec3> warningCells = new HashSet<IntVec3>();

	private readonly List<Matrix4x4> warningMatrices = new List<Matrix4x4>();

	private MaterialPropertyBlock vacuumTransitionPropBlock;

	private static readonly HashSet<Room> visited = new HashSet<Room>();

	private static readonly Dictionary<Room, HashSet<IntVec3>> exteriorCells = new Dictionary<Room, HashSet<IntVec3>>();

	private static readonly Queue<Room> frontier = new Queue<Room>();

	private CellBoolDrawer Drawer => drawerInt ?? (drawerInt = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3600));

	private Material WarningMat => IntWarningMat;

	private Material VacuumOverlayMat0 => IntVacuumOverlayMat0;

	private Material VacuumOverlayTransitionMat0 => IntVacuumOverlayTransitionMat0;

	private Material VacuumOverlayMat1 => IntVacuumOverlayMat1;

	private Material VacuumOverlayTransitionMat1 => IntVacuumOverlayTransitionMat1;

	private bool ActiveOnMap
	{
		get
		{
			if (ModLister.OdysseyInstalled)
			{
				return map.Biome.inVacuum;
			}
			return false;
		}
	}

	public Color Color => Color.white;

	public VacuumComponent(Map map)
		: base(map)
	{
		if (Scribe.mode == LoadSaveMode.Inactive)
		{
			InitializeIfRequired();
		}
	}

	private void InitializeIfRequired()
	{
		if (ActiveOnMap)
		{
			map.events.RegionsRoomsChanged += Dirty;
			map.events.DoorOpened += delegate
			{
				Dirty();
			};
			map.events.DoorClosed += delegate
			{
				Dirty();
			};
			map.events.TerrainChanged += delegate
			{
				Dirty();
			};
			map.events.RoofChanged += delegate
			{
				Dirty();
			};
		}
	}

	public override void FinalizeInit()
	{
		if (ActiveOnMap)
		{
			RebuildData();
		}
	}

	public override void MapComponentTick()
	{
		if (ActiveOnMap && map.IsHashIntervalTick(250))
		{
			if (dirty)
			{
				RebuildData();
			}
			ExchangeRoomVacuum();
			UpdateVacuumOverlay();
		}
	}

	private bool HasDirectPathToVacuum(Room room, out HashSet<IntVec3> warning)
	{
		frontier.Enqueue(room);
		Room result;
		while (frontier.TryDequeue(out result))
		{
			visited.Add(result);
			if (result.ExposedToSpace)
			{
				warning = exteriorCells[result];
				visited.Clear();
				exteriorCells.Clear();
				frontier.Clear();
				return true;
			}
			if (!roomGroups.TryGetValue(result, out var value))
			{
				continue;
			}
			foreach (var (room3, hashSet2) in value.adjacent)
			{
				if (visited.Contains(room3) || frontier.Contains(room3))
				{
					continue;
				}
				foreach (IntVec3 item in hashSet2)
				{
					Building edifice = item.GetEdifice(map);
					if (edifice == null || edifice is Building_Door { Open: not false })
					{
						exteriorCells[room3] = hashSet2;
						frontier.Enqueue(room3);
						break;
					}
				}
			}
		}
		visited.Clear();
		exteriorCells.Clear();
		frontier.Clear();
		warning = null;
		return false;
	}

	private void ExchangeRoomVacuum()
	{
		warningCells.Clear();
		warningMatrices.Clear();
		Room key;
		GroupData value;
		HashSet<IntVec3> value2;
		foreach (KeyValuePair<Room, GroupData> roomGroup in roomGroups)
		{
			roomGroup.Deconstruct(out key, out value);
			Room room = key;
			GroupData groupData = value;
			roomVacuums[room] = room.UnsanitizedVacuum;
			foreach (KeyValuePair<Room, HashSet<IntVec3>> item in groupData.adjacent)
			{
				item.Deconstruct(out key, out value2);
				Room room2 = key;
				roomVacuums[room2] = room2.UnsanitizedVacuum;
			}
		}
		foreach (KeyValuePair<Room, GroupData> roomGroup2 in roomGroups)
		{
			roomGroup2.Deconstruct(out key, out value);
			Room room3 = key;
			GroupData groupData2 = value;
			if (groupData2.adjacent.Count == 0 || room3.IsDoorway)
			{
				continue;
			}
			float num = roomVacuums[room3];
			int num2 = room3.CellCount;
			float num3 = (float)room3.CellCount * num;
			int num4 = room3.CellCount;
			if (room3.ExposedToSpace)
			{
				continue;
			}
			foreach (KeyValuePair<Room, HashSet<IntVec3>> item2 in groupData2.adjacent)
			{
				item2.Deconstruct(out key, out value2);
				Room room4 = key;
				HashSet<IntVec3> hashSet = value2;
				num2 += room4.CellCount;
				num3 += (float)room4.CellCount * roomVacuums[room4];
				if (!room4.ExposedToSpace)
				{
					num4 += room4.CellCount;
					continue;
				}
				foreach (IntVec3 item3 in hashSet)
				{
					if (warningCells.Add(item3))
					{
						warningMatrices.Add(Matrix4x4.Translate(item3.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay)));
					}
				}
			}
			float num5 = Mathf.Clamp(num3 / (float)num2, -0.1f, 1f);
			if (groupData2.hasDirectPathToVacuum)
			{
				if (room3.Vacuum >= 1f)
				{
					continue;
				}
				foreach (IntVec3 directWarning in groupData2.directWarnings)
				{
					if (warningCells.Add(directWarning))
					{
						warningMatrices.Add(Matrix4x4.Translate(directWarning.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay)));
					}
				}
				float num6 = Mathf.Min(0.1f / ((float)num4 / 100f), 1f);
				num5 = Mathf.Clamp(num5 + num6, -0.1f, 1f);
				float num7 = Mathf.Min(0.5f / ((float)num4 / 100f), 1f);
				float num8 = (num5 - num) * 1.0416666f * num7;
				room3.Vacuum = num + num8;
			}
			else if (!Mathf.Approximately(num, num5))
			{
				float num9 = Mathf.Min(0.5f / ((float)num4 / 100f), 1f);
				float num10 = (num5 - num) * 1.0416666f * num9;
				room3.Vacuum = num + num10;
			}
		}
		roomVacuums.Clear();
		Drawer.SetDirty();
	}

	public void RebuildData()
	{
		using (ProfilerBlock.Scope("VacuumComponent.RebuildData"))
		{
			dirty = false;
			ClearBuffer();
			MergeRoomsIntoGroups();
			RegenerateVacuumOverlay();
		}
	}

	public void Dirty()
	{
		dirty = true;
		SetDrawerDirty();
	}

	private void ClearBuffer()
	{
		foreach (KeyValuePair<Room, GroupData> roomGroup in roomGroups)
		{
			roomGroup.Deconstruct(out var _, out var value);
			FullPool<GroupData>.Return(value);
		}
		roomGroups.Clear();
	}

	private void MergeRoomsIntoGroups()
	{
		foreach (Room allRoom in map.regionGrid.AllRooms)
		{
			if (allRoom.ExposedToSpace)
			{
				continue;
			}
			GroupData orAddGroup = GetOrAddGroup(allRoom);
			if (allRoom.IsDoorway)
			{
				Building_Door door = allRoom.Cells.First().GetDoor(map);
				if (door == null || !door.ExchangeVacuum)
				{
					continue;
				}
				CellRect cellRect = door.OccupiedRect();
				foreach (IntVec3 item in cellRect)
				{
					for (int i = 0; i < 4; i++)
					{
						IntVec3 intVec = item + GenAdj.CardinalDirections[i];
						if (cellRect.Contains(intVec))
						{
							continue;
						}
						Room room = intVec.GetRoom(map);
						if (room != null && room != allRoom)
						{
							if (!orAddGroup.adjacent.TryGetValue(room, out var value))
							{
								HashSet<IntVec3> hashSet = (orAddGroup.adjacent[room] = SimplePool<HashSet<IntVec3>>.Get());
								value = hashSet;
							}
							value.Add(item);
						}
					}
				}
				continue;
			}
			foreach (IntVec3 item2 in allRoom.BorderCellsCardinal)
			{
				Building edifice = item2.GetEdifice(map);
				if (edifice == null || !edifice.ExchangeVacuum)
				{
					continue;
				}
				CellRect cellRect2 = edifice.OccupiedRect();
				foreach (IntVec3 item3 in cellRect2)
				{
					for (int j = 0; j < 4; j++)
					{
						IntVec3 intVec2 = item3 + GenAdj.CardinalDirections[j];
						if (cellRect2.Contains(intVec2))
						{
							continue;
						}
						Room room2 = intVec2.GetRoom(map);
						if (room2 != null && room2 != allRoom)
						{
							if (!orAddGroup.adjacent.TryGetValue(room2, out var value2))
							{
								HashSet<IntVec3> hashSet = (orAddGroup.adjacent[room2] = SimplePool<HashSet<IntVec3>>.Get());
								value2 = hashSet;
							}
							value2.Add(item2);
						}
					}
				}
			}
		}
		foreach (var (room4, groupData2) in roomGroups)
		{
			if (HasDirectPathToVacuum(room4, out var warning))
			{
				groupData2.hasDirectPathToVacuum = true;
				groupData2.directWarnings = warning;
			}
		}
	}

	private GroupData GetOrAddGroup(Room room)
	{
		if (roomGroups.TryGetValue(room, out var value))
		{
			return value;
		}
		GroupData groupData = FullPool<GroupData>.Get();
		roomGroups[room] = groupData;
		return groupData;
	}

	public void SetDrawerDirty()
	{
		Drawer.SetDirty();
	}

	private void RegenerateVacuumOverlay()
	{
		vacuumBoxMatrices.Clear();
		vacuumBoxIntensities.Clear();
		vacuumedCells.Clear();
		foreach (KeyValuePair<Room, GroupData> roomGroup in roomGroups)
		{
			roomGroup.Deconstruct(out var key, out var _);
			Room room = key;
			room.vacuumOverlayDrawn = RoomVacuumVisible(room);
			room.vacuumOverlayRects = 0;
			if (!room.vacuumOverlayDrawn)
			{
				continue;
			}
			float vacuum = room.Vacuum;
			foreach (IntVec3 cell in room.Cells)
			{
				vacuumedCells.Add(cell, vacuum);
			}
			foreach (Region region in room.Regions)
			{
				foreach (CellRect item4 in region.EnumerateRectangleCover())
				{
					Vector3 centerVector = item4.CenterVector3;
					centerVector.y = AltitudeLayer.Gas.AltitudeFor();
					Matrix4x4 item = Matrix4x4.TRS(centerVector, Quaternion.identity, new Vector3(item4.Width, 1f, item4.Height));
					room.vacuumOverlayRects++;
					vacuumBoxMatrices.Add(item);
					vacuumBoxIntensities.Add(vacuum);
				}
			}
		}
		vacuumTransitionMatrices.Clear();
		vacuumTransitionIntensities.Clear();
		foreach (KeyValuePair<IntVec3, float> vacuumedCell in vacuumedCells)
		{
			vacuumedCell.Deconstruct(out var key2, out var value2);
			IntVec3 intVec = key2;
			float item2 = value2;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 key3 = intVec + GenAdj.CardinalDirections[i];
				if (!vacuumedCells.ContainsKey(key3))
				{
					Matrix4x4 item3 = Matrix4x4.TRS(key3.ToVector3ShiftedWithAltitude(AltitudeLayer.Gas), new Rot4(i).AsQuat, Vector3.one);
					vacuumTransitionMatrices.Add(item3);
					vacuumTransitionIntensities.Add(item2);
				}
			}
		}
	}

	private void UpdateVacuumOverlay()
	{
		int num = 0;
		foreach (var (room2, _) in roomGroups)
		{
			if (RoomVacuumVisible(room2) != room2.vacuumOverlayDrawn)
			{
				RegenerateVacuumOverlay();
				break;
			}
			for (int i = 0; i < room2.vacuumOverlayRects; i++)
			{
				vacuumBoxIntensities[num++] = room2.Vacuum;
			}
		}
	}

	private static bool RoomVacuumVisible(Room room)
	{
		if (!room.ExposedToSpace && room.UnsanitizedVacuum > 0.5f)
		{
			if (room.IsDoorway)
			{
				return room.Door.Open;
			}
			return true;
		}
		return false;
	}

	public override void MapComponentDraw()
	{
		if (!map.Biome.inVacuum)
		{
			return;
		}
		if (Find.PlaySettings.showVacuumOverlay && !WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			Drawer.MarkForDraw();
			if (warningMatrices.Count > 0)
			{
				Graphics.DrawMeshInstanced(MeshPool.plane08, 0, WarningMat, warningMatrices);
			}
		}
		using (ProfilerBlock.Scope("VacuumComponent.Drawer.CellBoolDrawerUpdate()"))
		{
			Drawer.CellBoolDrawerUpdate();
		}
		if (WorldComponent_GravshipController.CutsceneInProgress)
		{
			return;
		}
		if (vacuumBoxMatrices.Count > 0)
		{
			if (vacuumBoxPropBlock == null)
			{
				vacuumBoxPropBlock = new MaterialPropertyBlock();
			}
			vacuumBoxPropBlock.Clear();
			if (vacuumBoxIntensities.Count > 0)
			{
				vacuumBoxPropBlock.SetFloatArray(VacuumPercent, vacuumBoxIntensities);
			}
			Graphics.DrawMeshInstanced(MeshPool.plane10, 0, VacuumOverlayMat0, vacuumBoxMatrices, vacuumBoxPropBlock);
			Graphics.DrawMeshInstanced(MeshPool.plane10, 0, VacuumOverlayMat1, vacuumBoxMatrices, vacuumBoxPropBlock);
		}
		if (vacuumTransitionMatrices.Count > 0)
		{
			if (vacuumTransitionPropBlock == null)
			{
				vacuumTransitionPropBlock = new MaterialPropertyBlock();
			}
			vacuumTransitionPropBlock.Clear();
			if (vacuumTransitionIntensities.Count > 0)
			{
				vacuumTransitionPropBlock.SetFloatArray(VacuumPercent, vacuumTransitionIntensities);
			}
			Graphics.DrawMeshInstanced(MeshPool.plane10, 0, VacuumOverlayTransitionMat0, vacuumTransitionMatrices, vacuumTransitionPropBlock);
			Graphics.DrawMeshInstanced(MeshPool.plane10, 0, VacuumOverlayTransitionMat1, vacuumTransitionMatrices, vacuumTransitionPropBlock);
		}
	}

	public bool GetCellBool(int index)
	{
		IntVec3 c = map.cellIndices.IndexToCell(index);
		Building edifice = c.GetEdifice(map);
		if (!c.Fogged(map))
		{
			return edifice?.ExchangeVacuum ?? true;
		}
		return false;
	}

	public Color GetCellExtraColor(int index)
	{
		float vacuum = map.cellIndices.IndexToCell(index).GetVacuum(map);
		if (vacuum < 0.5f)
		{
			return Color.Lerp(Color.green, Color.yellow, Mathf.InverseLerp(0f, 0.5f, vacuum));
		}
		return Color.Lerp(Color.yellow, Color.red, Mathf.InverseLerp(0.5f, 1f, vacuum));
	}

	public override void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			InitializeIfRequired();
		}
	}

	public void Dispose()
	{
		ClearBuffer();
	}
}
