using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_WaitForEscort : LordJob
	{
		public IntVec3 point;

		private bool addFleeToil = true;

		public override bool AddFleeToil => addFleeToil;

		public LordJob_WaitForEscort()
		{
		}

		public LordJob_WaitForEscort(IntVec3 point, bool addFleeToil = true)
		{
			this.point = point;
			this.addFleeToil = addFleeToil;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shuttle crash rescue is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 3454535);
				return stateGraph;
			}
			LordToil_WanderClose lordToil_WanderClose = new LordToil_WanderClose(point);
			stateGraph.AddToil(lordToil_WanderClose);
			stateGraph.StartingToil = lordToil_WanderClose;
			return stateGraph;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref point, "point");
			Scribe_Values.Look(ref addFleeToil, "addFleeToil", defaultValue: false);
		}
	}
}
