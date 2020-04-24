using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_LoadAndEnterTransporters : LordJob
	{
		public int transportersGroup = -1;

		public override bool AllowStartNewGatherings => false;

		public override bool AddFleeToil => false;

		public LordJob_LoadAndEnterTransporters()
		{
		}

		public LordJob_LoadAndEnterTransporters(int transportersGroup)
		{
			this.transportersGroup = transportersGroup;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref transportersGroup, "transportersGroup", 0);
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_LoadAndEnterTransporters lordToil_LoadAndEnterTransporters = (LordToil_LoadAndEnterTransporters)(stateGraph.StartingToil = new LordToil_LoadAndEnterTransporters(transportersGroup));
			LordToil_End toil = new LordToil_End();
			stateGraph.AddToil(toil);
			return stateGraph;
		}
	}
}
