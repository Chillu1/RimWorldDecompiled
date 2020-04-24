namespace Verse.AI.Group
{
	public class LordJob_DefendPoint : LordJob
	{
		private IntVec3 point;

		public LordJob_DefendPoint()
		{
		}

		public LordJob_DefendPoint(IntVec3 point)
		{
			this.point = point;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			stateGraph.AddToil(new LordToil_DefendPoint(point));
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref point, "point");
		}
	}
}
