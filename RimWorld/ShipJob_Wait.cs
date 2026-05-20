using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class ShipJob_Wait : ShipJob
{
	public bool leaveImmediatelyWhenSatisfied;

	public bool showGizmos = true;

	public bool targetPlayerSettlement;

	public List<Thing> sendAwayIfAllDespawned;

	public List<Thing> sendAwayIfAnyDespawnedDownedOrDead;

	private static readonly Texture2D DismissTex = ContentFinder<Texture2D>.Get("UI/Commands/DismissShuttle");

	protected static readonly Texture2D SendCommandTex = CompLaunchable.LaunchCommandTex;

	private const int CheckAllDespawnedInterval = 60;

	public override bool ShowGizmos => showGizmos;

	private int MaxLaunchDistance => transportShip.def.maxLaunchDistance;

	public override IEnumerable<Gizmo> GetJobGizmos()
	{
		if (transportShip.ShuttleComp.permitShuttle)
		{
			if (transportShip.TransporterComp.LoadingInProgressOrReadyToLaunch && transportShip.TransporterComp.innerContainer.Any())
			{
				yield return LaunchAction();
			}
			yield return new Command_Action
			{
				defaultLabel = "CommandShuttleDismiss".Translate(),
				defaultDesc = "CommandShuttleDismissDesc".Translate(),
				icon = DismissTex,
				alsoClickIfOtherInGroupClicked = false,
				action = delegate
				{
					transportShip.ForceJob(ShipJobDefOf.Unload);
					transportShip.AddJob(ShipJobDefOf.FlyAway);
				}
			};
		}
		else if (!leaveImmediatelyWhenSatisfied)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandSendShuttle".Translate();
			command_Action.defaultDesc = "CommandSendShuttleDesc".Translate();
			command_Action.icon = SendCommandTex;
			command_Action.alsoClickIfOtherInGroupClicked = false;
			command_Action.action = delegate
			{
				transportShip.ForceJob(ShipJobDefOf.FlyAway);
			};
			if (!transportShip.ShuttleComp.AllRequiredThingsLoaded)
			{
				command_Action.Disable("CommandSendShuttleFailMissingRequiredThing".Translate());
			}
			yield return command_Action;
		}
	}

	private Command_Action LaunchAction()
	{
		return new Command_Action
		{
			defaultLabel = "CommandLaunchGroup".Translate(),
			defaultDesc = "CommandLaunchGroupDesc".Translate(),
			icon = CompLaunchable.LaunchCommandTex,
			alsoClickIfOtherInGroupClicked = false,
			action = delegate
			{
				CameraJumper.TryJump(CameraJumper.GetWorldTarget(transportShip.shipThing));
				Find.WorldSelector.ClearSelection();
				PlanetTile tile = transportShip.shipThing.Map.Tile;
				Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: true, CompLaunchable.TargeterMouseAttachment, closeWorldTabWhenFinished: true, delegate
				{
					PlanetTile planetTile = tile;
					if (planetTile.Layer != Find.WorldSelector.SelectedLayer)
					{
						planetTile = Find.WorldSelector.SelectedLayer.GetClosestTile_NewTemp(planetTile);
					}
					GenDraw.DrawWorldRadiusRing(planetTile, MaxLaunchDistance);
				}, (GlobalTargetInfo target) => CompLaunchable.TargetingLabelGetter(target, tile, MaxLaunchDistance, Gen.YieldSingle((IThingHolder)transportShip.TransporterComp), Launch, null));
			}
		};
	}

	private bool ChoseWorldTarget(GlobalTargetInfo target)
	{
		PlanetTile tile = transportShip.shipThing.Map.Tile;
		return CompLaunchable.ChoseWorldTarget(target, tile, Gen.YieldSingle((IThingHolder)transportShip.TransporterComp), MaxLaunchDistance, Launch, null);
	}

	private void Launch(PlanetTile destinationTile, TransportersArrivalAction arrivalAction)
	{
		ShipJob_FlyAway shipJob_FlyAway = (ShipJob_FlyAway)ShipJobMaker.MakeShipJob(ShipJobDefOf.FlyAway);
		shipJob_FlyAway.destinationTile = destinationTile;
		shipJob_FlyAway.arrivalAction = arrivalAction;
		transportShip.SetNextJob(shipJob_FlyAway);
		transportShip.TryGetNextJob();
		CameraJumper.TryHideWorld();
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (!transportShip.ShipExistsAndIsSpawned)
		{
			return;
		}
		if (leaveImmediatelyWhenSatisfied && transportShip.ShuttleComp.AllRequiredThingsLoaded)
		{
			SendAway();
		}
		else
		{
			if (!transportShip.shipThing.IsHashIntervalTick(60, delta))
			{
				return;
			}
			bool flag = false;
			if (!sendAwayIfAnyDespawnedDownedOrDead.NullOrEmpty())
			{
				foreach (Thing item in sendAwayIfAnyDespawnedDownedOrDead)
				{
					Pawn pawn = item as Pawn;
					if ((!item.Spawned || (pawn != null && (pawn.Dead || pawn.Downed))) && !transportShip.TransporterComp.innerContainer.Contains(item))
					{
						flag = true;
						SendAway();
						break;
					}
				}
			}
			if (flag || sendAwayIfAllDespawned.NullOrEmpty())
			{
				return;
			}
			bool flag2 = false;
			foreach (Thing item2 in sendAwayIfAllDespawned)
			{
				if (item2.MapHeld == transportShip.shipThing.Map || item2.MapHeld.PocketMapParent.sourceMap == transportShip.shipThing.Map || transportShip.TransporterComp.innerContainer.Contains(item2))
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				SendAway();
			}
		}
	}

	protected virtual void SendAway()
	{
		transportShip.SetNextJob(ShipJobMaker.MakeShipJob(ShipJobDefOf.FlyAway));
		End();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref leaveImmediatelyWhenSatisfied, "leaveImmediatelyWhenSatisfied", defaultValue: false);
		Scribe_Values.Look(ref showGizmos, "showGizmos", defaultValue: true);
		Scribe_Values.Look(ref targetPlayerSettlement, "targetPlayerSettlement", defaultValue: false);
		Scribe_Collections.Look(ref sendAwayIfAllDespawned, "sendAwayIfAllDespawned", LookMode.Reference);
		Scribe_Collections.Look(ref sendAwayIfAnyDespawnedDownedOrDead, "sendAwayIfAnyDespawned", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			sendAwayIfAllDespawned?.RemoveAll((Thing x) => x == null);
			sendAwayIfAnyDespawnedDownedOrDead?.RemoveAll((Thing x) => x == null);
		}
	}
}
