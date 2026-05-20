using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class PowerConnectionMaker
{
	private const int ConnectMaxDist = 6;

	public static void ConnectAllConnectorsToTransmitter(CompPower newTransmitter)
	{
		foreach (CompPower item in PotentialConnectorsForTransmitter(newTransmitter))
		{
			if (item.connectParent == null)
			{
				item.ConnectToTransmitter(newTransmitter);
			}
		}
	}

	public static void DisconnectAllFromTransmitterAndSetWantConnect(CompPower deadPc, Map map)
	{
		if (deadPc.connectChildren == null)
		{
			return;
		}
		for (int i = 0; i < deadPc.connectChildren.Count; i++)
		{
			CompPower compPower = deadPc.connectChildren[i];
			compPower.connectParent = null;
			if (compPower is CompPowerTrader compPowerTrader)
			{
				compPowerTrader.PowerOn = false;
			}
			map.powerNetManager.Notify_ConnectorWantsConnect(compPower);
		}
	}

	public static void TryConnectToAnyPowerNet(CompPower pc, List<PowerNet> disallowedNets = null)
	{
		if (pc.connectParent == null && pc.parent.Spawned)
		{
			CompPower compPower = BestTransmitterForConnector(pc.parent.def.building.isAttachment ? GenConstruct.GetWallAttachedTo(pc.parent).Position : pc.parent.Position, pc.parent.Map, disallowedNets);
			if (compPower != null)
			{
				pc.ConnectToTransmitter(compPower);
			}
			else
			{
				pc.connectParent = null;
			}
		}
	}

	public static void DisconnectFromPowerNet(CompPower pc)
	{
		if (pc.connectParent == null)
		{
			return;
		}
		if (pc.PowerNet != null)
		{
			pc.PowerNet.DeregisterConnector(pc);
		}
		if (pc.connectParent.connectChildren != null)
		{
			pc.connectParent.connectChildren.Remove(pc);
			if (pc.connectParent.connectChildren.Count == 0)
			{
				pc.connectParent.connectChildren = null;
			}
		}
		pc.connectParent = null;
	}

	private static IEnumerable<CompPower> PotentialConnectorsForTransmitter(CompPower b)
	{
		if (!b.parent.Spawned)
		{
			Log.Warning("Can't check potential connectors for " + b?.ToString() + " because it's unspawned.");
			yield break;
		}
		CellRect rect = b.parent.OccupiedRect().ExpandedBy(6).ClipInsideMap(b.parent.Map);
		for (int z = rect.minZ; z <= rect.maxZ; z++)
		{
			for (int x = rect.minX; x <= rect.maxX; x++)
			{
				IntVec3 c = new IntVec3(x, 0, z);
				List<Thing> thingList = b.parent.Map.thingGrid.ThingsListAt(c);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j].def.ConnectToPower)
					{
						yield return ((Building)thingList[j]).PowerComp;
					}
				}
			}
		}
	}

	public static CompPower BestTransmitterForConnector(IntVec3 connectorPos, Map map, List<PowerNet> disallowedNets = null)
	{
		CellRect cellRect = CellRect.SingleCell(connectorPos).ExpandedBy(6).ClipInsideMap(map);
		cellRect.ClipInsideMap(map);
		float num = 999999f;
		CompPower result = null;
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				Building transmitter = new IntVec3(j, 0, i).GetTransmitter(map);
				if (transmitter == null || transmitter.Destroyed)
				{
					continue;
				}
				CompPower powerComp = transmitter.PowerComp;
				if (powerComp != null && powerComp.TransmitsPowerNow && (transmitter.def.building == null || transmitter.def.building.allowWireConnection) && (disallowedNets == null || !disallowedNets.Contains(powerComp.transNet)))
				{
					float num2 = (transmitter.Position - connectorPos).LengthHorizontalSquared;
					if (num2 < num)
					{
						num = num2;
						result = powerComp;
					}
				}
			}
		}
		return result;
	}
}
