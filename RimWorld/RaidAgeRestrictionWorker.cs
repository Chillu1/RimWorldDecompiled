using Verse;

namespace RimWorld
{
	public class RaidAgeRestrictionWorker
	{
		public RaidAgeRestrictionDef def;

		public virtual bool CanUseWith(IncidentParms parms)
		{
			if (GenDate.DaysPassedSinceSettle >= def.earliestDay && parms.faction != null && parms.faction.def.humanlikeFaction)
			{
				return !parms.faction.def.disallowedRaidAgeRestrictions.NotNullAndContains(def);
			}
			return false;
		}

		public bool ShouldApplyToKind(PawnKindDef kind)
		{
			return kind.RaceProps.Humanlike;
		}

		public bool CanUseKind(PawnKindDef kind)
		{
			if (def.ageRange.HasValue)
			{
				FloatRange value = def.ageRange.Value;
				if ((float)kind.minGenerationAge > value.max)
				{
					return false;
				}
				if ((float)kind.maxGenerationAge < value.min)
				{
					return false;
				}
			}
			if (kind.apparelRequired != null)
			{
				for (int i = 0; i < kind.apparelRequired.Count; i++)
				{
					if (!kind.apparelRequired[i].apparel.developmentalStageFilter.Has(def.developmentStage))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
