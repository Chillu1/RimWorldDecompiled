using Verse;

namespace RimWorld
{
	public class RitualStagePawnSecondFocus : IExposable
	{
		public int stageIndex;

		public Pawn pawn;

		public TargetInfo target;

		public void ExposeData()
		{
			Scribe_Values.Look(ref stageIndex, "stageIndex", 0);
			Scribe_References.Look(ref pawn, "pawn");
			Scribe_TargetInfo.Look(ref target, "target");
		}
	}
}
