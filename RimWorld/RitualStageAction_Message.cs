using Verse;

namespace RimWorld
{
	public class RitualStageAction_Message : RitualStageAction
	{
		[MustTranslate]
		public string text;

		public MessageTypeDef messageTypeDef;

		public override void Apply(LordJob_Ritual ritual)
		{
			Messages.Message(text.Formatted(ritual.Ritual.Label).CapitalizeFirst(), ritual.selectedTarget, messageTypeDef, historical: false);
		}

		public override void ExposeData()
		{
			Scribe_Defs.Look(ref messageTypeDef, "messageTypeDef");
			Scribe_Values.Look(ref text, "text");
		}
	}
}
