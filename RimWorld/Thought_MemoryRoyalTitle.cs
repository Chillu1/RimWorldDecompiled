using Verse;

namespace RimWorld
{
	public class Thought_MemoryRoyalTitle : Thought_Memory
	{
		public RoyalTitleDef titleDef;

		public override string LabelCap => base.CurStage.label.Formatted(titleDef.GetLabelCapFor(pawn).Named("TITLE"));

		public override string Description => base.CurStage.description.Formatted(titleDef.GetLabelCapFor(pawn).Named("TITLE"), pawn.Named("PAWN"));

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref titleDef, "titleDef");
		}
	}
}
