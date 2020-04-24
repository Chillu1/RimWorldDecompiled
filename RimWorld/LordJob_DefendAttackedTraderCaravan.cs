using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_DefendAttackedTraderCaravan : LordJob
	{
		private IntVec3 defendSpot;

		public LordJob_DefendAttackedTraderCaravan()
		{
		}

		public LordJob_DefendAttackedTraderCaravan(IntVec3 defendSpot)
		{
			this.defendSpot = defendSpot;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_DefendTraderCaravan firstSource = (LordToil_DefendTraderCaravan)(stateGraph.StartingToil = new LordToil_DefendTraderCaravan(defendSpot));
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
			stateGraph.AddToil(lordToil_ExitMap);
			Transition transition = new Transition(firstSource, lordToil_ExitMap);
			transition.AddTrigger(new Trigger_BecameNonHostileToPlayer());
			transition.AddTrigger(new Trigger_TraderAndAllTraderCaravanGuardsLost());
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref defendSpot, "defendSpot");
		}
	}
}
