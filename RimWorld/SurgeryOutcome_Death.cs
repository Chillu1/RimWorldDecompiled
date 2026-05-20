using Verse;

namespace RimWorld;

public class SurgeryOutcome_Death : SurgeryOutcome
{
	public override bool Apply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
	{
		if (ModsConfig.BiotechActive && patient.genes != null && patient.genes.HasActiveGene(GeneDefOf.Deathless))
		{
			return false;
		}
		if (Rand.Chance(recipe.deathOnFailedSurgeryChance))
		{
			ApplyDamage(patient, part);
			if (!patient.Dead)
			{
				patient.Kill(null, null);
			}
			SendLetter(surgeon, patient, recipe);
			return true;
		}
		return false;
	}
}
