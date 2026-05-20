using Verse;

namespace RimWorld
{
	public class Thought_AttendedRitual : Thought_Memory
	{
		public override string LabelCap => base.CurStage.LabelCap.Formatted(sourcePrecept.Named("RITUAL"));

		public override string Description => base.CurStage.description.Formatted(sourcePrecept.Named("RITUAL"));
	}
}
