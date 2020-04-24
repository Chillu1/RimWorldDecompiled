using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Steal : LordJob
	{
		public override bool GuiltyOnDowned => true;

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_StealCover lordToil_StealCover = new LordToil_StealCover
			{
				useAvoidGrid = true
			};
			stateGraph.AddToil(lordToil_StealCover);
			LordToil_StealCover lordToil_StealCover2 = new LordToil_StealCover
			{
				cover = false,
				useAvoidGrid = true
			};
			stateGraph.AddToil(lordToil_StealCover2);
			Transition transition = new Transition(lordToil_StealCover, lordToil_StealCover2);
			transition.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(1200));
			stateGraph.AddTransition(transition);
			return stateGraph;
		}
	}
}
