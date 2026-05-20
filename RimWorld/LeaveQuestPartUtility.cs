using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class LeaveQuestPartUtility
{
	public static void MakePawnLeave(Pawn pawn, Quest quest)
	{
		Caravan caravan = pawn.GetCaravan();
		if (caravan != null)
		{
			CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading);
			caravan.RemovePawn(pawn);
		}
		if (pawn.Faction == Faction.OfPlayer)
		{
			Rand.PushState(quest.id ^ 0x394042B4);
			Faction result;
			if (pawn.HasExtraHomeFaction(quest) && pawn.GetExtraHomeFaction(quest) != Faction.OfPlayer)
			{
				result = pawn.GetExtraHomeFaction(quest);
			}
			else if (pawn.HasExtraMiniFaction(quest) && pawn.GetExtraMiniFaction(quest) != Faction.OfPlayer)
			{
				result = pawn.GetExtraMiniFaction(quest);
			}
			else if (pawn.IsSlave && pawn.SlaveFaction != Faction.OfPlayer)
			{
				result = pawn.SlaveFaction;
			}
			else if (!(from x in Find.FactionManager.GetFactions(allowHidden: false, allowDefeated: false, allowNonHumanlike: false)
				where !x.HostileTo(Faction.OfPlayer)
				select x).TryRandomElement(out result) && !Find.FactionManager.GetFactions(allowHidden: false, allowDefeated: false, allowNonHumanlike: false).TryRandomElement(out result))
			{
				result = null;
			}
			Rand.PopState();
			if (pawn.Faction != result)
			{
				pawn.SetFaction(result);
			}
		}
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
		{
			if (item.playerSettings.Master == pawn)
			{
				item.playerSettings.Master = null;
			}
		}
		if (pawn.guest != null)
		{
			if (pawn.InBed() && pawn.CurrentBed().Faction == Faction.OfPlayer && (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer)))
			{
				pawn.guest.SetGuestStatus(Faction.OfPlayer);
			}
			else
			{
				pawn.guest.SetGuestStatus(null);
			}
		}
		if (pawn.carryTracker?.CarriedThing != null)
		{
			pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
		}
		pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedByQuest);
	}

	public static void MakePawnsLeave(IEnumerable<Pawn> pawns, bool sendLetter, Quest quest, bool wakeUp = false)
	{
		bool flag = pawns.Any((Pawn x) => x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer);
		List<Pawn> list = pawns.Where((Pawn x) => x.Spawned || x.IsCaravanMember()).ToList();
		if (sendLetter && list.Any())
		{
			Pawn singlePawn;
			string text = GenLabel.BestGroupLabel(list, definite: false, out singlePawn);
			string text2 = GenLabel.BestGroupLabel(list, definite: true, out singlePawn);
			if (flag)
			{
				if (singlePawn != null)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelPawnLeaving".Translate(text), "LetterPawnLeaving".Translate(text2), LetterDefOf.NeutralEvent, singlePawn, null, quest);
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterLabelPawnsLeaving".Translate(text), "LetterPawnsLeaving".Translate(text2), LetterDefOf.NeutralEvent, list[0], null, quest);
				}
			}
			else if (singlePawn != null)
			{
				Messages.Message("MessagePawnLeaving".Translate(text2), singlePawn, MessageTypeDefOf.NeutralEvent);
			}
			else
			{
				Messages.Message("MessagePawnsLeaving".Translate(text2), list[0], MessageTypeDefOf.NeutralEvent);
			}
		}
		foreach (Pawn pawn2 in pawns)
		{
			MakePawnLeave(pawn2, quest);
		}
		IEnumerable<Pawn> enumerable = pawns.Where((Pawn p) => p.Spawned && !p.Downed);
		if (enumerable.Any())
		{
			Pawn pawn = enumerable.First();
			LordJob_ExitMapBest lordJob = new LordJob_ExitMapBest(LocomotionUrgency.Walk, canDig: true, canDefendSelf: true);
			LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.MapHeld, enumerable);
		}
		if (!wakeUp)
		{
			return;
		}
		foreach (Pawn pawn3 in pawns)
		{
			if (!pawn3.Awake())
			{
				RestUtility.WakeUp(pawn3);
			}
		}
	}
}
