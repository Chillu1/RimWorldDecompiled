using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class TraitDef : Def
	{
		public List<TraitDegreeData> degreeDatas = new List<TraitDegreeData>();

		public List<TraitDef> conflictingTraits = new List<TraitDef>();

		public List<string> exclusionTags = new List<string>();

		public List<SkillDef> conflictingPassions = new List<SkillDef>();

		public List<SkillDef> forcedPassions = new List<SkillDef>();

		public List<WorkTypeDef> requiredWorkTypes = new List<WorkTypeDef>();

		public WorkTags requiredWorkTags;

		public List<WorkTypeDef> disabledWorkTypes = new List<WorkTypeDef>();

		public WorkTags disabledWorkTags;

		private float commonality = 1f;

		private float commonalityFemale = -1f;

		public bool allowOnHostileSpawn = true;

		public static TraitDef Named(string defName)
		{
			return DefDatabase<TraitDef>.GetNamed(defName);
		}

		public TraitDegreeData DataAtDegree(int degree)
		{
			for (int i = 0; i < degreeDatas.Count; i++)
			{
				if (degreeDatas[i].degree == degree)
				{
					return degreeDatas[i];
				}
			}
			Log.Error(defName + " found no data at degree " + degree + ", returning first defined.");
			return degreeDatas[0];
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (commonality < 0.001f && commonalityFemale < 0.001f)
			{
				yield return "TraitDef " + defName + " has 0 commonality.";
			}
			if (!degreeDatas.Any())
			{
				yield return defName + " has no degree datas.";
			}
			for (int i = 0; i < degreeDatas.Count; i++)
			{
				TraitDegreeData dd3 = degreeDatas[i];
				if (degreeDatas.Where((TraitDegreeData dd2) => dd2.degree == dd3.degree).Count() > 1)
				{
					yield return ">1 datas for degree " + dd3.degree;
				}
			}
		}

		public bool ConflictsWith(Trait other)
		{
			return ConflictsWith(other.def);
		}

		public bool ConflictsWith(TraitDef other)
		{
			if ((other.conflictingTraits != null && other.conflictingTraits.Contains(this)) || (conflictingTraits != null && conflictingTraits.Contains(other)))
			{
				return true;
			}
			if (exclusionTags != null && other.exclusionTags != null)
			{
				for (int i = 0; i < exclusionTags.Count; i++)
				{
					if (other.exclusionTags.Contains(exclusionTags[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ConflictsWithPassion(SkillDef passion)
		{
			if (conflictingPassions != null)
			{
				return conflictingPassions.Contains(passion);
			}
			return false;
		}

		public bool RequiresPassion(SkillDef passion)
		{
			if (forcedPassions != null)
			{
				return forcedPassions.Contains(passion);
			}
			return false;
		}

		public float GetGenderSpecificCommonality(Gender gender)
		{
			if (gender == Gender.Female && commonalityFemale >= 0f)
			{
				return commonalityFemale;
			}
			return commonality;
		}
	}
}
