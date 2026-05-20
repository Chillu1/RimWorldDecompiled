using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_ReturnedCaravan : LordJob
	{
		private IntVec3 entryPoint;

		public LordJob_ReturnedCaravan(IntVec3 entryPoint)
		{
			this.entryPoint = entryPoint;
		}

		public LordJob_ReturnedCaravan()
		{
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_ReturnedCaravan_PenAnimals lordToil_ReturnedCaravan_PenAnimals = new LordToil_ReturnedCaravan_PenAnimals(entryPoint);
			stateGraph.AddToil(lordToil_ReturnedCaravan_PenAnimals);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(lordToil_ReturnedCaravan_PenAnimals, lordToil_End);
			transition.AddTrigger(new Trigger_Memo("RepenningFinished"));
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref entryPoint, "entryPoint");
		}
	}
}
