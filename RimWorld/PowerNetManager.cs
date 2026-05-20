using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PowerNetManager
{
	private enum DelayedActionType
	{
		RegisterTransmitter,
		DeregisterTransmitter,
		RegisterConnector,
		DeregisterConnector
	}

	private struct DelayedAction
	{
		public DelayedActionType type;

		public CompPower compPower;

		public IntVec3 position;

		public Rot4 rotation;

		public DelayedAction(DelayedActionType type, CompPower compPower)
		{
			this.type = type;
			this.compPower = compPower;
			position = compPower.parent.Position;
			rotation = compPower.parent.Rotation;
		}
	}

	public Map map;

	private List<PowerNet> allNets = new List<PowerNet>();

	private List<DelayedAction> delayedActions = new List<DelayedAction>();

	public List<PowerNet> AllNetsListForReading => allNets;

	public PowerNetManager(Map map)
	{
		this.map = map;
	}

	public void Notify_TransmitterSpawned(CompPower newTransmitter)
	{
		delayedActions.Add(new DelayedAction(DelayedActionType.RegisterTransmitter, newTransmitter));
		NotifyDrawersForWireUpdate(newTransmitter.parent.Position);
	}

	public void Notify_TransmitterDespawned(CompPower oldTransmitter)
	{
		delayedActions.Add(new DelayedAction(DelayedActionType.DeregisterTransmitter, oldTransmitter));
		NotifyDrawersForWireUpdate(oldTransmitter.parent.Position);
	}

	public void Notfiy_TransmitterTransmitsPowerNowChanged(CompPower transmitter)
	{
		if (transmitter.parent.Spawned)
		{
			delayedActions.Add(new DelayedAction(DelayedActionType.DeregisterTransmitter, transmitter));
			delayedActions.Add(new DelayedAction(DelayedActionType.RegisterTransmitter, transmitter));
			NotifyDrawersForWireUpdate(transmitter.parent.Position);
		}
	}

	public void Notify_ConnectorWantsConnect(CompPower wantingCon)
	{
		if (Scribe.mode == LoadSaveMode.Inactive && !HasRegisterConnectorDuplicate(wantingCon))
		{
			delayedActions.Add(new DelayedAction(DelayedActionType.RegisterConnector, wantingCon));
		}
		NotifyDrawersForWireUpdate(wantingCon.parent.Position);
	}

	public void Notify_ConnectorDespawned(CompPower oldCon)
	{
		delayedActions.Add(new DelayedAction(DelayedActionType.DeregisterConnector, oldCon));
		NotifyDrawersForWireUpdate(oldCon.parent.Position);
	}

	public void NotifyDrawersForWireUpdate(IntVec3 root)
	{
		map.mapDrawer.MapMeshDirty(root, MapMeshFlagDefOf.Things, regenAdjacentCells: true, regenAdjacentSections: false);
		map.mapDrawer.MapMeshDirty(root, MapMeshFlagDefOf.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: true);
	}

	public void RegisterPowerNet(PowerNet newNet)
	{
		allNets.Add(newNet);
		newNet.powerNetManager = this;
		map.powerNetGrid.Notify_PowerNetCreated(newNet);
		PowerNetMaker.UpdateVisualLinkagesFor(newNet);
	}

	public void DeletePowerNet(PowerNet oldNet)
	{
		allNets.Remove(oldNet);
		map.powerNetGrid.Notify_PowerNetDeleted(oldNet);
	}

	public void PowerNetsTick()
	{
		for (int i = 0; i < allNets.Count; i++)
		{
			allNets[i].PowerNetTick();
		}
	}

	public void UpdatePowerNetsAndConnections_First()
	{
		int count = delayedActions.Count;
		for (int i = 0; i < count; i++)
		{
			DelayedAction delayedAction = delayedActions[i];
			switch (delayedActions[i].type)
			{
			case DelayedActionType.RegisterTransmitter:
			{
				if (!(delayedAction.position == delayedAction.compPower.parent.Position))
				{
					break;
				}
				ThingWithComps parent = delayedAction.compPower.parent;
				if (map.powerNetGrid.TransmittedPowerNetAt(parent.Position) != null)
				{
					Log.Warning("Tried to register trasmitter " + parent?.ToString() + " at " + parent.Position.ToString() + ", but there is already a power net here. There can't be two transmitters on the same cell.");
				}
				delayedAction.compPower.SetUpPowerVars();
				foreach (IntVec3 item in GenAdj.CellsAdjacentCardinal(parent))
				{
					TryDestroyNetAt(item);
				}
				break;
			}
			case DelayedActionType.DeregisterTransmitter:
				TryDestroyNetAt(delayedAction.position);
				PowerConnectionMaker.DisconnectAllFromTransmitterAndSetWantConnect(delayedAction.compPower, map);
				delayedAction.compPower.ResetPowerVars();
				break;
			}
		}
		for (int j = 0; j < count; j++)
		{
			DelayedAction delayedAction2 = delayedActions[j];
			if ((delayedAction2.type != DelayedActionType.RegisterTransmitter || !(delayedAction2.position == delayedAction2.compPower.parent.Position)) && delayedAction2.type != DelayedActionType.DeregisterTransmitter)
			{
				continue;
			}
			TryCreateNetAt(delayedAction2.position);
			foreach (IntVec3 item2 in GenAdj.CellsAdjacentCardinal(delayedAction2.position, delayedAction2.rotation, delayedAction2.compPower.parent.def.size))
			{
				TryCreateNetAt(item2);
			}
		}
		for (int k = 0; k < count; k++)
		{
			DelayedAction delayedAction3 = delayedActions[k];
			switch (delayedActions[k].type)
			{
			case DelayedActionType.RegisterConnector:
				if (delayedAction3.position == delayedAction3.compPower.parent.Position)
				{
					delayedAction3.compPower.SetUpPowerVars();
					PowerConnectionMaker.TryConnectToAnyPowerNet(delayedAction3.compPower);
				}
				break;
			case DelayedActionType.DeregisterConnector:
				PowerConnectionMaker.DisconnectFromPowerNet(delayedAction3.compPower);
				delayedAction3.compPower.ResetPowerVars();
				break;
			}
		}
		delayedActions.RemoveRange(0, count);
		if (DebugViewSettings.drawPower)
		{
			DrawDebugPowerNets();
		}
	}

	private bool HasRegisterConnectorDuplicate(CompPower compPower)
	{
		for (int num = delayedActions.Count - 1; num >= 0; num--)
		{
			if (delayedActions[num].compPower == compPower)
			{
				if (delayedActions[num].type == DelayedActionType.DeregisterConnector)
				{
					return false;
				}
				if (delayedActions[num].type == DelayedActionType.RegisterConnector)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void TryCreateNetAt(IntVec3 cell)
	{
		if (!cell.InBounds(map) || map.powerNetGrid.TransmittedPowerNetAt(cell) != null)
		{
			return;
		}
		Building transmitter = cell.GetTransmitter(map);
		if (transmitter != null && transmitter.TransmitsPowerNow)
		{
			PowerNet powerNet = PowerNetMaker.NewPowerNetStartingFrom(transmitter);
			RegisterPowerNet(powerNet);
			for (int i = 0; i < powerNet.transmitters.Count; i++)
			{
				PowerConnectionMaker.ConnectAllConnectorsToTransmitter(powerNet.transmitters[i]);
			}
		}
	}

	private void TryDestroyNetAt(IntVec3 cell)
	{
		if (cell.InBounds(map))
		{
			PowerNet powerNet = map.powerNetGrid.TransmittedPowerNetAt(cell);
			if (powerNet != null)
			{
				DeletePowerNet(powerNet);
			}
		}
	}

	private void DrawDebugPowerNets()
	{
		if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap != map)
		{
			return;
		}
		int num = 0;
		foreach (PowerNet allNet in allNets)
		{
			foreach (CompPower item in allNet.transmitters.Concat(allNet.connectors))
			{
				foreach (IntVec3 item2 in GenAdj.CellsOccupiedBy(item.parent))
				{
					CellRenderer.RenderCell(item2, (float)num * 0.44f);
				}
			}
			num++;
		}
	}
}
