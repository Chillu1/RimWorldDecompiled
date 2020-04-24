using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompTransporter : ThingComp, IThingHolder
	{
		public int groupID = -1;

		public ThingOwner innerContainer;

		public List<TransferableOneWay> leftToLoad;

		private bool notifiedCantLoadMore;

		private CompLaunchable cachedCompLaunchable;

		private CompShuttle cachedCompShuttle;

		private static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

		private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter");

		private static readonly Texture2D SelectPreviousInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter");

		private static readonly Texture2D SelectAllInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters");

		private static readonly Texture2D SelectNextInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter");

		private static List<CompTransporter> tmpTransportersInGroup = new List<CompTransporter>();

		public CompProperties_Transporter Props => (CompProperties_Transporter)props;

		public Map Map => parent.MapHeld;

		public bool AnythingLeftToLoad => FirstThingLeftToLoad != null;

		public bool LoadingInProgressOrReadyToLaunch => groupID >= 0;

		public bool AnyInGroupHasAnythingLeftToLoad => FirstThingLeftToLoadInGroup != null;

		public CompLaunchable Launchable
		{
			get
			{
				if (cachedCompLaunchable == null)
				{
					cachedCompLaunchable = parent.GetComp<CompLaunchable>();
				}
				return cachedCompLaunchable;
			}
		}

		public CompShuttle Shuttle
		{
			get
			{
				if (cachedCompShuttle == null)
				{
					cachedCompShuttle = parent.GetComp<CompShuttle>();
				}
				return cachedCompShuttle;
			}
		}

		public Thing FirstThingLeftToLoad
		{
			get
			{
				if (leftToLoad == null)
				{
					return null;
				}
				for (int i = 0; i < leftToLoad.Count; i++)
				{
					if (leftToLoad[i].CountToTransfer != 0 && leftToLoad[i].HasAnyThing)
					{
						return leftToLoad[i].AnyThing;
					}
				}
				return null;
			}
		}

		public Thing FirstThingLeftToLoadInGroup
		{
			get
			{
				List<CompTransporter> list = TransportersInGroup(parent.Map);
				for (int i = 0; i < list.Count; i++)
				{
					Thing firstThingLeftToLoad = list[i].FirstThingLeftToLoad;
					if (firstThingLeftToLoad != null)
					{
						return firstThingLeftToLoad;
					}
				}
				return null;
			}
		}

		public bool AnyInGroupNotifiedCantLoadMore
		{
			get
			{
				List<CompTransporter> list = TransportersInGroup(parent.Map);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].notifiedCantLoadMore)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AnyPawnCanLoadAnythingNow
		{
			get
			{
				if (!AnythingLeftToLoad)
				{
					return false;
				}
				if (!parent.Spawned)
				{
					return false;
				}
				List<Pawn> allPawnsSpawned = parent.Map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
					{
						CompTransporter transporter = ((JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
						if (transporter != null && transporter.groupID == groupID)
						{
							return true;
						}
					}
					if (allPawnsSpawned[i].CurJobDef == JobDefOf.EnterTransporter)
					{
						CompTransporter transporter2 = ((JobDriver_EnterTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
						if (transporter2 != null && transporter2.groupID == groupID)
						{
							return true;
						}
					}
				}
				List<CompTransporter> list = TransportersInGroup(parent.Map);
				for (int j = 0; j < allPawnsSpawned.Count; j++)
				{
					if (allPawnsSpawned[j].mindState.duty != null && allPawnsSpawned[j].mindState.duty.transportersGroup == groupID)
					{
						CompTransporter compTransporter = JobGiver_EnterTransporter.FindMyTransporter(list, allPawnsSpawned[j]);
						if (compTransporter != null && allPawnsSpawned[j].CanReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly))
						{
							return true;
						}
					}
				}
				for (int k = 0; k < allPawnsSpawned.Count; k++)
				{
					if (!allPawnsSpawned[k].IsColonist)
					{
						continue;
					}
					for (int l = 0; l < list.Count; l++)
					{
						if (LoadTransportersJobUtility.HasJobOnTransporter(allPawnsSpawned[k], list[l]))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public CompTransporter()
		{
			innerContainer = new ThingOwner<Thing>(this);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref groupID, "groupID", 0);
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Collections.Look(ref leftToLoad, "leftToLoad", LookMode.Deep);
			Scribe_Values.Look(ref notifiedCantLoadMore, "notifiedCantLoadMore", defaultValue: false);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void CompTick()
		{
			base.CompTick();
			innerContainer.ThingOwnerTick();
			if (Props.restEffectiveness != 0f)
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					Pawn pawn = innerContainer[i] as Pawn;
					if (pawn != null && !pawn.Dead && pawn.needs.rest != null)
					{
						pawn.needs.rest.TickResting(Props.restEffectiveness);
					}
				}
			}
			if (parent.IsHashIntervalTick(60) && parent.Spawned && LoadingInProgressOrReadyToLaunch && AnyInGroupHasAnythingLeftToLoad && !AnyInGroupNotifiedCantLoadMore && !AnyPawnCanLoadAnythingNow && (Shuttle == null || !Shuttle.Autoload))
			{
				notifiedCantLoadMore = true;
				Messages.Message("MessageCantLoadMoreIntoTransporters".Translate(FirstThingLeftToLoadInGroup.LabelNoCount, Faction.OfPlayer.def.pawnsPlural, FirstThingLeftToLoadInGroup), parent, MessageTypeDefOf.CautionInput);
			}
		}

		public List<CompTransporter> TransportersInGroup(Map map)
		{
			if (!LoadingInProgressOrReadyToLaunch)
			{
				return null;
			}
			TransporterUtility.GetTransportersInGroup(groupID, map, tmpTransportersInGroup);
			return tmpTransportersInGroup;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (Shuttle != null && !Shuttle.ShowLoadingGizmos)
			{
				yield break;
			}
			if (LoadingInProgressOrReadyToLaunch)
			{
				if (Shuttle == null || !Shuttle.Autoload)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "CommandCancelLoad".Translate();
					command_Action.defaultDesc = "CommandCancelLoadDesc".Translate();
					command_Action.icon = CancelLoadCommandTex;
					command_Action.action = delegate
					{
						SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
						CancelLoad();
					};
					yield return command_Action;
				}
				if (!Props.max1PerGroup)
				{
					Command_Action command_Action2 = new Command_Action();
					command_Action2.defaultLabel = "CommandSelectPreviousTransporter".Translate();
					command_Action2.defaultDesc = "CommandSelectPreviousTransporterDesc".Translate();
					command_Action2.icon = SelectPreviousInGroupCommandTex;
					command_Action2.action = delegate
					{
						SelectPreviousInGroup();
					};
					yield return command_Action2;
					Command_Action command_Action3 = new Command_Action();
					command_Action3.defaultLabel = "CommandSelectAllTransporters".Translate();
					command_Action3.defaultDesc = "CommandSelectAllTransportersDesc".Translate();
					command_Action3.icon = SelectAllInGroupCommandTex;
					command_Action3.action = delegate
					{
						SelectAllInGroup();
					};
					yield return command_Action3;
					Command_Action command_Action4 = new Command_Action();
					command_Action4.defaultLabel = "CommandSelectNextTransporter".Translate();
					command_Action4.defaultDesc = "CommandSelectNextTransporterDesc".Translate();
					command_Action4.icon = SelectNextInGroupCommandTex;
					command_Action4.action = delegate
					{
						SelectNextInGroup();
					};
					yield return command_Action4;
				}
				if (Props.canChangeAssignedThingsAfterStarting && (Shuttle == null || !Shuttle.Autoload))
				{
					Command_LoadToTransporter command_LoadToTransporter = new Command_LoadToTransporter();
					command_LoadToTransporter.defaultLabel = "CommandSetToLoadTransporter".Translate();
					command_LoadToTransporter.defaultDesc = "CommandSetToLoadTransporterDesc".Translate();
					command_LoadToTransporter.icon = LoadCommandTex;
					command_LoadToTransporter.transComp = this;
					yield return command_LoadToTransporter;
				}
				yield break;
			}
			Command_LoadToTransporter command_LoadToTransporter2 = new Command_LoadToTransporter();
			if (Props.max1PerGroup)
			{
				if (Props.canChangeAssignedThingsAfterStarting)
				{
					command_LoadToTransporter2.defaultLabel = "CommandSetToLoadTransporter".Translate();
					command_LoadToTransporter2.defaultDesc = "CommandSetToLoadTransporterDesc".Translate();
				}
				else
				{
					command_LoadToTransporter2.defaultLabel = "CommandLoadTransporterSingle".Translate();
					command_LoadToTransporter2.defaultDesc = "CommandLoadTransporterSingleDesc".Translate();
				}
			}
			else
			{
				int num = 0;
				for (int i = 0; i < Find.Selector.NumSelected; i++)
				{
					Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
					if (thing != null && thing.def == parent.def)
					{
						CompLaunchable compLaunchable = thing.TryGetComp<CompLaunchable>();
						if (compLaunchable == null || (compLaunchable.FuelingPortSource != null && compLaunchable.FuelingPortSourceHasAnyFuel))
						{
							num++;
						}
					}
				}
				command_LoadToTransporter2.defaultLabel = "CommandLoadTransporter".Translate(num.ToString());
				command_LoadToTransporter2.defaultDesc = "CommandLoadTransporterDesc".Translate();
			}
			command_LoadToTransporter2.icon = LoadCommandTex;
			command_LoadToTransporter2.transComp = this;
			CompLaunchable launchable = Launchable;
			if (launchable != null)
			{
				if (!launchable.ConnectedToFuelingPort)
				{
					command_LoadToTransporter2.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
				}
				else if (!launchable.FuelingPortSourceHasAnyFuel)
				{
					command_LoadToTransporter2.Disable("CommandLoadTransporterFailNoFuel".Translate());
				}
			}
			yield return command_LoadToTransporter2;
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (CancelLoad(map))
			{
				if (Props.max1PerGroup)
				{
					Messages.Message("MessageTransporterSingleLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent);
				}
				else
				{
					Messages.Message("MessageTransportersLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent);
				}
			}
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
		}

		public override string CompInspectStringExtra()
		{
			return "Contents".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
		}

		public void AddToTheToLoadList(TransferableOneWay t, int count)
		{
			if (!t.HasAnyThing || count <= 0)
			{
				return;
			}
			if (leftToLoad == null)
			{
				leftToLoad = new List<TransferableOneWay>();
			}
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t.AnyThing, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay != null)
			{
				for (int i = 0; i < t.things.Count; i++)
				{
					if (!transferableOneWay.things.Contains(t.things[i]))
					{
						transferableOneWay.things.Add(t.things[i]);
					}
				}
				if (transferableOneWay.CanAdjustBy(count).Accepted)
				{
					transferableOneWay.AdjustBy(count);
				}
			}
			else
			{
				TransferableOneWay transferableOneWay2 = new TransferableOneWay();
				leftToLoad.Add(transferableOneWay2);
				transferableOneWay2.things.AddRange(t.things);
				transferableOneWay2.AdjustTo(count);
			}
		}

		public void Notify_ThingAdded(Thing t)
		{
			SubtractFromToLoadList(t, t.stackCount);
			if (Props.pawnLoadedSound != null && t is Pawn)
			{
				Props.pawnLoadedSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			}
		}

		public void Notify_ThingAddedAndMergedWith(Thing t, int mergedCount)
		{
			SubtractFromToLoadList(t, mergedCount);
		}

		public bool CancelLoad()
		{
			return CancelLoad(Map);
		}

		public bool CancelLoad(Map map)
		{
			if (!LoadingInProgressOrReadyToLaunch)
			{
				return false;
			}
			TryRemoveLord(map);
			List<CompTransporter> list = TransportersInGroup(map);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].CleanUpLoadingVars(map);
			}
			CleanUpLoadingVars(map);
			return true;
		}

		public void TryRemoveLord(Map map)
		{
			if (LoadingInProgressOrReadyToLaunch)
			{
				Lord lord = TransporterUtility.FindLord(groupID, map);
				if (lord != null)
				{
					map.lordManager.RemoveLord(lord);
				}
			}
		}

		public void CleanUpLoadingVars(Map map)
		{
			groupID = -1;
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
			if (leftToLoad != null)
			{
				leftToLoad.Clear();
			}
			Shuttle?.CleanUpLoadingVars();
		}

		public int SubtractFromToLoadList(Thing t, int count, bool sendMessageOnFinished = true)
		{
			if (leftToLoad == null)
			{
				return 0;
			}
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(t, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay == null)
			{
				return 0;
			}
			if (transferableOneWay.CountToTransfer <= 0)
			{
				return 0;
			}
			int num = Mathf.Min(count, transferableOneWay.CountToTransfer);
			transferableOneWay.AdjustBy(-num);
			if (transferableOneWay.CountToTransfer <= 0)
			{
				leftToLoad.Remove(transferableOneWay);
			}
			if (sendMessageOnFinished && !AnyInGroupHasAnythingLeftToLoad)
			{
				CompShuttle comp = parent.GetComp<CompShuttle>();
				if (comp == null || comp.AllRequiredThingsLoaded)
				{
					if (Props.max1PerGroup)
					{
						Messages.Message("MessageFinishedLoadingTransporterSingle".Translate(), parent, MessageTypeDefOf.TaskCompletion);
					}
					else
					{
						Messages.Message("MessageFinishedLoadingTransporters".Translate(), parent, MessageTypeDefOf.TaskCompletion);
					}
				}
			}
			return num;
		}

		private void SelectPreviousInGroup()
		{
			List<CompTransporter> list = TransportersInGroup(Map);
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
		}

		private void SelectAllInGroup()
		{
			List<CompTransporter> list = TransportersInGroup(Map);
			Selector selector = Find.Selector;
			selector.ClearSelection();
			for (int i = 0; i < list.Count; i++)
			{
				selector.Select(list[i].parent);
			}
		}

		private void SelectNextInGroup()
		{
			List<CompTransporter> list = TransportersInGroup(Map);
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
		}
	}
}
