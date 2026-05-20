using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_Joinable_Party : LordJob_Joinable_Gathering
{
	public override bool AllowStartNewGatherings => false;

	protected virtual ThoughtDef AttendeeThought => ThoughtDefOf.AttendedParty;

	protected virtual TaleDef AttendeeTale => TaleDefOf.AttendedParty;

	protected virtual ThoughtDef OrganizerThought => ThoughtDefOf.AttendedParty;

	protected virtual TaleDef OrganizerTale => TaleDefOf.AttendedParty;

	public LordJob_Joinable_Party()
	{
	}

	public LordJob_Joinable_Party(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
		: base(spot, organizer, gatheringDef)
	{
		durationTicks = Rand.RangeInclusive(5000, 15000);
	}

	public override string GetReport(Pawn pawn)
	{
		return "LordReportAttendingParty".Translate();
	}

	protected override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
	{
		return new LordToil_Party(spot, gatheringDef);
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil party = CreateGatheringToil(spot, organizer, gatheringDef);
		stateGraph.AddToil(party);
		LordToil_End lordToil_End = new LordToil_End();
		stateGraph.AddToil(lordToil_End);
		Transition transition = new Transition(party, lordToil_End);
		transition.AddTrigger(new Trigger_TickCondition(ShouldBeCalledOff));
		transition.AddTrigger(new Trigger_PawnKilled());
		transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.LeftVoluntarily, organizer));
		transition.AddPreAction(new TransitionAction_Custom((Action)delegate
		{
			ApplyOutcome((LordToil_Party)party);
		}));
		transition.AddPreAction(new TransitionAction_Message(gatheringDef.calledOffMessage, MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, base.Map)));
		stateGraph.AddTransition(transition);
		timeoutTrigger = GetTimeoutTrigger();
		Transition transition2 = new Transition(party, lordToil_End);
		transition2.AddTrigger(timeoutTrigger);
		transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
		{
			ApplyOutcome((LordToil_Party)party);
		}));
		transition2.AddPreAction(new TransitionAction_Message(gatheringDef.finishedMessage, MessageTypeDefOf.SituationResolved, new TargetInfo(spot, base.Map)));
		stateGraph.AddTransition(transition2);
		return stateGraph;
	}

	private void ApplyOutcome(LordToil_Party toil)
	{
		List<Pawn> ownedPawns = lord.ownedPawns;
		LordToilData_Gathering lordToilData_Gathering = (LordToilData_Gathering)toil.data;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			Pawn pawn = ownedPawns[i];
			bool flag = pawn == organizer;
			if (lordToilData_Gathering.presentForTicks.TryGetValue(pawn, out var value) && value > 0)
			{
				if (ownedPawns[i].needs.mood != null)
				{
					ThoughtDef thoughtDef = (flag ? OrganizerThought : AttendeeThought);
					float num = 0.5f / thoughtDef.stages[0].baseMoodEffect;
					float moodPowerFactor = Mathf.Min((float)value / (float)durationTicks + num, 1f);
					Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
					thought_Memory.moodPowerFactor = moodPowerFactor;
					ownedPawns[i].needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
				}
				TaleRecorder.RecordTale(flag ? OrganizerTale : AttendeeTale, ownedPawns[i], organizer);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit && gatheringDef == null)
		{
			gatheringDef = GatheringDefOf.Party;
		}
	}
}
