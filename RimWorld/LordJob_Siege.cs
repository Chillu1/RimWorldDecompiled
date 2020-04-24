using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Siege : LordJob
	{
		private Faction faction;

		private IntVec3 siegeSpot;

		private float blueprintPoints;

		public override bool GuiltyOnDowned => true;

		public LordJob_Siege()
		{
		}

		public LordJob_Siege(Faction faction, IntVec3 siegeSpot, float blueprintPoints)
		{
			this.faction = faction;
			this.siegeSpot = siegeSpot;
			this.blueprintPoints = blueprintPoints;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(siegeSpot).CreateGraph()).StartingToil;
			LordToil_Siege lordToil_Siege = new LordToil_Siege(siegeSpot, blueprintPoints);
			stateGraph.AddToil(lordToil_Siege);
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: false, interruptCurrentJob: true)
			{
				useAvoidGrid = true
			};
			stateGraph.AddToil(lordToil_ExitMap);
			LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
			Transition transition = new Transition(startingToil, lordToil_Siege);
			transition.AddTrigger(new Trigger_Memo("TravelArrived"));
			transition.AddTrigger(new Trigger_TicksPassed(5000));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_Siege, startingToil2);
			transition2.AddTrigger(new Trigger_Memo("NoBuilders"));
			transition2.AddTrigger(new Trigger_Memo("NoArtillery"));
			transition2.AddTrigger(new Trigger_PawnHarmed(0.08f));
			transition2.AddTrigger(new Trigger_FractionPawnsLost(0.3f));
			transition2.AddTrigger(new Trigger_TicksPassed((int)(60000f * Rand.Range(1.5f, 3f))));
			transition2.AddPreAction(new TransitionAction_Message("MessageSiegersAssaulting".Translate(faction.def.pawnsPlural, faction), MessageTypeDefOf.ThreatBig));
			transition2.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_Siege, lordToil_ExitMap);
			transition3.AddSource(startingToil2);
			transition3.AddSource(startingToil);
			transition3.AddTrigger(new Trigger_BecameNonHostileToPlayer());
			transition3.AddPreAction(new TransitionAction_Message("MessageRaidersLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			stateGraph.AddTransition(transition3);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref siegeSpot, "siegeSpot");
			Scribe_Values.Look(ref blueprintPoints, "blueprintPoints", 0f);
		}
	}
}
