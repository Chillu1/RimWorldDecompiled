using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompLaunchable : ThingComp
	{
		private CompTransporter cachedCompTransporter;

		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment");

		public static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");

		private static readonly Texture2D DismissTex = ContentFinder<Texture2D>.Get("UI/Commands/DismissShuttle");

		private const float FuelPerTile = 2.25f;

		public CompProperties_Launchable Props => (CompProperties_Launchable)props;

		public Building FuelingPortSource => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(parent.Position, parent.Map);

		public bool ConnectedToFuelingPort
		{
			get
			{
				if (Props.requireFuel)
				{
					return FuelingPortSource != null;
				}
				return true;
			}
		}

		public bool FuelingPortSourceHasAnyFuel
		{
			get
			{
				if (Props.requireFuel)
				{
					if (ConnectedToFuelingPort)
					{
						return FuelingPortSource.GetComp<CompRefuelable>().HasFuel;
					}
					return false;
				}
				return true;
			}
		}

		public bool LoadingInProgressOrReadyToLaunch => Transporter.LoadingInProgressOrReadyToLaunch;

		public bool AnythingLeftToLoad => Transporter.AnythingLeftToLoad;

		public Thing FirstThingLeftToLoad => Transporter.FirstThingLeftToLoad;

		public List<CompTransporter> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

		public bool AnyInGroupHasAnythingLeftToLoad => Transporter.AnyInGroupHasAnythingLeftToLoad;

		public Thing FirstThingLeftToLoadInGroup => Transporter.FirstThingLeftToLoadInGroup;

		public bool AnyInGroupIsUnderRoof
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

		public CompTransporter Transporter
		{
			get
			{
				if (cachedCompTransporter == null)
				{
					cachedCompTransporter = parent.GetComp<CompTransporter>();
				}
				return cachedCompTransporter;
			}
		}

		public float FuelingPortSourceFuel
		{
			get
			{
				if (!ConnectedToFuelingPort)
				{
					return 0f;
				}
				return FuelingPortSource.GetComp<CompRefuelable>().Fuel;
			}
		}

		public bool AllInGroupConnectedToFuelingPort
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					if (!transportersInGroup[i].Launchable.ConnectedToFuelingPort)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool AllFuelingPortSourcesInGroupHaveAnyFuel
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					if (!transportersInGroup[i].Launchable.FuelingPortSourceHasAnyFuel)
					{
						return false;
					}
				}
				return true;
			}
		}

		private float FuelInLeastFueledFuelingPortSource
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				float num = 0f;
				bool flag = false;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					float num2 = (Props.requireFuel ? transportersInGroup[i].Launchable.FuelingPortSourceFuel : float.PositiveInfinity);
					if (!flag || num2 < num)
					{
						num = num2;
						flag = true;
					}
				}
				if (!flag)
				{
					return 0f;
				}
				return num;
			}
		}

		private int MaxLaunchDistance
		{
			get
			{
				if (!LoadingInProgressOrReadyToLaunch)
				{
					return 0;
				}
				if (Props.fixedLaunchDistanceMax >= 0)
				{
					return Props.fixedLaunchDistanceMax;
				}
				return MaxLaunchDistanceAtFuelLevel(FuelInLeastFueledFuelingPortSource);
			}
		}

		private int MaxLaunchDistanceEverPossible
		{
			get
			{
				if (!LoadingInProgressOrReadyToLaunch)
				{
					return 0;
				}
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				float num = 0f;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					Building fuelingPortSource = transportersInGroup[i].Launchable.FuelingPortSource;
					if (fuelingPortSource != null)
					{
						num = Mathf.Max(num, fuelingPortSource.GetComp<CompRefuelable>().Props.fuelCapacity);
					}
				}
				if (Props.fixedLaunchDistanceMax >= 0)
				{
					return Props.fixedLaunchDistanceMax;
				}
				return MaxLaunchDistanceAtFuelLevel(num);
			}
		}

		private bool PodsHaveAnyPotentialCaravanOwner
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					ThingOwner innerContainer = transportersInGroup[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && CaravanUtility.IsOwner(pawn, Faction.OfPlayer))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool CanTryLaunch
		{
			get
			{
				CompShuttle compShuttle = parent.TryGetComp<CompShuttle>();
				if (compShuttle != null)
				{
					if (compShuttle.permitShuttle || compShuttle.IsMissionShuttle)
					{
						return Transporter.innerContainer.Any();
					}
					return false;
				}
				return true;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			CompShuttle shuttleComp = parent.TryGetComp<CompShuttle>();
			if (LoadingInProgressOrReadyToLaunch && CanTryLaunch)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandLaunchGroup".Translate();
				command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
				command_Action.icon = LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;
				if (shuttleComp != null && shuttleComp.IsMissionShuttle && !shuttleComp.AllRequiredThingsLoaded)
				{
					command_Action.Disable("ShuttleRequiredItemsNotSatisfied".Translate());
				}
				command_Action.action = delegate
				{
					if (AnyInGroupHasAnythingLeftToLoad)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(FirstThingLeftToLoadInGroup.LabelCapNoCount, FirstThingLeftToLoadInGroup), StartChoosingDestination));
					}
					else if (shuttleComp != null && shuttleComp.IsMissionShuttle)
					{
						TransportPodsArrivalAction_Shuttle arrivalAction = new TransportPodsArrivalAction_Shuttle((MapParent)shuttleComp.missionShuttleTarget)
						{
							missionShuttleHome = shuttleComp.missionShuttleHome,
							missionShuttleTarget = shuttleComp.missionShuttleTarget,
							sendAwayIfQuestFinished = shuttleComp.sendAwayIfQuestFinished,
							questTags = parent.questTags
						};
						TryLaunch((parent.Tile == shuttleComp.missionShuttleTarget.Tile) ? shuttleComp.missionShuttleHome.Tile : shuttleComp.missionShuttleTarget.Tile, arrivalAction);
					}
					else
					{
						StartChoosingDestination();
					}
				};
				if (!AllInGroupConnectedToFuelingPort)
				{
					command_Action.Disable("CommandLaunchGroupFailNotConnectedToFuelingPort".Translate());
				}
				else if (!AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					command_Action.Disable("CommandLaunchGroupFailNoFuel".Translate());
				}
				else if (AnyInGroupIsUnderRoof)
				{
					command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
				}
				yield return command_Action;
			}
			if (shuttleComp == null || !shuttleComp.permitShuttle)
			{
				yield break;
			}
			yield return new Command_Action
			{
				defaultLabel = "CommandShuttleDismiss".Translate(),
				defaultDesc = "CommandShuttleDismissDesc".Translate(),
				icon = DismissTex,
				alsoClickIfOtherInGroupClicked = false,
				action = delegate
				{
					Transporter.innerContainer.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
					if (!LoadingInProgressOrReadyToLaunch)
					{
						TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
					}
					shuttleComp.Send();
				}
			};
		}

		public override string CompInspectStringExtra()
		{
			CompShuttle compShuttle = parent.TryGetComp<CompShuttle>();
			if (LoadingInProgressOrReadyToLaunch && CanTryLaunch)
			{
				if (!AllInGroupConnectedToFuelingPort)
				{
					return "NotReadyForLaunch".Translate() + ": " + "NotAllInGroupConnectedToFuelingPort".Translate().CapitalizeFirst() + ".";
				}
				if (!AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					return "NotReadyForLaunch".Translate() + ": " + "NotAllFuelingPortSourcesInGroupHaveAnyFuel".Translate().CapitalizeFirst() + ".";
				}
				if (AnyInGroupHasAnythingLeftToLoad)
				{
					return "NotReadyForLaunch".Translate() + ": " + "TransportPodInGroupHasSomethingLeftToLoad".Translate().CapitalizeFirst() + ".";
				}
				if (compShuttle == null || !compShuttle.IsMissionShuttle || compShuttle.AllRequiredThingsLoaded)
				{
					return "ReadyForLaunch".Translate();
				}
			}
			return null;
		}

		public void StartChoosingDestination()
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
			Find.WorldSelector.ClearSelection();
			int tile = parent.Map.Tile;
			Find.WorldTargeter.BeginTargeting_NewTemp(ChoseWorldTarget, canTargetTiles: true, TargeterMouseAttachment, closeWorldTabWhenFinished: true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance);
			}, (GlobalTargetInfo target) => TargetingLabelGetter(target, tile, MaxLaunchDistance, TransportersInGroup.Cast<IThingHolder>(), TryLaunch, this));
		}

		private bool ChoseWorldTarget(GlobalTargetInfo target)
		{
			if (!LoadingInProgressOrReadyToLaunch)
			{
				return true;
			}
			return ChoseWorldTarget(target, parent.Map.Tile, TransportersInGroup.Cast<IThingHolder>(), MaxLaunchDistance, TryLaunch, this);
		}

		public void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
		{
			if (!parent.Spawned)
			{
				Log.Error(string.Concat("Tried to launch ", parent, ", but it's unspawned."));
				return;
			}
			List<CompTransporter> transportersInGroup = TransportersInGroup;
			if (transportersInGroup == null)
			{
				Log.Error(string.Concat("Tried to launch ", parent, ", but it's not in any group."));
			}
			else
			{
				if (!LoadingInProgressOrReadyToLaunch || !AllInGroupConnectedToFuelingPort || !AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					return;
				}
				Map map = parent.Map;
				int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile);
				CompShuttle compShuttle = parent.TryGetComp<CompShuttle>();
				if (num <= MaxLaunchDistance || (compShuttle != null && compShuttle.IsMissionShuttle))
				{
					Transporter.TryRemoveLord(map);
					int groupID = Transporter.groupID;
					float amount = Mathf.Max(FuelNeededToLaunchAtDist(num), 1f);
					compShuttle?.SendLaunchedSignals(transportersInGroup);
					for (int i = 0; i < transportersInGroup.Count; i++)
					{
						CompTransporter compTransporter = transportersInGroup[i];
						compTransporter.Launchable.FuelingPortSource?.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
						ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
						ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
						activeDropPod.Contents = new ActiveDropPodInfo();
						activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, canMergeWithExistingStacks: true, destroyLeftover: true);
						DropPodLeaving obj = (DropPodLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
						obj.groupID = groupID;
						obj.destinationTile = destinationTile;
						obj.arrivalAction = arrivalAction;
						obj.worldObjectDef = ((compShuttle != null) ? WorldObjectDefOf.TravelingShuttle : WorldObjectDefOf.TravelingTransportPods);
						compTransporter.CleanUpLoadingVars(map);
						compTransporter.parent.Destroy();
						GenSpawn.Spawn(obj, compTransporter.parent.Position, map);
					}
					CameraJumper.TryHideWorld();
				}
			}
		}

		public void Notify_FuelingPortSourceDeSpawned()
		{
			if (Transporter.CancelLoad())
			{
				Messages.Message("MessageTransportersLoadCanceled_FuelingPortGiverDeSpawned".Translate(), parent, MessageTypeDefOf.NegativeEvent);
			}
		}

		public static int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
		{
			return Mathf.FloorToInt(fuelLevel / 2.25f);
		}

		public static float FuelNeededToLaunchAtDist(float dist)
		{
			return 2.25f * dist;
		}

		private IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile)
		{
			if (parent.TryGetComp<CompShuttle>() != null)
			{
				IEnumerable<FloatMenuOption> optionsForTile = GetOptionsForTile(tile, TransportersInGroup.Cast<IThingHolder>(), TryLaunch);
				foreach (FloatMenuOption item in optionsForTile)
				{
					yield return item;
				}
				yield break;
			}
			bool anything = false;
			if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(TransportersInGroup.Cast<IThingHolder>(), tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
			{
				anything = true;
				yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
				{
					TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan());
				});
			}
			List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < worldObjects.Count; i++)
			{
				if (worldObjects[i].Tile != tile)
				{
					continue;
				}
				foreach (FloatMenuOption transportPodsFloatMenuOption in worldObjects[i].GetTransportPodsFloatMenuOptions(TransportersInGroup.Cast<IThingHolder>(), this))
				{
					anything = true;
					yield return transportPodsFloatMenuOption;
				}
			}
			if (!anything && !Find.World.Impassable(tile))
			{
				yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate
				{
					TryLaunch(tile, null);
				});
			}
		}

		public static bool ChoseWorldTarget(GlobalTargetInfo target, int tile, IEnumerable<IThingHolder> pods, int maxLaunchDistance, Action<int, TransportPodsArrivalAction> launchAction, CompLaunchable launchable)
		{
			if (!target.IsValid)
			{
				Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile) > maxLaunchDistance)
			{
				Messages.Message("TransportPodDestinationBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			IEnumerable<FloatMenuOption> source = ((launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : GetOptionsForTile(target.Tile, pods, launchAction));
			if (!source.Any())
			{
				if (Find.World.Impassable(target.Tile))
				{
					Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				launchAction(target.Tile, null);
				return true;
			}
			if (source.Count() == 1)
			{
				if (!source.First().Disabled)
				{
					source.First().action();
					return true;
				}
				return false;
			}
			Find.WindowStack.Add(new FloatMenu(source.ToList()));
			return false;
		}

		public static IEnumerable<FloatMenuOption> GetOptionsForTile(int tile, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction)
		{
			bool anything = false;
			if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(pods, tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
			{
				anything = true;
				yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
				{
					launchAction(tile, new TransportPodsArrivalAction_FormCaravan("MessageShuttleArrived"));
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

		public static string TargetingLabelGetter(GlobalTargetInfo target, int tile, int maxLaunchDistance, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction, CompLaunchable launchable)
		{
			if (!target.IsValid)
			{
				return null;
			}
			if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile) > maxLaunchDistance)
			{
				GUI.color = ColoredText.RedReadable;
				return "TransportPodDestinationBeyondMaximumRange".Translate();
			}
			IEnumerable<FloatMenuOption> source = ((launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : GetOptionsForTile(target.Tile, pods, launchAction));
			if (!source.Any())
			{
				return string.Empty;
			}
			if (source.Count() == 1)
			{
				if (source.First().Disabled)
				{
					GUI.color = ColoredText.RedReadable;
				}
				return source.First().Label;
			}
			MapParent mapParent;
			if ((mapParent = target.WorldObject as MapParent) != null)
			{
				return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
			}
			return "ClickToSeeAvailableOrders_Empty".Translate();
		}
	}
}
