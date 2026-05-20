using Verse;

namespace RimWorld
{
	public class Thought_RelicAtRitual : Thought_Memory
	{
		public string relicName;

		public override string LabelCap => base.CurStage.label.Formatted(relicName.Named("RELICNAME")).CapitalizeFirst();

		public override string Description => base.CurStage.description.Formatted(relicName.Named("RELICNAME")).CapitalizeFirst();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref relicName, "relicName");
		}
	}
}
