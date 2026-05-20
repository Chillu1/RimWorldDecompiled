using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class ApparelProperties : IRenderNodePropertiesParent
{
	public List<BodyPartGroupDef> bodyPartGroups = new List<BodyPartGroupDef>();

	public List<ApparelLayerDef> layers = new List<ApparelLayerDef>();

	[NoTranslate]
	public string wornGraphicPath = "";

	[NoTranslate]
	public List<string> wornGraphicPaths;

	public WornGraphicData wornGraphicData;

	public bool useWornGraphicMask;

	private List<PawnRenderNodeProperties> renderNodeProperties;

	public List<RenderSkipFlagDef> renderSkipFlags;

	public DrawData drawData;

	public PawnRenderNodeTagDef parentTagDef;

	public List<int> forceEyesVisibleForRotations = new List<int>();

	public bool shellRenderedBehindHead;

	[NoTranslate]
	public List<string> tags = new List<string>();

	[NoTranslate]
	public List<string> defaultOutfitTags;

	public bool canBeGeneratedToSatisfyWarmth = true;

	public bool canBeGeneratedToSatisfyVacuumResistance = true;

	public bool canBeGeneratedToSatisfyToxicEnvironmentResistance = true;

	public bool canBeDesiredForIdeo = true;

	public List<string> ideoDesireAllowedFactionCategoryTags;

	public List<string> ideoDesireDisallowedFactionCategoryTags;

	public List<AbilityDef> abilities = new List<AbilityDef>();

	public float wearPerDay = 0.4f;

	public bool careIfWornByCorpse = true;

	public bool careIfDamaged = true;

	public bool ignoredByNonViolent;

	public bool ai_pickUpOpportunistically;

	public bool blocksAddedPartWoundGraphics;

	public bool blocksVision;

	public bool immuneToToxGasExposure;

	public bool slaveApparel;

	public bool mechanitorApparel;

	public bool legsNakedUnlessCoveredBySomethingElse;

	public bool useDeflectMetalEffect;

	public bool countsAsClothingForNudity = true;

	public bool anyTechLevelCanUseForWarmth;

	public Gender gender;

	public float scoreOffset;

	public DevelopmentalStage developmentalStageFilter = DevelopmentalStage.Adult;

	public SoundDef soundWear;

	public SoundDef soundRemove;

	[Unsaved(false)]
	private float cachedHumanBodyCoverage = -1f;

	[Unsaved(false)]
	private BodyPartGroupDef[][] interferingBodyPartGroups;

	private static BodyPartGroupDef[] apparelRelevantGroups;

	public bool HasDefinedGraphicProperties => !renderNodeProperties.NullOrEmpty();

	public List<PawnRenderNodeProperties> RenderNodeProperties => renderNodeProperties;

	public ApparelLayerDef LastLayer
	{
		get
		{
			if (layers.Count > 0)
			{
				return layers[layers.Count - 1];
			}
			Log.ErrorOnce("Failed to get last layer on apparel item (see your config errors)", 31234937);
			return ApparelLayerDefOf.Belt;
		}
	}

	public float HumanBodyCoverage
	{
		get
		{
			if (cachedHumanBodyCoverage < 0f)
			{
				cachedHumanBodyCoverage = 0f;
				List<BodyPartRecord> allParts = BodyDefOf.Human.AllParts;
				for (int i = 0; i < allParts.Count; i++)
				{
					if (CoversBodyPart(allParts[i]))
					{
						cachedHumanBodyCoverage += allParts[i].coverageAbs;
					}
				}
			}
			return cachedHumanBodyCoverage;
		}
	}

	public bool CorrectGenderForWearing(Gender wearerGender)
	{
		if (gender == Gender.None || wearerGender == Gender.None)
		{
			return true;
		}
		return gender == wearerGender;
	}

	public bool CorrectAgeForWearing(Pawn pawn)
	{
		if (ModsConfig.BiotechActive && !developmentalStageFilter.Has(pawn.DevelopmentalStage))
		{
			return false;
		}
		return true;
	}

	public bool PawnCanWear(Gender gender = Gender.None, DevelopmentalStage developmentalStage = DevelopmentalStage.Adult)
	{
		if (!CorrectGenderForWearing(gender))
		{
			return false;
		}
		if (!developmentalStageFilter.Has(developmentalStage))
		{
			return false;
		}
		return true;
	}

	public bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
	{
		return PawnCanWear((!ignoreGender) ? pawn.gender : Gender.None, pawn.DevelopmentalStage);
	}

	public static void ResetStaticData()
	{
		apparelRelevantGroups = DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => td.IsApparel).SelectMany((ThingDef td) => td.apparel.bodyPartGroups).Distinct()
			.ToArray();
	}

	public IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (layers.NullOrEmpty())
		{
			yield return parentDef.defName + " apparel has no layers.";
		}
		if (!HasDefinedGraphicProperties)
		{
			yield break;
		}
		foreach (PawnRenderNodeProperties renderNodeProperty in renderNodeProperties)
		{
			foreach (string item in renderNodeProperty.ConfigErrors())
			{
				yield return item;
			}
		}
	}

	public void PostLoadSpecial(ThingDef thingDef)
	{
	}

	public bool CoversBodyPart(BodyPartRecord partRec)
	{
		for (int i = 0; i < partRec.groups.Count; i++)
		{
			if (bodyPartGroups.Contains(partRec.groups[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool CoversBodyPartGroup(BodyPartGroupDef group)
	{
		return bodyPartGroups.NotNullAndContains(group);
	}

	public string GetCoveredOuterPartsString(BodyDef body)
	{
		return (from part in body.AllParts.Where((BodyPartRecord x) => x.depth == BodyPartDepth.Outside && x.groups.Any((BodyPartGroupDef y) => bodyPartGroups.Contains(y))).Distinct()
			select part.Label).ToCommaList().CapitalizeFirst();
	}

	public string GetLayersString()
	{
		return layers.Select((ApparelLayerDef layer) => layer.label).ToCommaList().CapitalizeFirst();
	}

	public BodyPartGroupDef[] GetInterferingBodyPartGroups(BodyDef body)
	{
		if (interferingBodyPartGroups == null || interferingBodyPartGroups.Length != DefDatabase<BodyDef>.DefCount)
		{
			interferingBodyPartGroups = new BodyPartGroupDef[DefDatabase<BodyDef>.DefCount][];
		}
		if (interferingBodyPartGroups[body.index] == null)
		{
			BodyPartGroupDef[] array = (from bpgd in body.AllParts.Where((BodyPartRecord part) => part.groups.Any((BodyPartGroupDef @group) => bodyPartGroups.Contains(@group))).ToArray().SelectMany((BodyPartRecord bpr) => bpr.groups)
					.Distinct()
				where apparelRelevantGroups.Contains(bpgd)
				select bpgd).ToArray();
			interferingBodyPartGroups[body.index] = array;
		}
		return interferingBodyPartGroups[body.index];
	}

	public void ResolveReferencesSpecial()
	{
		if (renderNodeProperties != null)
		{
			for (int i = 0; i < renderNodeProperties.Count; i++)
			{
				renderNodeProperties[i].ResolveReferencesRecursive();
			}
		}
	}
}
