namespace Verse.AI.Group
{
	public class LordJob_ExitMapNear : LordJob
	{
		private IntVec3 near;

		private float radius;

		private LocomotionUrgency locomotion = LocomotionUrgency.Jog;

		private bool canDig;

		private bool useAvoidGridSmart;

		public const float DefaultRadius = 12f;

		public LordJob_ExitMapNear()
		{
		}

		public LordJob_ExitMapNear(IntVec3 near, LocomotionUrgency locomotion, float radius = 12f, bool canDig = false, bool useAvoidGridSmart = false)
		{
			this.near = near;
			this.locomotion = locomotion;
			this.radius = radius;
			this.canDig = canDig;
			this.useAvoidGridSmart = useAvoidGridSmart;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_ExitMapNear lordToil_ExitMapNear = new LordToil_ExitMapNear(near, radius, locomotion, canDig);
			if (useAvoidGridSmart)
			{
				lordToil_ExitMapNear.useAvoidGrid = true;
			}
			stateGraph.AddToil(lordToil_ExitMapNear);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref near, "near");
			Scribe_Values.Look(ref radius, "radius", 0f);
			Scribe_Values.Look(ref locomotion, "locomotion", LocomotionUrgency.Jog);
			Scribe_Values.Look(ref canDig, "canDig", defaultValue: false);
			Scribe_Values.Look(ref useAvoidGridSmart, "useAvoidGridSmart", defaultValue: false);
		}
	}
}
