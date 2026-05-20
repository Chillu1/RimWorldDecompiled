using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_WaitForDurationThenExit : LordJob
	{
		public IntVec3 point;

		public int durationTicks;

		public LordJob_WaitForDurationThenExit()
		{
		}

		public LordJob_WaitForDurationThenExit(IntVec3 point, int durationTicks)
		{
			this.point = point;
			this.durationTicks = durationTicks;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_WanderClose lordToil_WanderClose = new LordToil_WanderClose(point);
			stateGraph.AddToil(lordToil_WanderClose);
			stateGraph.StartingToil = lordToil_WanderClose;
			LordToil_ExitMapRandom lordToil_ExitMapRandom = new LordToil_ExitMapRandom();
			stateGraph.AddToil(lordToil_ExitMapRandom);
			Transition transition = new Transition(lordToil_WanderClose, lordToil_ExitMapRandom);
			transition.AddTrigger(new Trigger_TicksPassed(durationTicks));
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref point, "point");
			Scribe_Values.Look(ref durationTicks, "durationTicks", 0);
		}
	}
}
