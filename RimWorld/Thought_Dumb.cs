using Verse;

namespace RimWorld
{
	public class Thought_Dumb : Thought
	{
		private int forcedStage;

		public override int CurStageIndex => forcedStage;

		public void SetForcedStage(int stageIndex)
		{
			forcedStage = stageIndex;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forcedStage, "stageIndex", 0);
		}
	}
}
