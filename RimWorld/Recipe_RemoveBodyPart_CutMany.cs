using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Recipe_RemoveBodyPart_CutMany : Recipe_RemoveBodyPart_Cut
{
	public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		IEnumerable<BodyPartRecord> source = FixedPartsToApplyOn(pawn, recipe);
		if (source.Any() && recipe.minPartCount <= source.Count())
		{
			yield return source.First();
		}
	}

	public IEnumerable<BodyPartRecord> FixedPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, (BodyPartRecord record) => !pawn.health.hediffSet.PartIsMissing(record));
	}

	public override void DamagePart(Pawn pawn, BodyPartRecord part)
	{
		foreach (BodyPartRecord item in FixedPartsToApplyOn(pawn, recipe))
		{
			pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, item));
		}
	}

	public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
	{
		return recipe.label;
	}
}
