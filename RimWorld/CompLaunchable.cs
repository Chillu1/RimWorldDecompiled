using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
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

		private const float FuelPerTile = 2.25f;

		public Building FuelingPortSource => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(parent.Position, parent.Map);

		public bool ConnectedToFuelingPort => FuelingPortSource != null;

		public bool FuelingPortSourceHasAnyFuel
		{
			get
			{
				if (ConnectedToFuelingPort)
				{
					return FuelingPortSource.GetComp<CompRefuelable>().HasFuel;
				}
				return false;
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
					float fuelingPortSourceFuel = transportersInGroup[i].Launchable.FuelingPortSourceFuel;
					if (!flag || fuelingPortSourceFuel < num)
					{
						num = fuelingPortSourceFuel;
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

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (LoadingInProgressOrReadyToLaunch)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandLaunchGroup".Translate();
				command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
				command_Action.icon = LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;
				command_Action.action = delegate
				{
					if (AnyInGroupHasAnythingLeftToLoad)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(FirstThingLeftToLoadInGroup.LabelCapNoCount, FirstThingLeftToLoadInGroup), StartChoosingDestination));
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
		}

		public override string CompInspectStringExtra()
		{
			if (LoadingInProgressOrReadyToLaunch)
			{
				if (!AllInGroupConnectedToFuelingPort)
				{
					return "NotReadyForLaunch".Translate() + ": " + "NotAllInGroupConnectedToFuelingPort".Translate() + ".";
				}
				if (!AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					return "NotReadyForLaunch".Translate() + ": " + "NotAllFuelingPortSourcesInGroupHaveAnyFuel".Translate() + ".";
				}
				if (AnyInGroupHasAnythingLeftToLoad)
				{
					return "NotReadyForLaunch".Translate() + ": " + "TransportPodInGroupHasSomethingLeftToLoad".Translate() + ".";
				}
				return "ReadyForLaunch".Translate();
			}
			return null;
		}

		private void StartChoosingDestination()
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
			Find.WorldSelector.ClearSelection();
			int tile = parent.Map.Tile;
			Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: true, TargeterMouseAttachment, closeWorldTabWhenFinished: true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance);
			}, delegate(GlobalTargetInfo target)
			{
				if (!target.IsValid)
				{
					return null;
				}
				int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
				if (num > MaxLaunchDistance)
				{
					GUI.color = ColoredText.RedReadable;
					if (num > MaxLaunchDistanceEverPossible)
					{
						return "TransportPodDestinationBeyondMaximumRange".Translate();
					}
					return "TransportPodNotEnoughFuel".Translate();
				}
				IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = GetTransportPodsFloatMenuOptionsAt(target.Tile);
				if (!transportPodsFloatMenuOptionsAt.Any())
				{
					return "";
				}
				if (transportPodsFloatMenuOptionsAt.Count() == 1)
				{
					if (transportPodsFloatMenuOptionsAt.First().Disabled)
					{
						GUI.color = ColoredText.RedReadable;
					}
					return transportPodsFloatMenuOptionsAt.First().Label;
				}
				MapParent mapParent = target.WorldObject as MapParent;
				return (mapParent != null) ? ((string)"ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap)) : ((string)"ClickToSeeAvailableOrders_Empty".Translate());
			});
		}

		private bool ChoseWorldTarget(GlobalTargetInfo target)
		{
			if (!LoadingInProgressOrReadyToLaunch)
			{
				return true;
			}
			if (!target.IsValid)
			{
				Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			int num = Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile);
			if (num > MaxLaunchDistance)
			{
				Messages.Message("MessageTransportPodsDestinationIsTooFar".Translate(FuelNeededToLaunchAtDist(num).ToString("0.#")), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = GetTransportPodsFloatMenuOptionsAt(target.Tile);
			if (!transportPodsFloatMenuOptionsAt.Any())
			{
				if (Find.World.Impassable(target.Tile))
				{
					Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				TryLaunch(target.Tile, null);
				return true;
			}
			if (transportPodsFloatMenuOptionsAt.Count() == 1)
			{
				if (!transportPodsFloatMenuOptionsAt.First().Disabled)
				{
					transportPodsFloatMenuOptionsAt.First().action();
				}
				return false;
			}
			Find.WindowStack.Add(new FloatMenu(transportPodsFloatMenuOptionsAt.ToList()));
			return false;
		}

		public void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
		{
			if (!parent.Spawned)
			{
				Log.Error("Tried to launch " + parent + ", but it's unspawned.");
				return;
			}
			List<CompTransporter> transportersInGroup = TransportersInGroup;
			if (transportersInGroup == null)
			{
				Log.Error("Tried to launch " + parent + ", but it's not in any group.");
			}
			else
			{
				if (!LoadingInProgressOrReadyToLaunch || !AllInGroupConnectedToFuelingPort || !AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					return;
				}
				Map map = parent.Map;
				int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile);
				if (num <= MaxLaunchDistance)
				{
					Transporter.TryRemoveLord(map);
					int groupID = Transporter.groupID;
					float amount = Mathf.Max(FuelNeededToLaunchAtDist(num), 1f);
					for (int i = 0; i < transportersInGroup.Count; i++)
					{
						CompTransporter compTransporter = transportersInGroup[i];
						compTransporter.Launchable.FuelingPortSource?.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
						ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
						ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
						activeDropPod.Contents = new ActiveDropPodInfo();
						activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, canMergeWithExistingStacks: true, destroyLeftover: true);
						DropPodLeaving obj = (DropPodLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.DropPodLeaving, activeDropPod);
						obj.groupID = groupID;
						obj.destinationTile = destinationTile;
						obj.arrivalAction = arrivalAction;
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

		public IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile)
		{
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
				if (worldObjects[i].Tile == tile)
				{
					foreach (FloatMenuOption transportPodsFloatMenuOption in worldObjects[i].GetTransportPodsFloatMenuOptions(TransportersInGroup.Cast<IThingHolder>(), this))
					{
						anything = true;
						yield return transportPodsFloatMenuOption;
					}
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
	}
}
