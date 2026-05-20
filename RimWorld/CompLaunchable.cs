using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompLaunchable : ThingComp
{
	public int lastLaunchTick = -1;

	private CompTransporter cachedCompTransporter;

	private CompRefuelable cachedCompRefuelable;

	private CompShuttle cachedCompShuttle;

	public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment");

	public static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");

	private PlanetTile cachedClosest;

	private PlanetTile cachedOrigin;

	private PlanetLayer cachedLayer;

	public CompProperties_Launchable Props => (CompProperties_Launchable)props;

	protected CompTransporter Transporter => cachedCompTransporter ?? (cachedCompTransporter = parent.GetComp<CompTransporter>());

	public virtual CompRefuelable Refuelable => cachedCompRefuelable ?? (cachedCompRefuelable = parent.GetComp<CompRefuelable>());

	private CompShuttle Shuttle => cachedCompShuttle ?? (cachedCompShuttle = parent.GetComp<CompShuttle>());

	protected List<CompTransporter> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

	private bool AnyInGroupHasAnythingLeftToLoad => Transporter.AnyInGroupHasAnythingLeftToLoad;

	private Thing FirstThingLeftToLoadInGroup => Transporter.FirstThingLeftToLoadInGroup;

	public virtual float FuelLevel => Refuelable.Fuel;

	public virtual float MaxFuelLevel => Refuelable.Props.fuelCapacity;

	public virtual bool RequiresFuelingPort => true;

	private bool AnyInGroupIsUnderRoof
	{
		get
		{
			List<CompTransporter> transportersInGroup = TransportersInGroup;
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				if (transportersInGroup[i].parent.Position.Roofed(parent.Map))
				{
					return true;
				}
			}
			return false;
		}
	}

	private bool AllLaunchablesInGroupHaveFuelForLaunch
	{
		get
		{
			foreach (CompTransporter item in TransportersInGroup)
			{
				if (item.Launchable.FuelLevel < item.Launchable.Props.minFuelCost)
				{
					return false;
				}
				if (item.Launchable.RequiresFuelingPort && !item.Launchable.Refuelable.HasFuel)
				{
					return false;
				}
			}
			return true;
		}
	}

	private float MinFuelLevelInGroup
	{
		get
		{
			List<CompTransporter> transportersInGroup = TransportersInGroup;
			float num = 0f;
			bool flag = false;
			foreach (CompTransporter item in transportersInGroup)
			{
				float fuelLevel = item.Launchable.FuelLevel;
				if (!flag || fuelLevel < num)
				{
					num = fuelLevel;
					flag = true;
				}
			}
			if (flag)
			{
				return num;
			}
			return 0f;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastLaunchTick, "lastLaunchTick", 0);
	}

	public override void CompTick()
	{
		if (!Props.cooldownEndedMessage.NullOrEmpty() && lastLaunchTick > 0 && lastLaunchTick + Props.cooldownTicks == Find.TickManager.TicksGame)
		{
			Messages.Message(Props.cooldownEndedMessage.Formatted(parent.LabelCap), parent, MessageTypeDefOf.NeutralEvent, historical: false);
		}
	}

	public virtual AcceptanceReport CanLaunch(float? overrideFuelLevel = null)
	{
		if (!Transporter.LoadingInProgressOrReadyToLaunch)
		{
			return "CommandLaunchGroupFailNotLoaded".Translate();
		}
		if (parent.Spawned && AnyInGroupIsUnderRoof)
		{
			return "CommandLaunchGroupFailUnderRoof".Translate();
		}
		if (overrideFuelLevel.HasValue)
		{
			if (overrideFuelLevel < Props.minFuelCost)
			{
				return "CommandLaunchGroupFailNoFuel".Translate();
			}
		}
		else if (!AllLaunchablesInGroupHaveFuelForLaunch)
		{
			return "CommandLaunchGroupFailNoFuel".Translate();
		}
		if (Transporter.OverMassCapacity)
		{
			return "CommandLaunchGroupFailOverMassCapacity".Translate() + ": " + "MassUsageString".Translate(Transporter.MassUsage.ToString("F0"), Transporter.MassCapacity.ToString("F0"));
		}
		int num = Props.cooldownTicks - Find.TickManager.TicksGame + lastLaunchTick;
		if (Props.cooldownTicks > 0 && lastLaunchTick > 0 && num > 0)
		{
			return "CommandLaunchGroupCooldown".Translate() + " (" + num.ToStringTicksToPeriod() + ")";
		}
		if (Shuttle != null)
		{
			return Shuttle.CanLaunch;
		}
		return true;
	}

	public int MaxLaunchDistanceEver(PlanetLayer layer)
	{
		if (!Transporter.LoadingInProgressOrReadyToLaunch)
		{
			return 0;
		}
		int num = MaxLaunchDistanceAtFuelLevel(MaxFuelLevel, layer);
		if (Props.fixedLaunchDistanceMax >= 0)
		{
			num = Mathf.Min(num, Mathf.RoundToInt((float)Props.fixedLaunchDistanceMax / layer.Def.rangeDistanceFactor));
		}
		return num;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		CompTransporter transporter = Transporter;
		if (transporter != null && !transporter.Groupable)
		{
			int num = 0;
			foreach (object selectedObject in Find.Selector.SelectedObjects)
			{
				if (selectedObject is ThingWithComps thing && thing.HasComp<CompTransporter>())
				{
					num++;
				}
			}
			if (num > 1)
			{
				yield break;
			}
		}
		Command_Action command_Action = new Command_Action
		{
			defaultLabel = "CommandLaunchGroup".Translate(),
			defaultDesc = "CommandLaunchGroupDesc".Translate(),
			icon = LaunchCommandTex,
			alsoClickIfOtherInGroupClicked = false,
			action = delegate
			{
				if (AnyInGroupHasAnythingLeftToLoad)
				{
					TaggedString text = "ConfirmSendNotCompletelyLoadedLaunchable".Translate() + ":\n";
					text += Transporter.leftToLoad.Select((TransferableOneWay x) => x.AnyThing.LabelCap).ToLineList(" -");
					text += "\n\n" + "ConfirmSendLaunchAnyway".Translate();
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
					{
						StartChoosingDestination(TryLaunch);
					}));
				}
				else
				{
					StartChoosingDestination(TryLaunch);
				}
			}
		};
		AcceptanceReport acceptanceReport = CanLaunch();
		if (!acceptanceReport.Accepted)
		{
			command_Action.Disable(acceptanceReport.Reason);
		}
		yield return command_Action;
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: End cooldown";
			command_Action2.action = delegate
			{
				lastLaunchTick = Find.TickManager.TicksGame - Props.cooldownTicks;
			};
			yield return command_Action2;
		}
	}

	public override string CompInspectStringExtra()
	{
		if (Transporter.LoadingInProgressOrReadyToLaunch)
		{
			if (Refuelable != null && !Refuelable.HasFuel)
			{
				return "NotReadyForLaunch".Translate() + ": " + "NotAllLaunchablesInGroupHaveAnyFuel".Translate().CapitalizeFirst() + ".";
			}
			if (AnyInGroupHasAnythingLeftToLoad)
			{
				return "NotReadyForLaunch".Translate() + ": " + "LaunchableInGroupHasSomethingLeftToLoad".Translate().CapitalizeFirst() + ".";
			}
			if (!AllLaunchablesInGroupHaveFuelForLaunch)
			{
				return "NotReadyForLaunch".Translate() + ": " + "NotAllLaunchablesInGroupHaveAnyFuel".Translate().CapitalizeFirst() + ".";
			}
			int num = Props.cooldownTicks - Find.TickManager.TicksGame + lastLaunchTick;
			if (Props.cooldownTicks > 0 && lastLaunchTick > 0 && num > 0)
			{
				return "NotReadyForLaunch".Translate() + ": " + "CommandLaunchGroupCooldown".Translate() + " (" + num.ToStringTicksToPeriod() + ")";
			}
			return "ReadyForLaunch".Translate();
		}
		return null;
	}

	public void StartChoosingDestination(Action<PlanetTile, TransportersArrivalAction> launchAction, float? overrideFuelLevel = null)
	{
		PlanetTile tile = parent.Tile;
		CameraJumper.TryJump(CameraJumper.GetWorldTarget(new GlobalTargetInfo(tile)));
		Find.WorldSelector.ClearSelection();
		Find.WorldTargeter.BeginTargeting((GlobalTargetInfo t) => ChoseWorldTarget(t, launchAction, overrideFuelLevel), canTargetTiles: true, TargeterMouseAttachment, !CaravanShuttleUtility.IsCaravanShuttle(Transporter), delegate
		{
			PlanetTile planetTile;
			if (cachedLayer != Find.WorldSelector.SelectedLayer || cachedOrigin != tile)
			{
				cachedLayer = Find.WorldSelector.SelectedLayer;
				cachedOrigin = tile;
				planetTile = (cachedClosest = Find.WorldSelector.SelectedLayer.GetClosestTile_NewTemp(tile));
			}
			else
			{
				planetTile = cachedClosest;
			}
			int num = MaxLaunchDistanceEver(planetTile.Layer);
			GenDraw.DrawWorldRadiusRing(planetTile, num, CompPilotConsole.GetThrusterRadiusMat(planetTile));
			if (Refuelable != null)
			{
				int num2 = MaxLaunchDistanceAtFuelLevel(FuelLevel, PlanetLayer.Selected);
				if (num2 < num)
				{
					GenDraw.DrawWorldRadiusRing(planetTile, num2, CompPilotConsole.GetFuelRadiusMat(planetTile));
				}
			}
		}, (GlobalTargetInfo target) => TargetingLabelGetter(target, tile, MaxLaunchDistanceEver(cachedClosest.Layer), TransportersInGroup, launchAction, this, overrideFuelLevel), null, tile, showCancelButton: true);
	}

	private bool ChoseWorldTarget(GlobalTargetInfo target, Action<PlanetTile, TransportersArrivalAction> launchAction, float? overrideFuelLevel = null)
	{
		if (!Transporter.LoadingInProgressOrReadyToLaunch)
		{
			return true;
		}
		cachedClosest = (cachedOrigin = PlanetTile.Invalid);
		cachedLayer = null;
		PlanetTile tile = parent.Tile;
		return ChoseWorldTarget(target, tile, TransportersInGroup, MaxLaunchDistanceEver(target.Tile.Layer), launchAction, this, overrideFuelLevel);
	}

	public void TryLaunch(PlanetTile destinationTile, TransportersArrivalAction arrivalAction)
	{
		if (!parent.Spawned)
		{
			Log.Error($"Tried to launch {parent}, but it's unspawned.");
			return;
		}
		List<CompTransporter> transportersInGroup = TransportersInGroup;
		if (transportersInGroup == null)
		{
			Log.Error($"Tried to launch {parent}, but it's not in any group.");
		}
		else
		{
			if (!CanLaunch())
			{
				return;
			}
			Map map = parent.Map;
			int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, passImpassable: true, int.MaxValue, canTraverseLayers: true);
			Current.Game.CurrentMap = map;
			if (num > MaxLaunchDistanceAtFuelLevel(MinFuelLevelInGroup, destinationTile.Layer))
			{
				return;
			}
			Transporter.TryRemoveLord(map);
			int groupID = Transporter.groupID;
			float amount = Mathf.Max(FuelNeededToLaunchAtDist(num, destinationTile.Layer), 1f);
			lastLaunchTick = Find.TickManager.TicksGame;
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				CompTransporter compTransporter = transportersInGroup[i];
				CompRefuelable refuelable = compTransporter.Launchable.Refuelable;
				refuelable?.ConsumeFuel(amount);
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				ActiveTransporter activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(Props.activeTransporterDef ?? ThingDefOf.ActiveDropPod);
				activeTransporter.Contents = new ActiveTransporterInfo();
				activeTransporter.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, canMergeWithExistingStacks: true, destroyLeftover: true);
				activeTransporter.Contents.sentTransporterDef = parent.def;
				activeTransporter.Rotation = parent.Rotation;
				FlyShipLeaving obj = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeTransporter);
				obj.groupID = groupID;
				obj.destinationTile = destinationTile;
				obj.arrivalAction = arrivalAction;
				obj.worldObjectDef = Props.worldObjectDef ?? WorldObjectDefOf.TravellingTransporters;
				if (compTransporter.parent.HasComp<CompShuttle>())
				{
					activeTransporter.Contents.SetShuttle(compTransporter.parent);
				}
				else
				{
					compTransporter.CleanUpLoadingVars(map);
					compTransporter.parent.Destroy();
				}
				GenSpawn.Spawn(obj, compTransporter.parent.Position, map);
				if (refuelable?.parent is INotifyLaunchableLaunch notifyLaunchableLaunch)
				{
					notifyLaunchableLaunch.Notify_LaunchableLaunched(compTransporter.Launchable);
				}
			}
			CameraJumper.TryHideWorld();
		}
	}

	public void Notify_FuelingPortSourceDeSpawned()
	{
		if (Transporter.CancelLoad())
		{
			Messages.Message("MessageTransportersLoadCanceled_FuelingPortGiverDeSpawned".Translate(), parent, MessageTypeDefOf.NegativeEvent);
		}
	}

	public int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
	{
		return MaxLaunchDistanceAtFuelLevel(fuelLevel, parent.Tile.Layer);
	}

	public int MaxLaunchDistanceAtFuelLevel(float fuelLevel, PlanetLayer layer)
	{
		if (fuelLevel < Props.minFuelCost)
		{
			return 0;
		}
		int num = Mathf.FloorToInt(fuelLevel / (Props.fuelPerTile * layer.Def.rangeDistanceFactor));
		if (num <= 0)
		{
			num = int.MaxValue;
		}
		if (Props.fixedLaunchDistanceMax >= 0)
		{
			num = Mathf.Min(num, Mathf.RoundToInt((float)Props.fixedLaunchDistanceMax / layer.Def.rangeDistanceFactor));
		}
		return num;
	}

	public float FuelNeededToLaunchAtDist(float dist, PlanetLayer layer)
	{
		float num = dist * (Props.fuelPerTile * layer.Def.rangeDistanceFactor);
		if (Props.minFuelCost > 0f && num < Props.minFuelCost)
		{
			return Props.minFuelCost;
		}
		return num;
	}

	private IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptionsAt(PlanetTile tile, Action<PlanetTile, TransportersArrivalAction> launchAction, bool isShuttle)
	{
		bool anything = false;
		if (TransportersArrivalAction_FormCaravan.CanFormCaravanAt(TransportersInGroup, tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
		{
			anything = true;
			yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
			{
				launchAction(tile, new TransportersArrivalAction_FormCaravan(isShuttle ? "MessageShuttleArrived" : "MessageTransportPodsArrived"));
			});
		}
		List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile != tile)
			{
				continue;
			}
			if (isShuttle)
			{
				foreach (FloatMenuOption shuttleFloatMenuOption in worldObjects[i].GetShuttleFloatMenuOptions(TransportersInGroup, launchAction))
				{
					anything = true;
					yield return shuttleFloatMenuOption;
				}
				continue;
			}
			foreach (FloatMenuOption transportersFloatMenuOption in worldObjects[i].GetTransportersFloatMenuOptions(TransportersInGroup, launchAction))
			{
				anything = true;
				yield return transportersFloatMenuOption;
			}
		}
		if (!anything && !Find.World.Impassable(tile))
		{
			yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate
			{
				launchAction(tile, null);
			});
		}
	}

	public static bool ChoseWorldTarget(GlobalTargetInfo target, PlanetTile tile, IEnumerable<IThingHolder> pods, int maxLaunchDistance, Action<PlanetTile, TransportersArrivalAction> launchAction, CompLaunchable launchable, float? overrideFuelLevel = null)
	{
		if (!target.IsValid)
		{
			Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (target.HasWorldObject && !target.WorldObject.def.validLaunchTarget)
		{
			Messages.Message("MessageWorldObjectIsInvalid".Translate(target.WorldObject.Named("OBJECT")), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (ModsConfig.OdysseyActive && target.HasWorldObject && target.WorldObject.RequiresSignalJammerToReach)
		{
			Messages.Message("TransportPodDestinationRequiresSignalJammer".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		float fuelLevel = overrideFuelLevel ?? launchable?.FuelLevel ?? (-1f);
		int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, passImpassable: true, int.MaxValue, canTraverseLayers: true);
		float num2 = ((float?)launchable?.MaxLaunchDistanceAtFuelLevel(fuelLevel, target.Tile.Layer)) ?? (-1f);
		float fuelRequired = launchable?.FuelNeededToLaunchAtDist(num, target.Tile.Layer) ?? (-1f);
		if (maxLaunchDistance >= 0 && num > maxLaunchDistance)
		{
			Messages.Message("TransportPodDestinationBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (num2 >= 0f && (float)num > num2)
		{
			Messages.Message("TransportPodNotEnoughFuel".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		bool flag = launchable?.Shuttle != null;
		List<FloatMenuOption> list = ((launchable != null) ? launchable.GetTransportersFloatMenuOptionsAt(target.Tile, launchAction, flag) : GetOptionsForTile(target.Tile, pods, launchAction)).ToList();
		if (!list.Any())
		{
			if (Find.World.Impassable(target.Tile))
			{
				Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (flag && fuelLevel < fuelRequired * 2f)
			{
				WorldObject worldObject = target.WorldObject;
				if (worldObject == null || worldObject.Faction?.IsPlayer != true)
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmShuttleNotEnoughFuelToReturn".Translate(fuelLevel, fuelRequired * 2f), delegate
					{
						launchAction(target.Tile, null);
					}));
					return true;
				}
			}
			launchAction(target.Tile, null);
			return true;
		}
		if (flag && fuelLevel < fuelRequired * 2f)
		{
			WorldObject worldObject2 = target.WorldObject;
			if (worldObject2 == null || worldObject2.Faction?.IsPlayer != true)
			{
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					Action oldAction = list[num3].action;
					list[num3].action = delegate
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmShuttleNotEnoughFuelToReturn".Translate(fuelLevel, fuelRequired * 2f), oldAction));
					};
				}
			}
		}
		if (list.Count == 1)
		{
			if (!list.First().Disabled)
			{
				list.First().action();
				return true;
			}
			return false;
		}
		Find.WindowStack.Add(new FloatMenu(list));
		return false;
	}

	public static IEnumerable<FloatMenuOption> GetOptionsForTile(PlanetTile tile, IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
	{
		bool anything = false;
		if (TransportersArrivalAction_FormCaravan.CanFormCaravanAt(pods, tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
		{
			anything = true;
			yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
			{
				launchAction(tile, new TransportersArrivalAction_FormCaravan("MessageShuttleArrived"));
			});
		}
		List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile != tile)
			{
				continue;
			}
			foreach (FloatMenuOption shuttleFloatMenuOption in worldObjects[i].GetShuttleFloatMenuOptions(pods, launchAction))
			{
				anything = true;
				yield return shuttleFloatMenuOption;
			}
		}
		if (!anything && !Find.World.Impassable(tile))
		{
			yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate
			{
				launchAction(tile, null);
			});
		}
	}

	public static TaggedString TargetingLabelGetter(GlobalTargetInfo target, PlanetTile tile, int maxLaunchDistance, IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction, CompLaunchable launchable, float? overrideFuelLevel = null)
	{
		if (!target.IsValid)
		{
			return null;
		}
		if (target.Tile.Layer != tile.Layer && !tile.Layer.HasConnectionPathTo(target.Tile.Layer))
		{
			GUI.color = ColorLibrary.RedReadable;
			return "TransportPodDestinationNoPath".Translate(target.Tile.Layer.Def.Named("LAYER"));
		}
		if (ModsConfig.OdysseyActive)
		{
			WorldObject worldObject = Find.World.worldObjects.WorldObjectAt<WorldObject>(target.Tile);
			if (worldObject != null && worldObject.RequiresSignalJammerToReach)
			{
				GUI.color = ColorLibrary.RedReadable;
				return "TransportPodDestinationRequiresSignalJammer".Translate();
			}
		}
		int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, passImpassable: true, maxLaunchDistance, canTraverseLayers: true);
		if (maxLaunchDistance > 0 && num > maxLaunchDistance)
		{
			GUI.color = ColorLibrary.RedReadable;
			return "TransportPodDestinationBeyondMaximumRange".Translate();
		}
		bool flag = launchable?.Shuttle != null;
		string text = "";
		if (flag)
		{
			float num2 = overrideFuelLevel ?? launchable.FuelLevel;
			float num3 = launchable.FuelNeededToLaunchAtDist(num, target.Tile.Layer);
			text = string.Format("{0}: {1}", "Cost".Translate().CapitalizeFirst(), "FuelAmount".Translate(num3, ThingDefOf.Chemfuel));
			if (num3 > num2)
			{
				text = (text + string.Format(" ({0})", "TransportPodNotEnoughFuel".Translate())).Colorize(ColorLibrary.RedReadable);
			}
		}
		if (target.HasWorldObject && !target.WorldObject.def.validLaunchTarget)
		{
			return string.Empty;
		}
		IEnumerable<FloatMenuOption> source = ((launchable != null) ? launchable.GetTransportersFloatMenuOptionsAt(target.Tile, launchAction, flag) : GetOptionsForTile(target.Tile, pods, launchAction));
		if (!source.Any())
		{
			return string.Empty;
		}
		if (source.Count() == 1)
		{
			if (source.First().Disabled)
			{
				GUI.color = ColorLibrary.RedReadable;
			}
			return source.First().Label + "\n" + text;
		}
		if (target.WorldObject is MapParent mapParent)
		{
			return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap) + "\n" + text;
		}
		return "ClickToSeeAvailableOrders_Empty".Translate() + "\n" + text;
	}

	public void Notify_Arrived()
	{
		lastLaunchTick = Find.TickManager.TicksGame;
	}
}
