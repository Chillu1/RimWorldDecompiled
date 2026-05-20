using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ApparelRequirement : IExposable
	{
		public List<BodyPartGroupDef> bodyPartGroupsMatchAny;

		public List<string> requiredTags;

		public List<string> allowedTags;

		public List<ThingDef> requiredDefs;

		[MustTranslate]
		public string groupLabel;

		[Unsaved(false)]
		private bool? isValid;

		public bool IsValid
		{
			get
			{
				if (!isValid.HasValue)
				{
					isValid = AllRequiredApparel().Any();
				}
				return isValid.Value;
			}
		}

		private bool MatchesBodyPartGroups(ThingDef apparel)
		{
			foreach (BodyPartGroupDef item in bodyPartGroupsMatchAny)
			{
				if (apparel.apparel.bodyPartGroups.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		private bool HasRequiredTag(ThingDef apparel)
		{
			if (requiredTags != null)
			{
				foreach (string tag in apparel.apparel.tags)
				{
					if (requiredTags.Contains(tag))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool WearsAny(Pawn p, ThingDef apparel)
		{
			foreach (Apparel item in p.apparel.WornApparel)
			{
				if (item.def == apparel)
				{
					return true;
				}
			}
			return false;
		}

		public bool AllowedForPawn(Pawn p, ThingDef apparel, bool ignoreGender = false)
		{
			if (!apparel.IsApparel)
			{
				return false;
			}
			if (!apparel.apparel.PawnCanWear(p, ignoreGender))
			{
				return false;
			}
			if (apparel.apparel.tags == null)
			{
				return false;
			}
			bool flag = requiredDefs != null && requiredDefs.Contains(apparel);
			if (!flag)
			{
				foreach (string tag in apparel.apparel.tags)
				{
					if ((requiredTags != null && requiredTags.Contains(tag)) || (allowedTags != null && allowedTags.Contains(tag)))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				return MatchesBodyPartGroups(apparel);
			}
			return false;
		}

		public IEnumerable<ThingDef> AllAllowedApparelForPawn(Pawn p, bool ignoreGender = false, bool includeWorn = false)
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
			{
				if (AllowedForPawn(p, item, ignoreGender) && (includeWorn || !WearsAny(p, item)))
				{
					yield return item;
				}
			}
		}

		public bool RequiredForPawn(Pawn p, ThingDef apparel, bool ignoreGender = false)
		{
			if (!apparel.IsApparel)
			{
				return false;
			}
			if (!apparel.apparel.PawnCanWear(p, ignoreGender))
			{
				return false;
			}
			if (apparel.apparel.tags == null)
			{
				return false;
			}
			if ((requiredDefs != null && requiredDefs.Contains(apparel)) || HasRequiredTag(apparel))
			{
				return MatchesBodyPartGroups(apparel);
			}
			return false;
		}

		public IEnumerable<ThingDef> AllRequiredApparelForPawn(Pawn p, bool ignoreGender = false, bool includeWorn = false)
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
			{
				if (RequiredForPawn(p, item, ignoreGender) && (includeWorn || !WearsAny(p, item)))
				{
					yield return item;
				}
			}
		}

		public IEnumerable<ThingDef> AllRequiredApparel(Gender gender = Gender.None)
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
			{
				if (item.IsApparel && item.apparel.tags != null && ((requiredDefs != null && requiredDefs.Contains(item)) || HasRequiredTag(item)) && item.apparel.PawnCanWear(gender) && MatchesBodyPartGroups(item))
				{
					yield return item;
				}
			}
		}

		public bool ApparelMeetsRequirement(ThingDef thingDef, bool allowUnmatched = true)
		{
			bool flag = false;
			for (int i = 0; i < bodyPartGroupsMatchAny.Count; i++)
			{
				if (thingDef.apparel.bodyPartGroups.Contains(bodyPartGroupsMatchAny[i]))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (requiredDefs != null && requiredDefs.Contains(thingDef))
				{
					return true;
				}
				if (requiredTags != null)
				{
					for (int j = 0; j < requiredTags.Count; j++)
					{
						if (thingDef.apparel.tags.Contains(requiredTags[j]))
						{
							return true;
						}
					}
				}
				if (allowedTags != null)
				{
					for (int k = 0; k < allowedTags.Count; k++)
					{
						if (thingDef.apparel.tags.Contains(allowedTags[k]))
						{
							return true;
						}
					}
				}
				return false;
			}
			return allowUnmatched;
		}

		public bool IsMet(Pawn p)
		{
			foreach (Apparel item in p.apparel.WornApparel)
			{
				bool flag = false;
				for (int i = 0; i < bodyPartGroupsMatchAny.Count; i++)
				{
					if (item.def.apparel.bodyPartGroups.Contains(bodyPartGroupsMatchAny[i]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (requiredDefs != null && requiredDefs.Contains(item.def))
				{
					return true;
				}
				if (requiredTags != null)
				{
					for (int j = 0; j < requiredTags.Count; j++)
					{
						if (item.def.apparel.tags.Contains(requiredTags[j]))
						{
							return true;
						}
					}
				}
				if (allowedTags == null)
				{
					continue;
				}
				for (int k = 0; k < allowedTags.Count; k++)
				{
					if (item.def.apparel.tags.Contains(allowedTags[k]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool SameApparelAs(ApparelRequirement other)
		{
			if (requiredDefs.SetsEqual(other.requiredDefs) && requiredTags.SetsEqual(other.requiredTags))
			{
				return allowedTags.SetsEqual(other.allowedTags);
			}
			return false;
		}

		public bool IsActive(Pawn forPawn)
		{
			if (forPawn.apparel == null)
			{
				return false;
			}
			return forPawn.apparel.ActiveRequirementsForReading.Contains(this);
		}

		public ThingDef RandomRequiredApparelForPawnInGeneration(Pawn p, Func<ThingDef, bool> validator)
		{
			ThingDef result = null;
			if (!DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef a) => a.IsApparel && a.apparel.tags != null && a.apparel.bodyPartGroups.Any((BodyPartGroupDef b) => bodyPartGroupsMatchAny.Contains(b)) && a.apparel.tags.Any((string t) => requiredTags.Contains(t)) && a.apparel.PawnCanWear(p) && (validator == null || validator(a))).TryRandomElementByWeight((ThingDef a) => a.generateCommonality, out result))
			{
				return null;
			}
			return result;
		}

		public override string ToString()
		{
			string text = "";
			if (requiredTags != null)
			{
				text += string.Join(",", requiredTags.ToArray());
			}
			if (allowedTags != null)
			{
				if (!text.NullOrEmpty())
				{
					text += "|";
				}
				text += string.Join(",", allowedTags.ToArray());
			}
			if (requiredDefs != null)
			{
				if (!text.NullOrEmpty())
				{
					text += "|";
				}
				text += string.Join(",", requiredDefs.Select((ThingDef d) => d.LabelCap).ToArray());
			}
			return "(" + string.Join(",", bodyPartGroupsMatchAny.Select((BodyPartGroupDef a) => a.defName).ToArray()) + ") -> " + text;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref bodyPartGroupsMatchAny, "bodyPartGroupsMatchAny", LookMode.Def);
			Scribe_Collections.Look(ref requiredDefs, "requiredDefs", LookMode.Def);
			Scribe_Collections.Look(ref requiredTags, "requiredTags", LookMode.Value);
			Scribe_Collections.Look(ref allowedTags, "allowedTags", LookMode.Value);
			Scribe_Values.Look(ref groupLabel, "groupLabel");
		}

		public virtual void CopyTo(ApparelRequirement other)
		{
			if (bodyPartGroupsMatchAny != null)
			{
				if (other.bodyPartGroupsMatchAny == null)
				{
					other.bodyPartGroupsMatchAny = new List<BodyPartGroupDef>();
				}
				other.bodyPartGroupsMatchAny.Clear();
				other.bodyPartGroupsMatchAny.AddRange(bodyPartGroupsMatchAny);
			}
			else
			{
				other.bodyPartGroupsMatchAny = null;
			}
			if (requiredTags != null)
			{
				if (other.requiredTags == null)
				{
					other.requiredTags = new List<string>();
				}
				other.requiredTags.Clear();
				other.requiredTags.AddRange(requiredTags);
			}
			else
			{
				other.requiredTags = null;
			}
			if (allowedTags != null)
			{
				if (other.allowedTags == null)
				{
					other.allowedTags = new List<string>();
				}
				other.allowedTags.Clear();
				other.allowedTags.AddRange(allowedTags);
			}
			else
			{
				other.allowedTags = null;
			}
			if (requiredDefs != null)
			{
				if (other.requiredDefs == null)
				{
					other.requiredDefs = new List<ThingDef>();
				}
				other.requiredDefs.Clear();
				other.requiredDefs.AddRange(requiredDefs);
			}
			else
			{
				other.requiredDefs = null;
			}
			other.groupLabel = groupLabel;
		}
	}
}
