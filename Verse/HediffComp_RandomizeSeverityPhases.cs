using System.Text;
using RimWorld;

namespace Verse
{
	public class HediffComp_RandomizeSeverityPhases : HediffComp_Randomizer
	{
		private int? randomPhaseIndex;

		public HediffCompProperties_RandomizeSeverityPhases Props => (HediffCompProperties_RandomizeSeverityPhases)props;

		public override string CompLabelPrefix => CurrentOption()?.labelPrefix;

		public override string CompDescriptionExtra => CurrentOption()?.descriptionExtra;

		public HediffCompProperties_RandomizeSeverityPhases.Phase CurrentOption()
		{
			if (randomPhaseIndex >= 0 && randomPhaseIndex < Props.phases?.Count)
			{
				return Props.phases[randomPhaseIndex.Value];
			}
			return null;
		}

		public override void CompPostMake()
		{
			base.CompPostMake();
			Randomize();
		}

		public override void Randomize()
		{
			if (Props.phases != null && Props.phases.Count != 0)
			{
				int num = Rand.Range(0, Props.phases.Count);
				if (randomPhaseIndex.HasValue && num != randomPhaseIndex && !Props.notifyMessage.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(parent.pawn))
				{
					Messages.Message(Props.notifyMessage.Formatted(parent.pawn.Named("PAWN"), Props.phases[randomPhaseIndex.Value].labelPrefix, Props.phases[num].labelPrefix), parent.pawn, MessageTypeDefOf.NeutralEvent);
				}
				randomPhaseIndex = num;
			}
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref randomPhaseIndex, "randomPhaseIndex");
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			severityAdjustment += (CurrentOption()?.severityPerDay ?? 0f) / 60000f;
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.CompDebugString());
			if (!base.Pawn.Dead)
			{
				stringBuilder.AppendLine("severity/day: " + (CurrentOption()?.severityPerDay ?? 0f).ToString("F3"));
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
	}
}
