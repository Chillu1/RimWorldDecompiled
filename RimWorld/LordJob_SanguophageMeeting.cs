using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_SanguophageMeeting : LordJob
	{
		public IntVec3 targetCell;

		public List<Thing> enemyTargets;

		public int meetingDurationTicks;

		public string outSignalMeetingStarted;

		public string outSignalMeetingEnded;

		public string outSignalAllSanguophagesGone;

		public string inSignalDefend;

		public string inSignalQuestSuccess;

		public string inSignalQuestFail;

		private const float HeatstrokeHypothermiaMinSeverityForLeaving = 0.35f;

		public override bool AddFleeToil => false;

		public LordJob_SanguophageMeeting()
		{
		}

		public LordJob_SanguophageMeeting(IntVec3 targetCell, List<Thing> enemyTargets, int meetingDurationTicks, string outSignalMeetingStarted, string outSignalMeetingEnded, string outSignalAllSanguophagesGone, string inSignalDefend, string inSignalQuestSuccess, string inSignalQuestFail)
		{
			this.targetCell = targetCell;
			this.enemyTargets = enemyTargets;
			this.meetingDurationTicks = meetingDurationTicks;
			this.outSignalMeetingStarted = outSignalMeetingStarted;
			this.outSignalMeetingEnded = outSignalMeetingEnded;
			this.outSignalAllSanguophagesGone = outSignalAllSanguophagesGone;
			this.inSignalDefend = inSignalDefend;
			this.inSignalQuestSuccess = inSignalQuestSuccess;
			this.inSignalQuestFail = inSignalQuestFail;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			if (!ModLister.CheckBiotech("Sanguophage meeting"))
			{
				return stateGraph;
			}
			LordToil_Travel lordToil_Travel = new LordToil_Travel(targetCell);
			stateGraph.AddToil(lordToil_Travel);
			stateGraph.StartingToil = lordToil_Travel;
			LordToil_SanguophageMeeting lordToil_SanguophageMeeting = new LordToil_SanguophageMeeting(targetCell, meetingDurationTicks);
			stateGraph.AddToil(lordToil_SanguophageMeeting);
			LordToil_ExitMapRandom lordToil_ExitMapRandom = new LordToil_ExitMapRandom();
			stateGraph.AddToil(lordToil_ExitMapRandom);
			LordToil_ExitMapAndDefendSelf lordToil_ExitMapAndDefendSelf = new LordToil_ExitMapAndDefendSelf();
			stateGraph.AddToil(lordToil_ExitMapAndDefendSelf);
			LordToil_AssaultThings lordToil_AssaultThings = new LordToil_AssaultThings(enemyTargets);
			stateGraph.AddToil(lordToil_AssaultThings);
			Transition transition = new Transition(lordToil_Travel, lordToil_SanguophageMeeting);
			transition.AddTrigger(new Trigger_Memo("TravelArrived"));
			transition.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				if (!outSignalMeetingStarted.NullOrEmpty())
				{
					Find.SignalManager.SendSignal(new Signal(outSignalMeetingStarted));
				}
				Building building = (Building)ThingMaker.MakeThing(ThingDefOf.SanguphageMeetingTorch);
				building.SetFaction(lord.faction);
				if (GenPlace.TryPlaceThing(building, targetCell, base.Map, ThingPlaceMode.Near))
				{
					lord.AddBuilding(building);
				}
			}));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_SanguophageMeeting, lordToil_ExitMapRandom);
			transition2.AddTrigger(new Trigger_TicksPassed(meetingDurationTicks));
			transition2.AddTrigger(new Trigger_Signal(inSignalQuestSuccess));
			transition2.AddTrigger(new Trigger_Signal(inSignalQuestFail));
			transition2.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				Find.SignalManager.SendSignal(new Signal(outSignalMeetingEnded));
			}));
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_SanguophageMeeting, lordToil_ExitMapAndDefendSelf);
			transition3.AddSource(lordToil_Travel);
			transition3.AddSource(lordToil_ExitMapRandom);
			transition3.AddSource(lordToil_AssaultThings);
			transition3.AddTrigger(new Trigger_BecamePlayerEnemy());
			transition3.AddTrigger(new Trigger_PawnKilled());
			transition3.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_Travel, lordToil_ExitMapAndDefendSelf);
			transition4.AddSource(lordToil_SanguophageMeeting);
			transition4.AddTrigger(new Trigger_PawnHarmed(1f, requireInstigatorWithFaction: true, Faction.OfPlayer));
			transition4.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				QuestUtility.SendQuestTargetSignals(lord.questTags, "BeingAttacked", lord.Named("SUBJECT"));
			}));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_SanguophageMeeting, lordToil_AssaultThings);
			transition5.AddTrigger(new Trigger_Signal(inSignalDefend));
			transition5.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_AssaultThings, lordToil_ExitMapRandom);
			transition6.AddTrigger(new Trigger_Signal(inSignalQuestSuccess));
			stateGraph.AddTransition(transition6);
			Transition transition7 = new Transition(lordToil_SanguophageMeeting, lordToil_ExitMapAndDefendSelf);
			transition7.AddTrigger(new Trigger_Custom(delegate(TriggerSignal signal)
			{
				if (signal.type == TriggerSignalType.Tick)
				{
					for (int i = 0; i < lord.ownedPawns.Count; i++)
					{
						Pawn pawn = lord.ownedPawns[i];
						if (!pawn.Dead)
						{
							Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
							if (firstHediffOfDef != null && firstHediffOfDef.Severity >= 0.35f)
							{
								return true;
							}
							Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke);
							if (firstHediffOfDef2 != null && firstHediffOfDef2.Severity >= 0.35f)
							{
								return true;
							}
						}
					}
				}
				return false;
			}));
			transition7.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				Messages.Message("SanguophagesLeavingTemperature".Translate(), lord.ownedPawns, MessageTypeDefOf.NegativeEvent);
				QuestUtility.SendQuestTargetSignals(lord.questTags, "BeingAttacked", lord.Named("SUBJECT"));
			}));
			stateGraph.AddTransition(transition7);
			return stateGraph;
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			base.Notify_PawnLost(p, condition);
			if ((condition == PawnLostCondition.ExitedMap || condition == PawnLostCondition.Incapped || condition == PawnLostCondition.Killed || condition == PawnLostCondition.ChangedFaction || condition == PawnLostCondition.Vanished) && lord.ownedPawns.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalAllSanguophagesGone));
			}
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref targetCell, "targetCell");
			Scribe_Collections.Look(ref enemyTargets, "enemyTargets", LookMode.Reference);
			Scribe_Values.Look(ref meetingDurationTicks, "meetingDurationTicks", 0);
			Scribe_Values.Look(ref outSignalMeetingStarted, "outSignalMeetingStarted");
			Scribe_Values.Look(ref outSignalMeetingEnded, "outSignalMeetingEnded");
			Scribe_Values.Look(ref outSignalAllSanguophagesGone, "outSignalAllSanguophagesGone");
			Scribe_Values.Look(ref inSignalDefend, "inSignalDefend");
			Scribe_Values.Look(ref inSignalQuestSuccess, "inSignalQuestSuccess");
			Scribe_Values.Look(ref inSignalQuestFail, "inSignalQuestFail");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				enemyTargets.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
