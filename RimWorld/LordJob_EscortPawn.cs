using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_EscortPawn : LordJob
	{
		public Pawn escortee;

		public Thing shuttle;

		private Faction escorteeFaction;

		public override bool AlwaysShowWeapon => true;

		public LordJob_EscortPawn()
		{
		}

		public LordJob_EscortPawn(Pawn escortee, Thing shuttle = null)
		{
			this.escortee = escortee;
			this.shuttle = shuttle;
			escorteeFaction = escortee.Faction;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_EscortPawn lordToil_EscortPawn = new LordToil_EscortPawn(escortee);
			stateGraph.AddToil(lordToil_EscortPawn);
			LordToil lordToil = ((shuttle == null) ? ((LordToil)new LordToil_ExitMap()) : ((LordToil)new LordToil_EnterShuttleOrLeave(shuttle)));
			stateGraph.AddToil(lordToil);
			Transition transition = new Transition(lordToil_EscortPawn, lordToil);
			Trigger_Custom trigger = new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && escortee.Dead);
			transition.AddTrigger(trigger);
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_EscortPawn, lordToil);
			Trigger_Custom trigger2 = new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && (escortee.MapHeld != lord.Map || (shuttle != null && escortee.ParentHolder == shuttle.TryGetComp<CompTransporter>() && !shuttle.TryGetComp<CompShuttle>().dropEverythingOnArrival)));
			transition2.AddTrigger(trigger2);
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_EscortPawn, lordToil);
			transition3.AddTrigger(new Trigger_BecamePlayerEnemy());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_EscortPawn, lordToil);
			transition4.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && escortee.Faction != escorteeFaction));
			stateGraph.AddTransition(transition4);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref escortee, "escortee");
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_References.Look(ref escorteeFaction, "escorteeFaction");
		}
	}
}
