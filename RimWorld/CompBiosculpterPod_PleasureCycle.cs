using Verse;

namespace RimWorld
{
	public class CompBiosculpterPod_PleasureCycle : CompBiosculpterPod_Cycle
	{
		public override void CycleCompleted(Pawn pawn)
		{
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BiosculpterPleasure);
			Messages.Message("BiosculpterPleasureCompletedMessage".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
		}
	}
}
