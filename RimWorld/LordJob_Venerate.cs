using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Venerate : LordJob
	{
		private Thing target;

		private int venerateDurationTicks;

		private string outSignalVenerationCompleted;

		private string forceExitSignal;

		public LordJob_Venerate()
		{
		}

		public LordJob_Venerate(Thing target, int venerateDurationTicks, string outSignalVenerationCompleted = null, string forceExitSignal = null)
		{
			this.target = target;
			this.venerateDurationTicks = venerateDurationTicks;
			this.outSignalVenerationCompleted = outSignalVenerationCompleted;
			this.forceExitSignal = forceExitSignal;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = new LordToil_Travel(target.InteractionCell);
			stateGraph.AddToil(lordToil_Travel);
			stateGraph.StartingToil = lordToil_Travel;
			LordToil_Venerate lordToil_Venerate = new LordToil_Venerate(target);
			stateGraph.AddToil(lordToil_Venerate);
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
			stateGraph.AddToil(lordToil_ExitMap);
			LordToil_ExitMapAndDefendSelf toil = new LordToil_ExitMapAndDefendSelf();
			stateGraph.AddToil(toil);
			Transition transition = new Transition(lordToil_Travel, lordToil_Venerate);
			transition.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_Venerate, lordToil_ExitMap);
			transition2.AddTrigger(new Trigger_TicksPassed(venerateDurationTicks));
			if (!outSignalVenerationCompleted.NullOrEmpty())
			{
				transition2.AddPostAction(new TransitionAction_Custom((Action)delegate
				{
					Find.SignalManager.SendSignal(new Signal(outSignalVenerationCompleted));
				}));
			}
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_Venerate, toil);
			transition3.AddSource(lordToil_Travel);
			transition3.AddSource(lordToil_ExitMap);
			transition3.AddTrigger(new Trigger_BecamePlayerEnemy());
			transition3.AddTrigger(new Trigger_PawnKilled());
			transition3.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_Travel, lordToil_ExitMap);
			transition4.AddSource(lordToil_Venerate);
			transition4.AddTrigger(new Trigger_AnyThingDamageTaken(new List<Thing> { target }, 1f));
			if (forceExitSignal != null)
			{
				transition4.AddTrigger(new Trigger_Signal(forceExitSignal));
			}
			transition4.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition4);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref target, "target");
			Scribe_Values.Look(ref venerateDurationTicks, "venerateDurationTicks", 0);
			Scribe_Values.Look(ref outSignalVenerationCompleted, "outSignalVenerationCompleted");
			Scribe_Values.Look(ref forceExitSignal, "forceExitSignal");
		}
	}
}
