using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TraitSet : IExposable
	{
		protected Pawn pawn;

		public List<Trait> allTraits = new List<Trait>();

		public float HungerRateFactor
		{
			get
			{
				float num = 1f;
				foreach (Trait allTrait in allTraits)
				{
					num *= allTrait.CurrentData.hungerRateFactor;
				}
				return num;
			}
		}

		public IEnumerable<MentalBreakDef> TheOnlyAllowedMentalBreaks
		{
			get
			{
				for (int i = 0; i < allTraits.Count; i++)
				{
					Trait trait = allTraits[i];
					if (trait.CurrentData.theOnlyAllowedMentalBreaks != null)
					{
						for (int j = 0; j < trait.CurrentData.theOnlyAllowedMentalBreaks.Count; j++)
						{
							yield return trait.CurrentData.theOnlyAllowedMentalBreaks[j];
						}
					}
				}
			}
		}

		public TraitSet(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allTraits, "allTraits", LookMode.Deep);
		}

		public void GainTrait(Trait trait)
		{
			if (HasTrait(trait.def))
			{
				Log.Warning(string.Concat(pawn, " already has trait ", trait.def));
				return;
			}
			allTraits.Add(trait);
			pawn.Notify_DisabledWorkTypesChanged();
			if (pawn.skills != null)
			{
				pawn.skills.Notify_SkillDisablesChanged();
			}
			if (!pawn.Dead && pawn.RaceProps.Humanlike && pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
		}

		public bool HasTrait(TraitDef tDef)
		{
			for (int i = 0; i < allTraits.Count; i++)
			{
				if (allTraits[i].def == tDef)
				{
					return true;
				}
			}
			return false;
		}

		public Trait GetTrait(TraitDef tDef)
		{
			for (int i = 0; i < allTraits.Count; i++)
			{
				if (allTraits[i].def == tDef)
				{
					return allTraits[i];
				}
			}
			return null;
		}

		public int DegreeOfTrait(TraitDef tDef)
		{
			for (int i = 0; i < allTraits.Count; i++)
			{
				if (allTraits[i].def == tDef)
				{
					return allTraits[i].Degree;
				}
			}
			return 0;
		}
	}
}
