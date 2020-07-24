using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class CompPower : ThingComp
	{
		public PowerNet transNet;

		public CompPower connectParent;

		public List<CompPower> connectChildren;

		private static List<PowerNet> recentlyConnectedNets = new List<PowerNet>();

		private static CompPower lastManualReconnector = null;

		public static readonly float WattsToWattDaysPerTick = 1.66666669E-05f;

		public bool TransmitsPowerNow => ((Building)parent).TransmitsPowerNow;

		public PowerNet PowerNet
		{
			get
			{
				if (transNet != null)
				{
					return transNet;
				}
				if (connectParent != null)
				{
					return connectParent.transNet;
				}
				return null;
			}
		}

		public CompProperties_Power Props => (CompProperties_Power)props;

		public virtual void ResetPowerVars()
		{
			transNet = null;
			connectParent = null;
			connectChildren = null;
			recentlyConnectedNets.Clear();
			lastManualReconnector = null;
		}

		public virtual void SetUpPowerVars()
		{
		}

		public override void PostExposeData()
		{
			Thing refee = null;
			if (Scribe.mode == LoadSaveMode.Saving && connectParent != null)
			{
				refee = connectParent.parent;
			}
			Scribe_References.Look(ref refee, "parentThing");
			if (refee != null)
			{
				connectParent = ((ThingWithComps)refee).GetComp<CompPower>();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && connectParent != null)
			{
				ConnectToTransmitter(connectParent, reconnectingAfterLoading: true);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (Props.transmitsPower || parent.def.ConnectToPower)
			{
				parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
				if (Props.transmitsPower)
				{
					parent.Map.powerNetManager.Notify_TransmitterSpawned(this);
				}
				if (parent.def.ConnectToPower)
				{
					parent.Map.powerNetManager.Notify_ConnectorWantsConnect(this);
				}
				SetUpPowerVars();
			}
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (!Props.transmitsPower && !parent.def.ConnectToPower)
			{
				return;
			}
			if (Props.transmitsPower)
			{
				if (connectChildren != null)
				{
					for (int i = 0; i < connectChildren.Count; i++)
					{
						connectChildren[i].LostConnectParent();
					}
				}
				map.powerNetManager.Notify_TransmitterDespawned(this);
			}
			if (parent.def.ConnectToPower)
			{
				map.powerNetManager.Notify_ConnectorDespawned(this);
			}
			map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
		}

		public virtual void LostConnectParent()
		{
			connectParent = null;
			if (parent.Spawned)
			{
				parent.Map.powerNetManager.Notify_ConnectorWantsConnect(this);
			}
		}

		public override void PostPrintOnto(SectionLayer layer)
		{
			base.PostPrintOnto(layer);
			if (connectParent != null)
			{
				PowerNetGraphics.PrintWirePieceConnecting(layer, parent, connectParent.parent, forPowerOverlay: false);
			}
		}

		public override void CompPrintForPowerGrid(SectionLayer layer)
		{
			if (TransmitsPowerNow)
			{
				PowerOverlayMats.LinkedOverlayGraphic.Print(layer, parent);
			}
			if (parent.def.ConnectToPower)
			{
				PowerNetGraphics.PrintOverlayConnectorBaseFor(layer, parent);
			}
			if (connectParent != null)
			{
				PowerNetGraphics.PrintWirePieceConnecting(layer, parent, connectParent.parent, forPowerOverlay: true);
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (connectParent != null && parent.Faction == Faction.OfPlayer)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.action = delegate
				{
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					TryManualReconnect();
				};
				command_Action.hotKey = KeyBindingDefOf.Misc2;
				command_Action.defaultDesc = "CommandTryReconnectDesc".Translate();
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/TryReconnect");
				command_Action.defaultLabel = "CommandTryReconnectLabel".Translate();
				yield return command_Action;
			}
		}

		private void TryManualReconnect()
		{
			if (lastManualReconnector != this)
			{
				recentlyConnectedNets.Clear();
				lastManualReconnector = this;
			}
			if (PowerNet != null)
			{
				recentlyConnectedNets.Add(PowerNet);
			}
			CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(parent.Position, parent.Map, recentlyConnectedNets);
			if (compPower == null)
			{
				recentlyConnectedNets.Clear();
				compPower = PowerConnectionMaker.BestTransmitterForConnector(parent.Position, parent.Map);
			}
			if (compPower != null)
			{
				PowerConnectionMaker.DisconnectFromPowerNet(this);
				ConnectToTransmitter(compPower);
				for (int i = 0; i < 5; i++)
				{
					MoteMaker.ThrowMetaPuff(compPower.parent.Position.ToVector3Shifted(), compPower.parent.Map);
				}
				parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.PowerGrid);
				parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
			}
		}

		public void ConnectToTransmitter(CompPower transmitter, bool reconnectingAfterLoading = false)
		{
			if (connectParent != null && (!reconnectingAfterLoading || connectParent != transmitter))
			{
				Log.Error(string.Concat("Tried to connect ", this, " to transmitter ", transmitter, " but it's already connected to ", connectParent, "."));
			}
			else
			{
				connectParent = transmitter;
				if (connectParent.connectChildren == null)
				{
					connectParent.connectChildren = new List<CompPower>();
				}
				transmitter.connectChildren.Add(this);
				PowerNet?.RegisterConnector(this);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (PowerNet == null)
			{
				return "PowerNotConnected".Translate();
			}
			string value = (PowerNet.CurrentEnergyGainRate() / WattsToWattDaysPerTick).ToString("F0");
			string value2 = PowerNet.CurrentStoredEnergy().ToString("F0");
			return "PowerConnectedRateStored".Translate(value, value2);
		}
	}
}
