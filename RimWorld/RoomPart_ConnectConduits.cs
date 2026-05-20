using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RoomPart_ConnectConduits : RoomPartWorker
{
	public override bool FillOnPost => true;

	public RoomPart_ConnectConduits(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		HashSet<Building> hashSet = new HashSet<Building>();
		HashSet<Building> hashSet2 = new HashSet<Building>();
		HashSet<Building> hashSet3 = new HashSet<Building>();
		foreach (CellRect rect in room.rects)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				List<Thing> list = map.thingGrid.ThingsListAt(cell);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is Building { PowerComp: not null } building)
					{
						if (building.PowerComp is CompPowerPlant)
						{
							hashSet.Add(building);
						}
						else if (building.def == ThingDefOf.HiddenConduit)
						{
							hashSet3.Add(building);
						}
						else if (!(building.PowerComp is CompPowerTransmitter))
						{
							hashSet2.Add(building);
						}
					}
				}
			}
		}
		if (hashSet.Any() && hashSet2.Any())
		{
			foreach (Building conduit in hashSet3)
			{
				Building building2 = hashSet.OrderBy((Building ps) => (ps.Position - conduit.Position).LengthHorizontalSquared).FirstOrDefault();
				if (building2 != null)
				{
					CreateHiddenConduitPath(map, building2.Position, conduit.Position);
				}
			}
		}
		if (hashSet.Count > 1)
		{
			List<Building> list2 = hashSet.ToList();
			Building building3 = list2[0];
			for (int num = 1; num < list2.Count; num++)
			{
				CreateHiddenConduitPath(map, building3.Position, list2[num].Position);
			}
		}
		foreach (Building item in hashSet2)
		{
			CompPower powerComp = item.PowerComp;
			PowerConnectionMaker.DisconnectFromPowerNet(powerComp);
			Building transmitter = powerComp.parent.Position.GetTransmitter(map);
			if (transmitter?.PowerComp != null)
			{
				powerComp.ConnectToTransmitter(transmitter.PowerComp);
			}
			else
			{
				map.powerNetManager.Notify_ConnectorWantsConnect(powerComp);
			}
		}
	}

	private void CreateHiddenConduitPath(Map map, IntVec3 from, IntVec3 to)
	{
		List<IntVec3> list = FindCardinalOnlyPath(map, from, to);
		if (list == null || list.Count == 0)
		{
			return;
		}
		foreach (IntVec3 item in list)
		{
			if (item.GetTransmitter(map) == null)
			{
				GenSpawn.TrySpawn(ThingDefOf.HiddenConduit, item, map, out var _);
			}
		}
	}

	private List<IntVec3> FindCardinalOnlyPath(Map map, IntVec3 from, IntVec3 to)
	{
		new List<IntVec3>();
		List<IntVec3> list = TryLShapedPath(map, from, to, horizontalFirst: true);
		if (list != null)
		{
			return list;
		}
		List<IntVec3> list2 = TryLShapedPath(map, from, to, horizontalFirst: false);
		if (list2 != null)
		{
			return list2;
		}
		return null;
	}

	private List<IntVec3> TryLShapedPath(Map map, IntVec3 from, IntVec3 to, bool horizontalFirst)
	{
		List<IntVec3> list = new List<IntVec3>();
		IntVec3 intVec = from;
		if (horizontalFirst)
		{
			while (intVec.x != to.x)
			{
				if (intVec.x < to.x)
				{
					intVec.x++;
				}
				else
				{
					intVec.x--;
				}
				if (intVec != to && intVec != from)
				{
					if (!CanPlaceConduitAt(map, intVec))
					{
						return null;
					}
					list.Add(intVec);
				}
			}
			while (intVec.z != to.z)
			{
				if (intVec.z < to.z)
				{
					intVec.z++;
				}
				else
				{
					intVec.z--;
				}
				if (intVec != to && intVec != from)
				{
					if (!CanPlaceConduitAt(map, intVec))
					{
						return null;
					}
					list.Add(intVec);
				}
			}
		}
		else
		{
			while (intVec.z != to.z)
			{
				if (intVec.z < to.z)
				{
					intVec.z++;
				}
				else
				{
					intVec.z--;
				}
				if (intVec != to && intVec != from)
				{
					if (!CanPlaceConduitAt(map, intVec))
					{
						return null;
					}
					list.Add(intVec);
				}
			}
			while (intVec.x != to.x)
			{
				if (intVec.x < to.x)
				{
					intVec.x++;
				}
				else
				{
					intVec.x--;
				}
				if (intVec != to && intVec != from)
				{
					if (!CanPlaceConduitAt(map, intVec))
					{
						return null;
					}
					list.Add(intVec);
				}
			}
		}
		return list;
	}

	private bool CanPlaceConduitAt(Map map, IntVec3 cell)
	{
		if (!cell.InBounds(map))
		{
			return false;
		}
		Building transmitter = cell.GetTransmitter(map);
		if (transmitter != null && transmitter.def != ThingDefOf.HiddenConduit)
		{
			return false;
		}
		return true;
	}
}
