using Verse;

namespace RimWorld
{
	public class SurgeryOutcome_Failure : SurgeryOutcome
	{
		protected virtual bool CanApply(RecipeDef recipe)
		{
			return Rand.Chance(chance);
		}

		public override bool Apply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
		{
			if (CanApply(recipe))
			{
				ApplyDamage(patient, part);
				PostDamagedApplied(patient);
				if (!patient.Dead)
				{
					TryGainBotchedSurgeryThought(patient, surgeon);
				}
				SendLetter(surgeon, patient, recipe);
				return true;
			}
			return false;
		}

		protected virtual void PostDamagedApplied(Pawn patient)
		{
		}

		protected void TryGainBotchedSurgeryThought(Pawn patient, Pawn surgeon)
		{
			if (patient.RaceProps.Humanlike && patient.needs.mood != null)
			{
				patient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BotchedMySurgery, surgeon);
			}
		}
	}
}
