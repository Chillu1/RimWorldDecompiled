using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeEffectDef : Def
	{
		public List<SurgeryOutcomeComp> comps;

		public List<SurgeryOutcome> outcomes;

		public float GetQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			float quality = 1f;
			if (!comps.NullOrEmpty())
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].Affects(recipe, surgeon, patient, part))
					{
						comps[i].AffectQuality(recipe, surgeon, patient, ingredients, part, bill, ref quality);
					}
				}
			}
			return quality;
		}

		public SurgeryOutcome GetOutcome(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			float quality = GetQuality(recipe, surgeon, patient, ingredients, part, bill);
			if (!comps.NullOrEmpty())
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].Affects(recipe, surgeon, patient, part))
					{
						comps[i].PreApply(quality, recipe, surgeon, patient, ingredients, part, bill);
					}
				}
			}
			if (!outcomes.NullOrEmpty())
			{
				for (int j = 0; j < outcomes.Count; j++)
				{
					if (outcomes[j].Apply(quality, recipe, surgeon, patient, part))
					{
						return outcomes[j];
					}
				}
			}
			return null;
		}
	}
}
