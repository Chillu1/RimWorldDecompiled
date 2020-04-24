using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_TradeWithColony : LordJob
	{
		private Faction faction;

		private IntVec3 chillSpot;

		public override bool AddFleeToil => false;

		public LordJob_TradeWithColony()
		{
		}

		public LordJob_TradeWithColony(Faction faction, IntVec3 chillSpot)
		{
			this.faction = faction;
			this.chillSpot = chillSpot;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = (LordToil_Travel)(stateGraph.StartingToil = new LordToil_Travel(chillSpot));
			LordToil_DefendTraderCaravan lordToil_DefendTraderCaravan = new LordToil_DefendTraderCaravan();
			stateGraph.AddToil(lordToil_DefendTraderCaravan);
			LordToil_DefendTraderCaravan lordToil_DefendTraderCaravan2 = new LordToil_DefendTraderCaravan(chillSpot);
			stateGraph.AddToil(lordToil_DefendTraderCaravan2);
			LordToil_ExitMapAndEscortCarriers lordToil_ExitMapAndEscortCarriers = new LordToil_ExitMapAndEscortCarriers();
			stateGraph.AddToil(lordToil_ExitMapAndEscortCarriers);
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
			stateGraph.AddToil(lordToil_ExitMap);
			LordToil_ExitMap lordToil_ExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Walk, canDig: true);
			stateGraph.AddToil(lordToil_ExitMap2);
			LordToil_ExitMapTraderFighting lordToil_ExitMapTraderFighting = new LordToil_ExitMapTraderFighting();
			stateGraph.AddToil(lordToil_ExitMapTraderFighting);
			Transition transition = new Transition(lordToil_Travel, lordToil_ExitMapAndEscortCarriers);
			transition.AddSources(lordToil_DefendTraderCaravan, lordToil_DefendTraderCaravan2);
			transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_Travel, lordToil_ExitMap2);
			transition2.AddSources(lordToil_DefendTraderCaravan, lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers, lordToil_ExitMap, lordToil_ExitMapTraderFighting);
			transition2.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			transition2.AddPostAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			transition2.AddPostAction(new TransitionAction_WakeAll());
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_ExitMap2, lordToil_ExitMapTraderFighting);
			transition3.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition3.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_Travel, lordToil_ExitMapTraderFighting);
			transition4.AddSources(lordToil_DefendTraderCaravan, lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers, lordToil_ExitMap);
			transition4.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
			transition4.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_Travel, lordToil_DefendTraderCaravan);
			transition5.AddTrigger(new Trigger_PawnHarmed());
			transition5.AddPreAction(new TransitionAction_SetDefendTrader());
			transition5.AddPostAction(new TransitionAction_WakeAll());
			transition5.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_DefendTraderCaravan, lordToil_Travel);
			transition6.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
			stateGraph.AddTransition(transition6);
			Transition transition7 = new Transition(lordToil_Travel, lordToil_DefendTraderCaravan2);
			transition7.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition7);
			Transition transition8 = new Transition(lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers);
			transition8.AddTrigger(new Trigger_TicksPassed((!DebugSettings.instantVisitorsGift) ? Rand.Range(27000, 45000) : 0));
			transition8.AddPreAction(new TransitionAction_CheckGiveGift());
			transition8.AddPreAction(new TransitionAction_Message("MessageTraderCaravanLeaving".Translate(faction.Name)));
			transition8.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition8);
			Transition transition9 = new Transition(lordToil_ExitMapAndEscortCarriers, lordToil_ExitMapAndEscortCarriers, canMoveToSameState: true);
			transition9.canMoveToSameState = true;
			transition9.AddTrigger(new Trigger_PawnLost());
			transition9.AddTrigger(new Trigger_TickCondition(() => LordToil_ExitMapAndEscortCarriers.IsAnyDefendingPosition(lord.ownedPawns) && !GenHostility.AnyHostileActiveThreatTo(base.Map, faction, countDormantPawnsAsHostile: true), 60));
			stateGraph.AddTransition(transition9);
			Transition transition10 = new Transition(lordToil_ExitMapAndEscortCarriers, lordToil_ExitMap);
			transition10.AddTrigger(new Trigger_TicksPassed(60000));
			transition10.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition10);
			Transition transition11 = new Transition(lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers);
			transition11.AddSources(lordToil_Travel, lordToil_DefendTraderCaravan);
			transition11.AddTrigger(new Trigger_ImportantTraderCaravanPeopleLost());
			transition11.AddTrigger(new Trigger_BecamePlayerEnemy());
			transition11.AddPostAction(new TransitionAction_WakeAll());
			transition11.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition11);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref chillSpot, "chillSpot");
		}
	}
}
