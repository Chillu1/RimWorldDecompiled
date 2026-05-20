using System.Collections.Generic;
using Verse;

namespace RimWorld;

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

	public AnimalType? disableHostilityFromAnimalType;

	public FactionDef disableHostilityFromFaction;

	public bool canBeSuppressed = true;

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
		if (!degreeDatas.Any())
		{
			yield return defName + " has no degree datas.";
		}
		for (int i = 0; i < degreeDatas.Count; i++)
		{
			TraitDegreeData dd = degreeDatas[i];
			if (degreeDatas.Count((TraitDegreeData traitDegreeData) => traitDegreeData.degree == dd.degree) > 1)
			{
				yield return ">1 datas for degree " + dd.degree;
			}
			if (dd.ingestibleModifiers.NullOrEmpty())
			{
				continue;
			}
			foreach (IngestibleModifiers ingestibleModifier in dd.ingestibleModifiers)
			{
				if (ingestibleModifier?.ingestible == null)
				{
					yield return "ingestible override has a null target ingestible";
				}
			}
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		foreach (TraitDegreeData degreeData in degreeDatas)
		{
			degreeData.ResolveReferences();
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

	public bool CanSuppress(Trait other)
	{
		if (!ConflictsWith(other))
		{
			return other.def == this;
		}
		return true;
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
