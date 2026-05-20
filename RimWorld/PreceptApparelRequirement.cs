using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PreceptApparelRequirement : IExposable
	{
		public List<string> allowedFactionCategoryTags;

		public List<string> disallowedFactionCategoryTags;

		public List<MemeDef> anyMemeRequired;

		public ApparelRequirement requirement;

		public bool CanAddRequirement(Precept precept, List<PreceptApparelRequirement> currentRequirements, out string cannotAddReason, FactionDef generatingFor = null)
		{
			AcceptanceReport acceptanceReport = Compatible(precept.ideo, generatingFor);
			if (!acceptanceReport)
			{
				if (!string.IsNullOrWhiteSpace(acceptanceReport.Reason))
				{
					cannotAddReason = acceptanceReport.Reason;
				}
				else
				{
					cannotAddReason = null;
				}
				return false;
			}
			if (requirement.AllRequiredApparel().Any((ThingDef x) => currentRequirements.Any((PreceptApparelRequirement y) => y.requirement.AllRequiredApparel().Contains(x))))
			{
				cannotAddReason = "AlreadyAssigned".Translate();
				return false;
			}
			cannotAddReason = null;
			return true;
		}

		public bool CanWearTogetherWith(ThingDef apparel)
		{
			foreach (ThingDef item in requirement.AllRequiredApparel())
			{
				if (ApparelUtility.CanWearTogether(item, apparel, BodyDefOf.Human) || item == apparel)
				{
					return true;
				}
			}
			return false;
		}

		public bool RequirementOverlapsOther(List<PreceptApparelRequirement> currentRequirements, out string compatibilityReason)
		{
			foreach (IEnumerable<ThingDef> item in from x in currentRequirements.Except(this)
				select x.requirement.AllRequiredApparel())
			{
				foreach (ThingDef item2 in requirement.AllRequiredApparel())
				{
					foreach (ThingDef item3 in item)
					{
						if (!ApparelUtility.CanWearTogether(item2, item3, BodyDefOf.Human))
						{
							compatibilityReason = "CannotBeWornWith".Translate(item3);
							return true;
						}
					}
				}
			}
			compatibilityReason = null;
			return false;
		}

		public AcceptanceReport Compatible(Ideo ideo, FactionDef forFaction)
		{
			if (!requirement.IsValid)
			{
				return false;
			}
			AcceptanceReport result = CheckFaction(forFaction);
			if (!result.Accepted)
			{
				return result;
			}
			if (Find.World != null && Find.FactionManager != null)
			{
				foreach (Faction allFaction in Find.FactionManager.AllFactions)
				{
					if (allFaction.def != forFaction && allFaction.ideos != null && allFaction.ideos.PrimaryIdeo == ideo)
					{
						result = CheckFaction(allFaction.def);
						if (!result)
						{
							return result;
						}
					}
				}
			}
			if (anyMemeRequired != null && !ideo.memes.Any((MemeDef m) => anyMemeRequired.Contains(m)))
			{
				return new AcceptanceReport("RoleApparelRequirementIncompatibleAnyMemeRequired".Translate() + ": " + anyMemeRequired.Select((MemeDef m) => m.label).ToCommaListOr().CapitalizeFirst());
			}
			return true;
			AcceptanceReport CheckFaction(FactionDef faction)
			{
				if (faction != null && allowedFactionCategoryTags != null && !allowedFactionCategoryTags.Contains(faction.categoryTag))
				{
					return new AcceptanceReport("RoleApparelRequirementIncompatibleFaction".Translate(Find.ActiveLanguageWorker.WithIndefiniteArticle(allowedFactionCategoryTags.Select((string t) => ("RoleApparelRequirementIncompatibleFaction_Allowed_" + t).Translate().Resolve()).ToCommaListOr())));
				}
				if (faction != null && disallowedFactionCategoryTags != null && disallowedFactionCategoryTags.Contains(faction.categoryTag))
				{
					return new AcceptanceReport("RoleApparelRequirementIncompatibleFaction".Translate(Find.ActiveLanguageWorker.WithIndefiniteArticle(disallowedFactionCategoryTags.Select((string t) => ("RoleApparelRequirementIncompatibleFaction_Disallowed_" + t).Translate().Resolve()).ToCommaListOr())));
				}
				return true;
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allowedFactionCategoryTags, "allowedFactionCategoryTags", LookMode.Value);
			Scribe_Collections.Look(ref disallowedFactionCategoryTags, "disallowedFactionCategoryTags", LookMode.Value);
			Scribe_Collections.Look(ref anyMemeRequired, "anyMemeRequired", LookMode.Def);
			Scribe_Deep.Look(ref requirement, "requirement");
		}

		public virtual void CopyTo(PreceptApparelRequirement other)
		{
			if (allowedFactionCategoryTags != null)
			{
				if (other.allowedFactionCategoryTags == null)
				{
					other.allowedFactionCategoryTags = new List<string>();
				}
				other.allowedFactionCategoryTags.Clear();
				other.allowedFactionCategoryTags.AddRange(allowedFactionCategoryTags);
			}
			else
			{
				other.allowedFactionCategoryTags = null;
			}
			if (disallowedFactionCategoryTags != null)
			{
				if (other.disallowedFactionCategoryTags == null)
				{
					other.disallowedFactionCategoryTags = new List<string>();
				}
				other.disallowedFactionCategoryTags.Clear();
				other.disallowedFactionCategoryTags.AddRange(disallowedFactionCategoryTags);
			}
			else
			{
				other.disallowedFactionCategoryTags = null;
			}
			if (anyMemeRequired != null)
			{
				if (other.anyMemeRequired == null)
				{
					other.anyMemeRequired = new List<MemeDef>();
				}
				other.anyMemeRequired.Clear();
				other.anyMemeRequired.AddRange(anyMemeRequired);
			}
			else
			{
				other.anyMemeRequired = null;
			}
			if (requirement != null)
			{
				ApparelRequirement other2 = new ApparelRequirement();
				requirement.CopyTo(other2);
				other.requirement = other2;
			}
			else
			{
				other.requirement = null;
			}
		}
	}
}
