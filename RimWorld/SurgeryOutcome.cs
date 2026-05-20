using Verse;

namespace RimWorld
{
	public abstract class SurgeryOutcome
	{
		public float chance;

		public int totalDamage;

		public bool applyEffectsToPart = true;

		public bool failure;

		[MustTranslate]
		public string letterLabel;

		[MustTranslate]
		public string letterText;

		public abstract bool Apply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part);

		protected void ApplyDamage(Pawn patient, BodyPartRecord part)
		{
			if (totalDamage > 0)
			{
				HealthUtility.GiveRandomSurgeryInjuries(patient, totalDamage, applyEffectsToPart ? part : null);
			}
		}

		protected void SendLetter(Pawn surgeon, Pawn patient, RecipeDef recipe)
		{
			if (!letterLabel.NullOrEmpty() && !letterText.NullOrEmpty())
			{
				Find.LetterStack.ReceiveLetter(letterLabel.Formatted(patient.Named("PATIENT")), letterText.Formatted(surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), LetterDefOf.NegativeEvent, patient);
			}
		}
	}
}
