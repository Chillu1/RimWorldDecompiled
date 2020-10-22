using RimWorld;

namespace Verse
{
	public class HediffComp_Discoverable : HediffComp
	{
		private bool discovered;

		public HediffCompProperties_Discoverable Props => (HediffCompProperties_Discoverable)props;

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref discovered, "discovered", defaultValue: false);
		}

		public override bool CompDisallowVisible()
		{
			return !discovered;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Find.TickManager.TicksGame % 103 == 0)
			{
				CheckDiscovered();
			}
		}

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			CheckDiscovered();
		}

		private void CheckDiscovered()
		{
			if (discovered || !parent.CurStage.becomeVisible)
			{
				return;
			}
			discovered = true;
			if (!Props.sendLetterWhenDiscovered || !PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				return;
			}
			if (base.Pawn.RaceProps.Humanlike)
			{
				string str = (Props.discoverLetterLabel.NullOrEmpty() ? ((string)("LetterLabelNewDisease".Translate() + ": " + base.Def.LabelCap)) : string.Format(Props.discoverLetterLabel, base.Pawn.LabelShortCap).CapitalizeFirst());
				string str2 = ((!Props.discoverLetterText.NullOrEmpty()) ? ((string)Props.discoverLetterText.Formatted(base.Pawn.LabelIndefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst()) : ((parent.Part != null) ? ((string)"NewPartDisease".Translate(base.Pawn.Named("PAWN"), parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.label).AdjustedFor(base.Pawn).CapitalizeFirst()) : ((string)"NewDisease".Translate(base.Pawn.Named("PAWN"), base.Def.label, base.Pawn.LabelDefinite()).AdjustedFor(base.Pawn).CapitalizeFirst())));
				Find.LetterStack.ReceiveLetter(str, str2, (Props.letterType != null) ? Props.letterType : LetterDefOf.NegativeEvent, base.Pawn);
				return;
			}
			string text;
			if (Props.discoverLetterText.NullOrEmpty())
			{
				text = ((parent.Part != null) ? ((string)"NewPartDiseaseAnimal".Translate(base.Pawn.LabelShort, parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.LabelCap, base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst()) : ((string)"NewDiseaseAnimal".Translate(base.Pawn.LabelShort, base.Def.LabelCap, base.Pawn.LabelDefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst()));
			}
			else
			{
				string value = base.Pawn.KindLabelIndefinite();
				if (base.Pawn.Name.IsValid && !base.Pawn.Name.Numerical)
				{
					value = string.Concat(base.Pawn.Name, " (", base.Pawn.KindLabel, ")");
				}
				text = Props.discoverLetterText.Formatted(value, base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst();
			}
			Messages.Message(text, base.Pawn, (Props.messageType != null) ? Props.messageType : MessageTypeDefOf.NegativeHealthEvent);
		}

		public override void Notify_PawnDied()
		{
			CheckDiscovered();
		}

		public override string CompDebugString()
		{
			return "discovered: " + discovered;
		}
	}
}
