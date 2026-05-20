using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public static class CaravanFormingUtility
{
	private static readonly Texture2D RemoveFromCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/RemoveFromCaravan");

	private static readonly Texture2D AddToCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/AddToCaravan");

	private static readonly Texture2D ForceDepartCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/ForceCaravanToDepart");

	private static List<Pawn> tmpPawnsInCancelledCaravan = new List<Pawn>();

	private static List<Pawn> tmpRopers = new List<Pawn>();

	private static List<Pawn> tmpNeedRoping = new List<Pawn>();

	private static readonly List<Thing> tmpThings = new List<Thing>();

	private static List<ThingCount> tmpCaravanPawns = new List<ThingCount>();

	public static void FormAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, PlanetTile exitFromTile, PlanetTile directionTile, PlanetTile destinationTile)
	{
		CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, faction, exitFromTile, directionTile, destinationTile);
	}

	public static void StartFormingCaravan(List<Pawn> pawns, List<Pawn> downedPawns, Faction faction, List<TransferableOneWay> transferables, IntVec3 meetingPoint, IntVec3 exitSpot, PlanetTile startingTile, PlanetTile destinationTile)
	{
		if (!startingTile.Valid)
		{
			Log.Error("Can't start forming caravan because startingTile is invalid.");
			return;
		}
		if (!pawns.Any())
		{
			Log.Error("Can't start forming caravan with 0 pawns.");
			return;
		}
		if (pawns.Any((Pawn x) => x.Downed))
		{
			Log.Warning("Forming a caravan with a downed pawn. This shouldn't happen because we have to create a Lord.");
		}
		List<TransferableOneWay> list = transferables.ToList();
		list.RemoveAll((TransferableOneWay x) => x.CountToTransfer <= 0 || !x.HasAnyThing || x.AnyThing is Pawn);
		for (int num = 0; num < pawns.Count; num++)
		{
			pawns[num].GetLord()?.Notify_PawnLost(pawns[num], PawnLostCondition.ForcedToJoinOtherLord);
		}
		LordJob_FormAndSendCaravan lordJob = new LordJob_FormAndSendCaravan(list, downedPawns, meetingPoint, exitSpot, startingTile, destinationTile);
		LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, pawns[0].MapHeld, pawns);
		for (int num2 = 0; num2 < pawns.Count; num2++)
		{
			Pawn pawn = pawns[num2];
			if (pawn.Spawned)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	public static void StopFormingCaravan(Lord lord)
	{
		tmpPawnsInCancelledCaravan.Clear();
		tmpPawnsInCancelledCaravan.AddRange(lord.ownedPawns);
		bool isPlayerHome = lord.Map.IsPlayerHome;
		SetToUnloadEverything(lord);
		lord.lordManager.RemoveLord(lord);
		if (isPlayerHome)
		{
			LeadAnimalsToPen(tmpPawnsInCancelledCaravan);
		}
		tmpPawnsInCancelledCaravan.Clear();
	}

	public static void LeadAnimalsToPen(List<Pawn> pawns)
	{
		tmpRopers.Clear();
		tmpNeedRoping.Clear();
		foreach (Pawn pawn in pawns)
		{
			if (pawn.Spawned && !pawn.Downed && !pawn.Dead && !pawn.Drafted)
			{
				if (AnimalPenUtility.NeedsToBeManagedByRope(pawn))
				{
					tmpNeedRoping.Add(pawn);
				}
				else if (pawn.IsColonist)
				{
					tmpRopers.Add(pawn);
				}
			}
		}
		if (tmpNeedRoping.Any() && tmpRopers.Any())
		{
			foreach (Pawn ropee in tmpNeedRoping)
			{
				if (!ropee.roping.IsRoped)
				{
					tmpRopers.MinBy((Pawn p) => p.Position.DistanceToSquared(ropee.Position)).roping.RopePawn(ropee);
				}
			}
			StartReturnedLord(tmpRopers.Where((Pawn p) => p.roping.IsRopingOthers).Concat(tmpNeedRoping).ToList());
		}
		tmpRopers.Clear();
		tmpNeedRoping.Clear();
	}

	private static void StartReturnedLord(List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord);
		}
		LordJob_ReturnedCaravan lordJob = new LordJob_ReturnedCaravan(pawns[0].Position);
		LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, pawns[0].MapHeld, pawns);
		foreach (Pawn pawn2 in pawns)
		{
			if (pawn2.Spawned)
			{
				pawn2.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	public static void RemovePawnFromCaravan(Pawn pawn, Lord lord, bool removeFromDowned = true)
	{
		bool flag = false;
		pawn.inventory.DropAllPackingCaravanThings();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn2 = lord.ownedPawns[i];
			if (pawn2 != pawn && CaravanUtility.IsOwner(pawn2, Faction.OfPlayer))
			{
				flag = true;
				break;
			}
		}
		bool flag2 = true;
		TaggedString taggedString = "MessagePawnLostWhileFormingCaravan".Translate(pawn).CapitalizeFirst();
		if (!flag)
		{
			StopFormingCaravan(lord);
			taggedString += " " + "MessagePawnLostWhileFormingCaravan_AllLost".Translate();
		}
		else
		{
			pawn.inventory.UnloadEverything = true;
			if (lord.ownedPawns.Contains(pawn))
			{
				lord.Notify_PawnLost(pawn, PawnLostCondition.ForcedByPlayerAction);
				flag2 = false;
			}
			if (lord.LordJob is LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan && lordJob_FormAndSendCaravan.downedPawns.Contains(pawn))
			{
				if (!removeFromDowned)
				{
					flag2 = false;
				}
				else
				{
					lordJob_FormAndSendCaravan.downedPawns.Remove(pawn);
				}
			}
			if (pawn.jobs != null)
			{
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
		}
		if (flag2)
		{
			Messages.Message(taggedString, pawn, MessageTypeDefOf.NegativeEvent);
		}
	}

	public static void ForceCaravanDepart(Lord lord)
	{
		LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = (LordJob_FormAndSendCaravan)lord.LordJob;
		for (int num = lord.ownedPawns.Count - 1; num >= 0; num--)
		{
			Pawn pawn = lord.ownedPawns[num];
			if (pawn.InMentalState)
			{
				lord.RemovePawn(pawn);
				pawn.inventory.UnloadEverything = true;
			}
		}
		if (lord.ownedPawns.Count == 0)
		{
			lord.Map.lordManager.RemoveLord(lord);
			Messages.Message("MessageForceSendingCaravan_AllLost".Translate(), MessageTypeDefOf.NeutralEvent);
			return;
		}
		for (int num2 = lord.ownedPawns.Count - 1; num2 >= 0; num2--)
		{
			Pawn pawn2 = lord.ownedPawns[num2];
			if (pawn2.Drafted)
			{
				pawn2.drafter.Drafted = false;
			}
			else if (pawn2.roping.RopedToSpot.IsValid)
			{
				pawn2.roping.UnropeFromSpot();
			}
		}
		lordJob_FormAndSendCaravan.downedPawns?.Clear();
		lord.ReceiveMemo("ForceDepartNow");
	}

	private static void SetToUnloadEverything(Lord lord)
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			lord.ownedPawns[i].inventory.DropAllPackingCaravanThings();
			lord.ownedPawns[i].inventory.UnloadEverything = true;
		}
	}

	public static void TryAddItemBackToTransferables(Thing item, List<TransferableOneWay> transferables, int count)
	{
		TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(item, transferables, TransferAsOneMode.PodsOrCaravanPacking);
		if (transferableOneWay != null && !transferableOneWay.things.Contains(item))
		{
			transferableOneWay.things.Add(item);
		}
		transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + count));
	}

	public static List<Thing> AllReachableColonyItems(Map map, bool allowEvenIfOutsideHomeArea = false, bool allowEvenIfReserved = false, bool canMinify = false)
	{
		tmpThings.Clear();
		map.listerThings.GetAllThings(in tmpThings, IsValidCaravanItem, lookInHaulSources: true);
		return tmpThings;
		bool IsValidCaravanItem(Thing t)
		{
			bool flag = canMinify && t.def.Minifiable && t.def.plant == null;
			if (flag)
			{
				Faction faction = t.Faction;
				if (faction != null && !faction.IsPlayer)
				{
					return false;
				}
			}
			if (!t.Spawned && !HaulAIUtility.IsInHaulableInventory(t))
			{
				return false;
			}
			if ((flag || t.def.category == ThingCategory.Item) && (allowEvenIfOutsideHomeArea || map.areaManager.Home[t.PositionHeld] || t.IsInAnyStorage()) && !t.PositionHeld.Fogged(t.MapHeld) && (allowEvenIfReserved || !map.reservationManager.IsReservedByAnyoneOf(t, Faction.OfPlayer)) && (flag || t.def.EverHaulable))
			{
				return t.GetInnerIfMinified().def.canLoadIntoCaravan;
			}
			return false;
		}
	}

	public static List<Pawn> AllSendablePawns(Map map, bool allowEvenIfDowned = false, bool allowEvenIfInMentalState = false, bool allowEvenIfPrisonerNotSecure = false, bool allowCapturableDownedPawns = false, bool allowLodgers = false, int allowLoadAndEnterTransportersLordForGroupID = -1)
	{
		List<Pawn> list = new List<Pawn>();
		IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn = allPawnsSpawned[i];
			if ((allowEvenIfDowned || !pawn.Downed) && (allowEvenIfInMentalState || !pawn.InMentalState) && (pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony || (allowCapturableDownedPawns && CanListAsAutoCapturable(pawn))) && pawn.RaceProps.allowedOnCaravan && !pawn.IsQuestHelper() && (!pawn.IsQuestLodger() || allowLodgers) && (allowEvenIfPrisonerNotSecure || !pawn.IsPrisoner || pawn.guest.PrisonerIsSecure) && (pawn.GetLord() == null || pawn.GetLord().LordJob is LordJob_VoluntarilyJoinable || pawn.GetLord().LordJob.IsCaravanSendable || (allowLoadAndEnterTransportersLordForGroupID >= 0 && pawn.GetLord().LordJob is LordJob_LoadAndEnterTransporters && ((LordJob_LoadAndEnterTransporters)pawn.GetLord().LordJob).transportersGroup == allowLoadAndEnterTransportersLordForGroupID)))
			{
				list.Add(pawn);
			}
		}
		return list;
	}

	private static bool CanListAsAutoCapturable(Pawn p)
	{
		if (p.Downed && !p.mindState.WillJoinColonyIfRescued)
		{
			return CaravanUtility.ShouldAutoCapture(p, Faction.OfPlayer);
		}
		return false;
	}

	public static IEnumerable<Gizmo> GetGizmos(Pawn pawn)
	{
		if (IsFormingCaravanOrDownedPawnToBeTakenByCaravan(pawn))
		{
			Lord lord = GetFormAndSendCaravanLord(pawn);
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandCancelFormingCaravan".Translate();
			command_Action.defaultDesc = "CommandCancelFormingCaravanDesc".Translate();
			command_Action.icon = TexCommand.ClearPrioritizedWork;
			command_Action.activateSound = SoundDefOf.Tick_Low;
			command_Action.action = delegate
			{
				StopFormingCaravan(lord);
			};
			command_Action.hotKey = KeyBindingDefOf.Designator_Cancel;
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "CommandRemoveFromCaravan".Translate();
			command_Action2.defaultDesc = "CommandRemoveFromCaravanDesc".Translate();
			command_Action2.icon = RemoveFromCaravanCommand;
			command_Action2.action = delegate
			{
				RemovePawnFromCaravan(pawn, lord);
			};
			command_Action2.hotKey = KeyBindingDefOf.Misc6;
			yield return command_Action2;
			if (lord.CurLordToil is LordToil_PrepareCaravan_Leave)
			{
				yield break;
			}
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "CommandForceCaravanDepart".Translate();
			command_Action3.defaultDesc = "CommandForceCaravanDepartDesc".Translate();
			command_Action3.icon = ForceDepartCaravanCommand;
			command_Action3.action = delegate
			{
				string forceDepartWarningMessage = GetForceDepartWarningMessage(lord.LordJob as LordJob_FormAndSendCaravan);
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(forceDepartWarningMessage, delegate
				{
					ForceCaravanDepart(lord);
				}));
			};
			command_Action3.hotKey = KeyBindingDefOf.Misc8;
			yield return command_Action3;
		}
		else
		{
			if (!pawn.Spawned)
			{
				yield break;
			}
			bool flag = false;
			for (int num = 0; num < pawn.Map.lordManager.lords.Count; num++)
			{
				Lord lord2 = pawn.Map.lordManager.lords[num];
				if (lord2.faction == Faction.OfPlayer && lord2.LordJob is LordJob_FormAndSendCaravan)
				{
					flag = true;
					break;
				}
			}
			if (!flag || !Dialog_FormCaravan.AllSendablePawns(pawn.Map, reform: false).Contains(pawn))
			{
				yield break;
			}
			Command_Action command_Action4 = new Command_Action();
			command_Action4.defaultLabel = "CommandAddToCaravan".Translate();
			command_Action4.defaultDesc = "CommandAddToCaravanDesc".Translate();
			command_Action4.icon = AddToCaravanCommand;
			command_Action4.action = delegate
			{
				List<Lord> list = new List<Lord>();
				for (int i = 0; i < pawn.Map.lordManager.lords.Count; i++)
				{
					Lord lord3 = pawn.Map.lordManager.lords[i];
					if (lord3.faction == Faction.OfPlayer && lord3.LordJob is LordJob_FormAndSendCaravan)
					{
						list.Add(lord3);
					}
				}
				if (list.Count != 0)
				{
					if (list.Count == 1)
					{
						LateJoinFormingCaravan(pawn, list[0]);
						SoundDefOf.Click.PlayOneShotOnCamera();
					}
					else
					{
						List<FloatMenuOption> list2 = new List<FloatMenuOption>();
						for (int j = 0; j < list.Count; j++)
						{
							Lord caravanLocal = list[j];
							string label = string.Concat("Caravan".Translate() + " ", (j + 1).ToString());
							list2.Add(new FloatMenuOption(label, delegate
							{
								if (pawn.Spawned && pawn.Map.lordManager.lords.Contains(caravanLocal) && Dialog_FormCaravan.AllSendablePawns(pawn.Map, reform: false).Contains(pawn))
								{
									LateJoinFormingCaravan(pawn, caravanLocal);
								}
							}));
						}
						Find.WindowStack.Add(new FloatMenu(list2));
					}
				}
			};
			command_Action4.hotKey = KeyBindingDefOf.Misc7;
			yield return command_Action4;
		}
	}

	public static void LateJoinFormingCaravan(Pawn pawn, Lord lord)
	{
		pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord);
		if (pawn.Downed)
		{
			((LordJob_FormAndSendCaravan)lord.LordJob).downedPawns.Add(pawn);
		}
		else
		{
			lord.AddPawn(pawn);
		}
		if (pawn.Spawned)
		{
			pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public static bool IsFormingCaravan(this Pawn p)
	{
		Lord lord;
		return p.TryGetFormingCaravanLord(out lord);
	}

	public static bool TryGetFormingCaravanLord(this Pawn p, out Lord lord)
	{
		lord = p.GetLord();
		if (lord != null)
		{
			return lord.LordJob is LordJob_FormAndSendCaravan;
		}
		return false;
	}

	public static bool IsFormingCaravanOrDownedPawnToBeTakenByCaravan(Pawn p)
	{
		return GetFormAndSendCaravanLord(p) != null;
	}

	public static Lord GetFormAndSendCaravanLord(Pawn p)
	{
		if (p.IsFormingCaravan())
		{
			return p.GetLord();
		}
		if (p.SpawnedOrAnyParentSpawned)
		{
			List<Lord> lords = p.MapHeld.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				if (lords[i].LordJob is LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan && lordJob_FormAndSendCaravan.downedPawns.Contains(p))
				{
					return lords[i];
				}
			}
		}
		return null;
	}

	public static float CapacityLeft(LordJob_FormAndSendCaravan lordJob)
	{
		float num = CollectionsMassCalculator.MassUsageTransferables(lordJob.transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
		tmpCaravanPawns.Clear();
		for (int i = 0; i < lordJob.lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lordJob.lord.ownedPawns[i];
			tmpCaravanPawns.Add(new ThingCount(pawn, pawn.stackCount));
		}
		num += CollectionsMassCalculator.MassUsage(tmpCaravanPawns, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
		float num2 = CollectionsMassCalculator.Capacity(tmpCaravanPawns);
		tmpCaravanPawns.Clear();
		return num2 - num;
	}

	public static string AppendOverweightInfo(string text, float capacityLeft)
	{
		if (capacityLeft < 0f)
		{
			text += " (" + "OverweightLower".Translate() + ")";
		}
		return text;
	}

	public static bool AllItemsLoadedOntoCaravan(Lord lord, Map map)
	{
		bool flag = true;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (pawn.IsColonist && pawn.mindState.lastJobTag != JobTag.WaitingForOthersToFinishGatheringItems)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int j = 0; j < allPawnsSpawned.Count; j++)
			{
				if (allPawnsSpawned[j].CurJob != null && allPawnsSpawned[j].jobs.curDriver is JobDriver_PrepareCaravan_GatherItems && allPawnsSpawned[j].CurJob.lord == lord)
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}

	public static string GetForceDepartWarningMessage(LordJob_FormAndSendCaravan lordJob)
	{
		List<Transferable> list = new List<Transferable>();
		if (!AllItemsLoadedOntoCaravan(lordJob.lord, lordJob.lord.Map))
		{
			foreach (TransferableOneWay transferable in lordJob.transferables)
			{
				if (transferable.CountToTransfer > 0)
				{
					list.Add(transferable);
				}
			}
		}
		List<ThingCount> list2 = new List<ThingCount>();
		List<Pawn> list3 = new List<Pawn>();
		foreach (Pawn ownedPawn in lordJob.lord.ownedPawns)
		{
			if (ownedPawn.Downed)
			{
				list3.Add(ownedPawn);
			}
			else if (ownedPawn.InMentalState)
			{
				list3.Add(ownedPawn);
			}
			else
			{
				list2.Add(new ThingCount(ownedPawn, ownedPawn.stackCount));
			}
		}
		List<ThingCount> list4 = new List<ThingCount>();
		foreach (Pawn item in list3)
		{
			foreach (Thing item2 in item.inventory.innerContainer)
			{
				list4.Add(new ThingCount(item2, item2.stackCount));
			}
		}
		list4 = (from tc in list4
			group tc by tc.Thing.def into g
			select new ThingCount(g.First().Thing, g.Sum((ThingCount tc) => tc.Count), ignoreStackLimit: true)).ToList();
		TaggedString taggedString = "ConfirmForceDepartIntro".Translate();
		if (list.Count > 0)
		{
			taggedString += "\n\n" + "ConfirmForceDepartItemsStillLoading".Translate();
			taggedString += "\n\n" + list.Select((Transferable i) => i.LabelCap + " x" + i.CountToTransfer).ToLineList("- ");
		}
		if (list3.Count > 0)
		{
			taggedString += "\n\n" + "ConfirmForceDepartPawnsNotLeaving".Translate();
			taggedString += "\n\n" + list3.Select((Pawn p) => p.LabelNoCountColored.ToString()).ToLineList("- ");
		}
		if (list4.Count > 0)
		{
			taggedString += "\n\n" + "ConfirmForceDepartItemsOnPawnsNotLeaving".Translate();
			taggedString += "\n\n" + list4.Select((ThingCount i) => i.Thing.LabelCapNoCount + " x" + i.Count).ToLineList("- ");
		}
		return taggedString;
	}
}
