using System;
using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompTransporter : ThingComp, IThingHolder, ISearchableContents
{
	public int groupID = -1;

	public ThingOwner innerContainer;

	public List<TransferableOneWay> leftToLoad;

	private bool notifiedCantLoadMore;

	public float massCapacityOverride = -1f;

	private CompLaunchable cachedCompLaunchable;

	private CompShuttle cachedCompShuttle;

	private bool massUsageDirty = true;

	private float cachedMassUsage;

	public static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter");

	private static readonly Texture2D SelectPreviousInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter");

	private static readonly Texture2D SelectAllInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters");

	private static readonly Texture2D SelectNextInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter");

	private List<Thing> tmpThings = new List<Thing>();

	private List<Pawn> tmpSavedPawns = new List<Pawn>();

	private static List<CompTransporter> tmpTransportersInGroup = new List<CompTransporter>();

	public CompProperties_Transporter Props => (CompProperties_Transporter)props;

	public Map Map => parent.MapHeld;

	public bool AnythingLeftToLoad => FirstThingLeftToLoad != null;

	public bool LoadingInProgressOrReadyToLaunch
	{
		get
		{
			if (groupID < 0)
			{
				return parent.IsInCaravan();
			}
			return true;
		}
	}

	public bool AnyInGroupHasAnythingLeftToLoad => FirstThingLeftToLoadInGroup != null;

	public float MassCapacity
	{
		get
		{
			if (!(massCapacityOverride <= 0f))
			{
				return massCapacityOverride;
			}
			return Props.massCapacity;
		}
	}

	public ThingOwner SearchableContents => innerContainer;

	public bool Groupable => !Props.max1PerGroup;

	public bool OverMassCapacity => MassUsage > MassCapacity;

	public float MassUsage
	{
		get
		{
			if (massUsageDirty)
			{
				massUsageDirty = false;
				cachedMassUsage = CollectionsMassCalculator.MassUsage(innerContainer, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMass: true);
			}
			return cachedMassUsage;
		}
	}

	public CompLaunchable Launchable => cachedCompLaunchable ?? (cachedCompLaunchable = parent.GetComp<CompLaunchable>());

	public CompShuttle Shuttle => cachedCompShuttle ?? (cachedCompShuttle = parent.GetComp<CompShuttle>());

	public virtual bool RequiresFuelingPort => true;

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
			if (list == null)
			{
				return null;
			}
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
			if (list == null)
			{
				return false;
			}
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
			IReadOnlyList<Pawn> allPawnsSpawned = parent.Map.mapPawns.AllPawnsSpawned;
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
			if (list == null)
			{
				return false;
			}
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
		bool flag = !parent.SpawnedOrAnyParentSpawned;
		if (flag && Scribe.mode == LoadSaveMode.Saving)
		{
			tmpThings.Clear();
			tmpThings.AddRange(innerContainer);
			tmpSavedPawns.Clear();
			for (int i = 0; i < tmpThings.Count; i++)
			{
				if (tmpThings[i] is Pawn pawn)
				{
					innerContainer.Remove(pawn);
					tmpSavedPawns.Add(pawn);
					if (!pawn.IsWorldPawn())
					{
						Log.Error("Trying to save a non-world pawn (" + pawn?.ToString() + ") as a reference in a transporter.");
					}
				}
			}
			tmpThings.Clear();
		}
		Scribe_Collections.Look(ref tmpSavedPawns, "tmpSavedPawns", LookMode.Reference);
		Scribe_Values.Look(ref groupID, "groupID", 0);
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Collections.Look(ref leftToLoad, "leftToLoad", LookMode.Deep);
		Scribe_Values.Look(ref notifiedCantLoadMore, "notifiedCantLoadMore", defaultValue: false);
		Scribe_Values.Look(ref massCapacityOverride, "massCapacityOverride", 0f);
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			leftToLoad?.RemoveWhere((TransferableOneWay t) => t == null);
		}
		if (flag && (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving))
		{
			for (int num = 0; num < tmpSavedPawns.Count; num++)
			{
				innerContainer.TryAdd(tmpSavedPawns[num]);
			}
			tmpSavedPawns.Clear();
		}
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
		if (Props.shouldTickContents)
		{
			innerContainer.DoTick();
		}
		else if (Props.restEffectiveness != 0f)
		{
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (innerContainer[i] is Pawn { Dead: false } pawn && pawn.needs.rest != null)
				{
					pawn.needs.rest.TickResting(Props.restEffectiveness);
				}
			}
		}
		if (!parent.IsHashIntervalTick(60) || !parent.Spawned)
		{
			return;
		}
		CompShuttle shuttle = Shuttle;
		if ((shuttle == null || !shuttle.Autoload) && LoadingInProgressOrReadyToLaunch && AnyInGroupHasAnythingLeftToLoad && !AnyInGroupNotifiedCantLoadMore && !AnyPawnCanLoadAnythingNow)
		{
			notifiedCantLoadMore = true;
			if (Shuttle != null)
			{
				Messages.Message("MessageCantLoadMoreIntoShuttle".Translate(FirstThingLeftToLoadInGroup.LabelNoCount, Faction.OfPlayer.def.pawnsPlural, FirstThingLeftToLoadInGroup), parent, MessageTypeDefOf.CautionInput);
			}
			else
			{
				Messages.Message("MessageCantLoadMoreIntoTransporters".Translate(FirstThingLeftToLoadInGroup.LabelNoCount, Faction.OfPlayer.def.pawnsPlural, FirstThingLeftToLoadInGroup), parent, MessageTypeDefOf.CautionInput);
			}
		}
	}

	public List<CompTransporter> TransportersInGroup(Map map)
	{
		tmpTransportersInGroup.Clear();
		if (!parent.Spawned)
		{
			tmpTransportersInGroup.Add(this);
			return tmpTransportersInGroup;
		}
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
		int num = 0;
		foreach (object selectedObject in Find.Selector.SelectedObjects)
		{
			if (selectedObject is ThingWithComps thing && thing.HasComp<CompTransporter>())
			{
				num++;
			}
		}
		if (Shuttle != null && (!Shuttle.ShowLoadingGizmos || num > 1))
		{
			yield break;
		}
		if (LoadingInProgressOrReadyToLaunch)
		{
			CompShuttle shuttle = Shuttle;
			if ((shuttle == null || !shuttle.Autoload) && innerContainer.Any)
			{
				TaggedString taggedString = (AnythingLeftToLoad ? "CommandCancelLoad".Translate() : "CommandUnload".Translate());
				TaggedString taggedString2 = (AnythingLeftToLoad ? "CommandCancelLoadDesc".Translate() : "CommandUnloadDesc".Translate(parent.LabelShort));
				Texture2D cancelLoadCommandTex = CancelLoadCommandTex;
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = taggedString;
				command_Action.defaultDesc = taggedString2;
				command_Action.icon = cancelLoadCommandTex;
				command_Action.action = delegate
				{
					SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
					CancelLoad();
				};
				yield return command_Action;
			}
			if (Groupable)
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "CommandSelectPreviousTransporter".Translate();
				command_Action2.defaultDesc = "CommandSelectPreviousTransporterDesc".Translate();
				command_Action2.icon = SelectPreviousInGroupCommandTex;
				command_Action2.action = SelectPreviousInGroup;
				yield return command_Action2;
				Command_Action command_Action3 = new Command_Action();
				command_Action3.defaultLabel = "CommandSelectAllTransporters".Translate();
				command_Action3.defaultDesc = "CommandSelectAllTransportersDesc".Translate();
				command_Action3.icon = SelectAllInGroupCommandTex;
				command_Action3.action = SelectAllInGroup;
				yield return command_Action3;
				Command_Action command_Action4 = new Command_Action();
				command_Action4.defaultLabel = "CommandSelectNextTransporter".Translate();
				command_Action4.defaultDesc = "CommandSelectNextTransporterDesc".Translate();
				command_Action4.icon = SelectNextInGroupCommandTex;
				command_Action4.action = SelectNextInGroup;
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
		if (!Groupable)
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
			int num2 = 0;
			for (int num3 = 0; num3 < Find.Selector.NumSelected; num3++)
			{
				if (!(Find.Selector.SelectedObjectsListForReading[num3] is Thing thing2) || thing2.def != parent.def)
				{
					continue;
				}
				CompLaunchable compLaunchable = thing2.TryGetComp<CompLaunchable>();
				if (compLaunchable != null)
				{
					CompRefuelable refuelable = compLaunchable.Refuelable;
					if (refuelable == null || !refuelable.HasFuel)
					{
						continue;
					}
				}
				num2++;
			}
			command_LoadToTransporter2.defaultLabel = "CommandLoadTransporter".Translate(num2.ToString());
			command_LoadToTransporter2.defaultDesc = "CommandLoadTransporterDesc".Translate();
		}
		command_LoadToTransporter2.icon = LoadCommandTex;
		command_LoadToTransporter2.transComp = this;
		CompLaunchable launchable = Launchable;
		if (launchable != null && launchable.RequiresFuelingPort)
		{
			CompLaunchable_TransportPod obj = launchable as CompLaunchable_TransportPod;
			if (obj != null && !obj.ConnectedToFuelingPort)
			{
				command_LoadToTransporter2.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
			}
			else if (!launchable.Refuelable.HasFuel && Shuttle == null)
			{
				command_LoadToTransporter2.Disable("CommandLoadTransporterFailNoFuel".Translate());
			}
		}
		yield return command_LoadToTransporter2;
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		if (parent.BeingTransportedOnGravship || (Launchable != null && Launchable.lastLaunchTick == Find.TickManager.TicksGame))
		{
			return;
		}
		if (CancelLoad(map) && Shuttle == null)
		{
			if (!Groupable)
			{
				Messages.Message("MessageTransporterSingleLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent);
			}
			else
			{
				Messages.Message("MessageTransportersLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent);
			}
		}
		if (mode != DestroyMode.WillReplace)
		{
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
		}
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Contents".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst());
		if (Props.showMassInInspectString)
		{
			TaggedString taggedString = "Mass".Translate() + ": " + MassUsage.ToString("F0") + " / " + MassCapacity.ToString("F0") + " kg";
			stringBuilder.AppendLine().Append((MassUsage > MassCapacity) ? taggedString.Colorize(ColorLibrary.RedReadable) : ((string)taggedString));
		}
		return stringBuilder.ToString();
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

	public bool LeftToLoadContains(Thing thing)
	{
		if (leftToLoad == null)
		{
			return false;
		}
		for (int i = 0; i < leftToLoad.Count; i++)
		{
			for (int j = 0; j < leftToLoad[i].things.Count; j++)
			{
				if (leftToLoad[i].things[j] == thing)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Notify_ThingAdded(Thing t)
	{
		try
		{
			SubtractFromToLoadList(t, t.stackCount);
			if (parent.Spawned && Props.pawnLoadedSound != null && t is Pawn)
			{
				Props.pawnLoadedSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			}
			if (t is Pawn pawn && pawn.IsFormingCaravan())
			{
				pawn.GetLord().Notify_PawnLost(pawn, PawnLostCondition.ForcedByPlayerAction);
			}
			QuestUtility.SendQuestTargetSignals(parent.questTags, "ThingAdded", t.Named("SUBJECT"));
			massUsageDirty = true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in Notify_ThingAdded: " + ex);
		}
	}

	public void Notify_ThingRemoved(Thing t)
	{
		if (Props.pawnExitSound != null && t is Pawn)
		{
			Props.pawnExitSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
		}
		massUsageDirty = true;
	}

	public void Notify_ThingAddedAndMergedWith(Thing t, int mergedCount)
	{
		SubtractFromToLoadList(t, mergedCount);
		massUsageDirty = true;
	}

	public bool CancelLoad()
	{
		if (Shuttle?.shipParent != null)
		{
			Shuttle.shipParent.ForceJob_DelayCurrent(ShipJobMaker.MakeShipJob(ShipJobDefOf.Unload));
		}
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
		if (list == null)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].CleanUpLoadingVars(map);
		}
		CleanUpLoadingVars(map);
		return true;
	}

	public void TryRemoveLord(Map map)
	{
		if (!LoadingInProgressOrReadyToLaunch)
		{
			return;
		}
		Lord lord = TransporterUtility.FindLord(groupID, map);
		if (lord == null)
		{
			return;
		}
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			if (ownedPawn.IsColonist && map.IsPlayerHome)
			{
				ownedPawn.inventory.UnloadEverything = true;
			}
		}
		map.lordManager.RemoveLord(lord);
	}

	public void CleanUpLoadingVars(Map map)
	{
		groupID = -1;
		innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
		leftToLoad?.Clear();
		Shuttle?.CleanUpLoadingVars();
		notifiedCantLoadMore = false;
		massUsageDirty = true;
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
				if (comp != null)
				{
					Messages.Message("MessageFinishedLoadingShuttle".Translate(parent.Named("SHUTTLE")), parent, MessageTypeDefOf.TaskCompletion);
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
		if (list != null)
		{
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
		}
	}

	private void SelectAllInGroup()
	{
		List<CompTransporter> list = TransportersInGroup(Map);
		if (list != null)
		{
			Selector selector = Find.Selector;
			selector.ClearSelection();
			for (int i = 0; i < list.Count; i++)
			{
				selector.Select(list[i].parent);
			}
		}
	}

	private void SelectNextInGroup()
	{
		List<CompTransporter> list = TransportersInGroup(Map);
		if (list != null)
		{
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
		}
	}
}
