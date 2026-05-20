using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_Joinable_MarriageCeremony : LordJob_VoluntarilyJoinable
{
	public Pawn firstPawn;

	public Pawn secondPawn;

	private IntVec3 spot;

	private int firstPawnSpouseCount;

	private int secondPawnSpouseCount;

	private Trigger_TicksPassed afterPartyTimeoutTrigger;

	public override bool LostImportantReferenceDuringLoading
	{
		get
		{
			if (firstPawn != null)
			{
				return secondPawn == null;
			}
			return true;
		}
	}

	public override bool AllowStartNewGatherings => false;

	public LordJob_Joinable_MarriageCeremony()
	{
	}

	public LordJob_Joinable_MarriageCeremony(Pawn firstPawn, Pawn secondPawn, IntVec3 spot)
	{
		this.firstPawn = firstPawn;
		this.secondPawn = secondPawn;
		this.spot = spot;
		firstPawnSpouseCount = firstPawn.GetSpouseCount(includeDead: false);
		secondPawnSpouseCount = firstPawn.GetSpouseCount(includeDead: false);
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_Party lordToil_Party = new LordToil_Party(spot, GatheringDefOf.Party);
		stateGraph.AddToil(lordToil_Party);
		LordToil_MarriageCeremony lordToil_MarriageCeremony = new LordToil_MarriageCeremony(firstPawn, secondPawn, spot);
		stateGraph.AddToil(lordToil_MarriageCeremony);
		LordToil_Party lordToil_Party2 = new LordToil_Party(spot, GatheringDefOf.Party);
		stateGraph.AddToil(lordToil_Party2);
		LordToil_End lordToil_End = new LordToil_End();
		stateGraph.AddToil(lordToil_End);
		Transition transition = new Transition(lordToil_Party, lordToil_MarriageCeremony);
		transition.AddTrigger(new Trigger_TickCondition(() => lord.ticksInToil >= 5000 && AreFiancesInPartyArea()));
		transition.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyStarts".Translate(firstPawn.LabelShort, secondPawn.LabelShort, firstPawn.Named("PAWN1"), secondPawn.Named("PAWN2")), MessageTypeDefOf.PositiveEvent, firstPawn));
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(lordToil_MarriageCeremony, lordToil_Party2);
		transition2.AddTrigger(new Trigger_TickCondition(() => firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, secondPawn)));
		transition2.AddPreAction(new TransitionAction_Message("MessageNewlyMarried".Translate(firstPawn.LabelShort, secondPawn.LabelShort, firstPawn.Named("PAWN1"), secondPawn.Named("PAWN2")), MessageTypeDefOf.PositiveEvent, new TargetInfo(spot, base.Map)));
		transition2.AddPreAction(new TransitionAction_Custom(WeddingSucceeded));
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(lordToil_Party2, lordToil_End);
		transition3.AddTrigger(new Trigger_TickCondition(ShouldAfterPartyBeCalledOff));
		transition3.AddTrigger(new Trigger_PawnKilled());
		stateGraph.AddTransition(transition3);
		afterPartyTimeoutTrigger = new Trigger_TicksPassed(7500);
		Transition transition4 = new Transition(lordToil_Party2, lordToil_End);
		transition4.AddTrigger(afterPartyTimeoutTrigger);
		transition4.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyAfterPartyFinished".Translate(firstPawn.LabelShort, secondPawn.LabelShort, firstPawn.Named("PAWN1"), secondPawn.Named("PAWN2")), MessageTypeDefOf.PositiveEvent, firstPawn));
		stateGraph.AddTransition(transition4);
		Transition transition5 = new Transition(lordToil_MarriageCeremony, lordToil_End);
		transition5.AddSource(lordToil_Party);
		transition5.AddTrigger(new Trigger_TickCondition(() => lord.ticksInToil >= 120000 && (firstPawn.Drafted || secondPawn.Drafted || !firstPawn.Position.InHorDistOf(spot, 4f) || !secondPawn.Position.InHorDistOf(spot, 4f))));
		transition5.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyCalledOff".Translate(firstPawn.LabelShort, secondPawn.LabelShort, firstPawn.Named("PAWN1"), secondPawn.Named("PAWN2")), MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, base.Map)));
		stateGraph.AddTransition(transition5);
		Transition transition6 = new Transition(lordToil_MarriageCeremony, lordToil_End);
		transition6.AddSource(lordToil_Party);
		transition6.AddTrigger(new Trigger_TickCondition(ShouldCeremonyBeCalledOff));
		transition6.AddTrigger(new Trigger_PawnKilled());
		transition6.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyCalledOff".Translate(firstPawn.LabelShort, secondPawn.LabelShort, firstPawn.Named("PAWN1"), secondPawn.Named("PAWN2")), MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, base.Map)));
		stateGraph.AddTransition(transition6);
		return stateGraph;
	}

	private bool AreFiancesInPartyArea()
	{
		if (!lord.ownedPawns.Contains(firstPawn) || !lord.ownedPawns.Contains(secondPawn))
		{
			return false;
		}
		if (firstPawn.Map != base.Map || !GatheringsUtility.InGatheringArea(firstPawn.Position, spot, base.Map))
		{
			return false;
		}
		if (secondPawn.Map != base.Map || !GatheringsUtility.InGatheringArea(secondPawn.Position, spot, base.Map))
		{
			return false;
		}
		return true;
	}

	private bool ShouldCeremonyBeCalledOff()
	{
		if (firstPawn.Destroyed || secondPawn.Destroyed)
		{
			return true;
		}
		if (!firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, secondPawn))
		{
			return true;
		}
		if (spot.GetDangerFor(firstPawn, base.Map) != Danger.None || spot.GetDangerFor(secondPawn, base.Map) != Danger.None)
		{
			return true;
		}
		if (!GatheringsUtility.AcceptableGameConditionsToContinueGathering(base.Map) || !MarriageCeremonyUtility.FianceCanContinueCeremony(firstPawn, secondPawn) || !MarriageCeremonyUtility.FianceCanContinueCeremony(secondPawn, firstPawn))
		{
			return true;
		}
		return false;
	}

	private bool ShouldAfterPartyBeCalledOff()
	{
		if (firstPawn.Destroyed || secondPawn.Destroyed)
		{
			return true;
		}
		if (firstPawn.Downed || secondPawn.Downed)
		{
			return true;
		}
		if (spot.GetDangerFor(firstPawn, base.Map) != Danger.None || spot.GetDangerFor(secondPawn, base.Map) != Danger.None)
		{
			return true;
		}
		if (!GatheringsUtility.AcceptableGameConditionsToContinueGathering(base.Map))
		{
			return true;
		}
		return false;
	}

	public override float VoluntaryJoinPriorityFor(Pawn p)
	{
		if (IsFiance(p))
		{
			if (!MarriageCeremonyUtility.FianceCanContinueCeremony(p, (p == firstPawn) ? secondPawn : firstPawn))
			{
				return 0f;
			}
			return VoluntarilyJoinableLordJobJoinPriorities.MarriageCeremonyFiance;
		}
		if (IsGuest(p))
		{
			if (!MarriageCeremonyUtility.ShouldGuestKeepAttendingCeremony(p))
			{
				return 0f;
			}
			if (!lord.ownedPawns.Contains(p))
			{
				if (IsCeremonyAboutToEnd())
				{
					return 0f;
				}
				if (lord.CurLordToil is LordToil_MarriageCeremony lordToil_MarriageCeremony && !SpectatorCellFinder.TryFindSpectatorCellFor(p, lordToil_MarriageCeremony.Data.spectateRect, base.Map, out var _, lordToil_MarriageCeremony.Data.spectateRectAllowedSides))
				{
					return 0f;
				}
			}
			return VoluntarilyJoinableLordJobJoinPriorities.MarriageCeremonyGuest;
		}
		return 0f;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref firstPawn, "firstPawn");
		Scribe_References.Look(ref secondPawn, "secondPawn");
		Scribe_Values.Look(ref firstPawnSpouseCount, "firstPawnSpouseCount", 0);
		Scribe_Values.Look(ref secondPawnSpouseCount, "secondPawnSpouseCount", 0);
		Scribe_Values.Look(ref spot, "spot");
	}

	public override string GetReport(Pawn pawn)
	{
		return "LordReportAttendingMarriageCeremony".Translate();
	}

	private bool IsCeremonyAboutToEnd()
	{
		if (afterPartyTimeoutTrigger.TicksLeft < 1200)
		{
			return true;
		}
		return false;
	}

	private bool IsFiance(Pawn p)
	{
		if (p != firstPawn)
		{
			return p == secondPawn;
		}
		return true;
	}

	private bool IsGuest(Pawn p)
	{
		if (!p.RaceProps.Humanlike)
		{
			return false;
		}
		if (p == firstPawn || p == secondPawn)
		{
			return false;
		}
		HistoryEvent ev = new HistoryEvent(SpouseRelationUtility.GetHistoryEventForSpouseCount(firstPawnSpouseCount + 1), p.Named(HistoryEventArgsNames.Doer));
		HistoryEvent ev2 = new HistoryEvent(SpouseRelationUtility.GetHistoryEventForSpouseCount(secondPawnSpouseCount + 1), p.Named(HistoryEventArgsNames.Doer));
		if (!ev.DoerWillingToDo() || !ev2.DoerWillingToDo())
		{
			return false;
		}
		if (p.Faction != firstPawn.Faction)
		{
			return p.Faction == secondPawn.Faction;
		}
		return true;
	}

	private void WeddingSucceeded()
	{
		List<Pawn> ownedPawns = lord.ownedPawns;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			if (ownedPawns[i] != firstPawn && ownedPawns[i] != secondPawn && ownedPawns[i].needs.mood != null && (firstPawn.Position.InHorDistOf(ownedPawns[i].Position, 18f) || secondPawn.Position.InHorDistOf(ownedPawns[i].Position, 18f)))
			{
				ownedPawns[i].needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AttendedWedding);
			}
		}
	}
}
