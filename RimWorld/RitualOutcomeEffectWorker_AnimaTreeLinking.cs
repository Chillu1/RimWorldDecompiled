using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualOutcomeEffectWorker_AnimaTreeLinking : RitualOutcomeEffectWorker_FromQuality
	{
		public static readonly SimpleCurve RestoredGrassFromQuality = new SimpleCurve
		{
			new CurvePoint(0.2f, 0f),
			new CurvePoint(0.4f, 1f),
			new CurvePoint(0.6f, 3f),
			new CurvePoint(0.8f, 5f),
			new CurvePoint(1f, 8f)
		};

		public override bool SupportsAttachableOutcomeEffect => false;

		public RitualOutcomeEffectWorker_AnimaTreeLinking()
		{
		}

		public RitualOutcomeEffectWorker_AnimaTreeLinking(RitualOutcomeEffectDef def)
			: base(def)
		{
		}

		public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
		{
			float quality = GetQuality(jobRitual, progress);
			Pawn pawn = jobRitual.PawnWithRole("organizer");
			CompPsylinkable obj = jobRitual.selectedTarget.Thing?.TryGetComp<CompPsylinkable>();
			int num = (int)RestoredGrassFromQuality.Evaluate(quality);
			obj?.FinishLinkingRitual(pawn, num);
			string text = "LetterTextLinkingRitualCompleted".Translate(pawn.Named("PAWN"), jobRitual.selectedTarget.Thing.Named("LINKABLE"));
			if (num > 0)
			{
				text += " " + "LetterTextLinkingRitualCompletedAnimaGrass".Translate(num);
			}
			text = text + "\n\n" + OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
			Find.LetterStack.ReceiveLetter("LetterLabelLinkingRitualCompleted".Translate(), text, LetterDefOf.RitualOutcomePositive, new LookTargets(pawn, jobRitual.selectedTarget.Thing));
		}
	}
}
