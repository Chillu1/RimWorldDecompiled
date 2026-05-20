using RimWorld;

namespace Verse
{
	public class HediffComp_GiveRandomSituationalThought : HediffComp
	{
		public ThoughtDef selectedThought;

		public HediffCompProperties_GiveRandomSituationalThought Props => (HediffCompProperties_GiveRandomSituationalThought)props;

		public override string CompLabelInBracketsExtra => selectedThought?.labelInBracketsExtraForHediff;

		public override void CompPostMake()
		{
			base.CompPostMake();
			selectedThought = Props.thoughtDefs.RandomElement();
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Defs.Look(ref selectedThought, "selectedThought");
		}
	}
}
