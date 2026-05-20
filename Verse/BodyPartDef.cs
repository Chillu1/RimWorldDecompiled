using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class BodyPartDef : Def
{
	[MustTranslate]
	public string labelShort;

	public List<BodyPartTagDef> tags = new List<BodyPartTagDef>();

	public int hitPoints = 10;

	public float permanentInjuryChanceFactor = 1f;

	public float bleedRate = 1f;

	public float frostbiteVulnerability;

	private bool skinCovered;

	private bool solid;

	public bool alive = true;

	public bool delicate;

	public bool canScarify;

	public bool beautyRelated;

	public bool conceptual;

	public bool socketed;

	public ThingDef spawnThingOnRemoved;

	public bool pawnGeneratorCanAmputate;

	public bool canSuggestAmputation = true;

	public bool forceAlwaysRemovable;

	public Dictionary<DamageDef, float> hitChanceFactors;

	public bool destroyableByDamage = true;

	public bool canBeVacuumBurnt = true;

	[MustTranslate]
	public string removeRecipeLabelOverride;

	public float executionPartPriority;

	[Unsaved(false)]
	private string cachedLabelShortCap;

	public bool IsSolidInDefinition_Debug => solid;

	public bool IsSkinCoveredInDefinition_Debug => skinCovered;

	public bool IsMirroredPart => tags.Contains(BodyPartTagDefOf.Mirrored);

	public string LabelShort
	{
		get
		{
			if (!labelShort.NullOrEmpty())
			{
				return labelShort;
			}
			return label;
		}
	}

	public string LabelShortCap
	{
		get
		{
			if (labelShort.NullOrEmpty())
			{
				return LabelCap;
			}
			if (cachedLabelShortCap == null)
			{
				cachedLabelShortCap = labelShort.CapitalizeFirst();
			}
			return cachedLabelShortCap;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (frostbiteVulnerability > 10f)
		{
			yield return "frostbitePriority > max 10: " + frostbiteVulnerability;
		}
		if (solid && bleedRate > 0f)
		{
			yield return "solid but bleedRate is not zero";
		}
	}

	public bool IsSolid(BodyPartRecord part, List<Hediff> hediffs)
	{
		for (BodyPartRecord bodyPartRecord = part; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
		{
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Part == bodyPartRecord && hediffs[i] is Hediff_AddedPart)
				{
					if (hediffs[i].def.addedPartProps.solid)
					{
						return !hediffs[i].def.organicAddedBodypart;
					}
					return false;
				}
			}
		}
		return solid;
	}

	public bool IsSkinCovered(BodyPartRecord part, HediffSet body)
	{
		if (body.PartOrAnyAncestorHasDirectlyAddedParts(part))
		{
			return false;
		}
		return skinCovered;
	}

	public float GetMaxHealth(Pawn pawn)
	{
		return Mathf.CeilToInt((float)hitPoints * pawn.HealthScale);
	}

	public float GetHitChanceFactorFor(DamageDef damage)
	{
		if (conceptual)
		{
			return 0f;
		}
		if (hitChanceFactors == null)
		{
			return 1f;
		}
		if (hitChanceFactors.TryGetValue(damage, out var value))
		{
			return value;
		}
		return 1f;
	}
}
