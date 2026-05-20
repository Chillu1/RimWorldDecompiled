using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RoyalTitleDef : Def
{
	public int seniority;

	public int favorCost;

	[MustTranslate]
	public string labelFemale;

	public int changeHeirQuestPoints = -1;

	public float commonality = 1f;

	public WorkTags disabledWorkTags;

	public Type inheritanceWorkerOverrideClass;

	public QualityCategory requiredMinimumApparelQuality;

	public List<ApparelRequirement> requiredApparel;

	public List<RoyalTitlePermitDef> permits;

	public ExpectationDef minExpectation;

	public List<JoyKindDef> disabledJoyKinds;

	[NoTranslate]
	public List<string> tags;

	public List<ThingDefCountClass> rewards;

	public bool suppressIdleAlert;

	public bool canBeInherited;

	public bool allowDignifiedMeditationFocus = true;

	public int permitPointsAwarded;

	public Type awardWorkerClass = typeof(RoyalTitleAwardWorker);

	public ThoughtDef awardThought;

	public ThoughtDef lostThought;

	public List<RoomRequirement> throneRoomRequirements;

	public List<RoomRequirement> bedroomRequirements;

	public float recruitmentResistanceOffset;

	public RoyalTitleFoodRequirement foodRequirement;

	public RoyalTitleDef replaceOnRecruited;

	public float decreeMtbDays = -1f;

	public float decreeMinIntervalDays = 2f;

	public float decreeMentalBreakCommonality;

	public List<string> decreeTags;

	public List<AbilityDef> grantedAbilities = new List<AbilityDef>();

	public IntRange speechCooldown;

	public int maxPsylinkLevel;

	[Unsaved(false)]
	private List<ThingDef> satisfyingMealsCached;

	[Unsaved(false)]
	private List<ThingDef> satisfyingMealsNoDrugsCached;

	private RoyalTitleAwardWorker awardWorker;

	private RoyalTitleInheritanceWorker inheritanceWorkerOverride;

	public bool Awardable => favorCost > 0;

	public IEnumerable<WorkTypeDef> DisabledWorkTypes
	{
		get
		{
			List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < list.Count; i++)
			{
				if ((disabledWorkTags & list[i].workTags) != WorkTags.None)
				{
					yield return list[i];
				}
			}
		}
	}

	public RoyalTitleInheritanceWorker InheritanceWorkerOverride
	{
		get
		{
			if (inheritanceWorkerOverride == null && inheritanceWorkerOverrideClass != null)
			{
				inheritanceWorkerOverride = (RoyalTitleInheritanceWorker)Activator.CreateInstance(inheritanceWorkerOverrideClass);
			}
			return inheritanceWorkerOverride;
		}
	}

	public float MinThroneRoomImpressiveness
	{
		get
		{
			if (throneRoomRequirements.NullOrEmpty())
			{
				return 0f;
			}
			RoomRequirement_Impressiveness roomRequirement_Impressiveness = throneRoomRequirements.OfType<RoomRequirement_Impressiveness>().FirstOrDefault();
			if (roomRequirement_Impressiveness == null)
			{
				return 0f;
			}
			return roomRequirement_Impressiveness.impressiveness;
		}
	}

	public RoyalTitleAwardWorker AwardWorker
	{
		get
		{
			if (awardWorker == null)
			{
				awardWorker = (RoyalTitleAwardWorker)Activator.CreateInstance(awardWorkerClass);
				awardWorker.def = this;
			}
			return awardWorker;
		}
	}

	public RoyalTitleInheritanceWorker GetInheritanceWorker(Faction faction)
	{
		if (inheritanceWorkerOverrideClass == null)
		{
			return faction.def.RoyalTitleInheritanceWorker;
		}
		return InheritanceWorkerOverride;
	}

	public string GetLabelFor(Pawn p)
	{
		if (p == null)
		{
			return GetLabelForBothGenders();
		}
		return GetLabelFor(p.gender);
	}

	public string GetLabelFor(Gender g)
	{
		if (g == Gender.Female)
		{
			if (string.IsNullOrEmpty(labelFemale))
			{
				return label;
			}
			return labelFemale;
		}
		return label;
	}

	public string GetLabelForBothGenders()
	{
		if (!string.IsNullOrEmpty(labelFemale))
		{
			return label + " / " + labelFemale;
		}
		return label;
	}

	public string GetLabelCapForBothGenders()
	{
		if (!string.IsNullOrEmpty(labelFemale))
		{
			return LabelCap + " / " + labelFemale.CapitalizeFirst();
		}
		return LabelCap;
	}

	public string GetLabelCapFor(Pawn p)
	{
		return GetLabelFor(p).CapitalizeFirst(this);
	}

	public IEnumerable<RoomRequirement> GetBedroomRequirements(Pawn p)
	{
		if (p.story.traits.HasTrait(TraitDefOf.Ascetic))
		{
			return null;
		}
		return bedroomRequirements;
	}

	public string GetReportText(Faction faction)
	{
		return description + "\n\n" + RoyalTitleUtility.GetTitleProgressionInfo(faction);
	}

	public bool JoyKindDisabled(JoyKindDef joyKind)
	{
		if (disabledJoyKinds == null)
		{
			return false;
		}
		return disabledJoyKinds.Contains(joyKind);
	}

	private bool HasSameRoomRequirement(RoomRequirement otherReq, List<RoomRequirement> list)
	{
		if (list == null)
		{
			return false;
		}
		foreach (RoomRequirement item in list)
		{
			if (item.SameOrSubsetOf(otherReq))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSameThroneroomRequirement(RoomRequirement otherReq)
	{
		return HasSameRoomRequirement(otherReq, throneRoomRequirements);
	}

	public bool HasSameBedroomRequirement(RoomRequirement otherReq)
	{
		return HasSameRoomRequirement(otherReq, bedroomRequirements);
	}

	public int MaxAllowedPsylinkLevel(FactionDef faction)
	{
		int result = 0;
		for (int i = 0; i < faction.royalImplantRules.Count; i++)
		{
			RoyalImplantRule royalImplantRule = faction.royalImplantRules[i];
			if (royalImplantRule.implantHediff == HediffDefOf.PsychicAmplifier && royalImplantRule.minTitle.Awardable && royalImplantRule.minTitle.seniority <= seniority)
			{
				result = royalImplantRule.maxLevel;
			}
		}
		return result;
	}

	public IEnumerable<ThingDef> SatisfyingMeals(bool includeDrugs = true)
	{
		if (includeDrugs)
		{
			if (satisfyingMealsCached == null)
			{
				satisfyingMealsCached = (from t in DefDatabase<ThingDef>.AllDefsListForReading
					where foodRequirement.Acceptable(t)
					orderby t.ingestible.preferability descending
					select t).ToList();
			}
		}
		else if (satisfyingMealsNoDrugsCached == null)
		{
			satisfyingMealsNoDrugsCached = (from t in DefDatabase<ThingDef>.AllDefsListForReading
				where foodRequirement.Acceptable(t) && !t.IsDrug
				orderby t.ingestible.preferability descending
				select t).ToList();
		}
		if (!includeDrugs)
		{
			return satisfyingMealsNoDrugsCached;
		}
		return satisfyingMealsCached;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		if (!permits.NullOrEmpty())
		{
			TaggedString taggedString = "RoyalTitleTooltipPermits".Translate();
			string valueString = permits.Select((RoyalTitlePermitDef r) => r.label).ToCommaList().CapitalizeFirst();
			string reportText = permits.Select((RoyalTitlePermitDef r) => r.LabelCap.ToString()).ToLineList("  - ", capitalizeItems: true);
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, taggedString, valueString, reportText, 99999);
		}
		if ((int)requiredMinimumApparelQuality > 0)
		{
			TaggedString taggedString2 = "RoyalTitleTooltipRequiredApparelQuality".Translate();
			string text = requiredMinimumApparelQuality.GetLabel().CapitalizeFirst();
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, taggedString2, text, text, 99998);
		}
		if (!requiredApparel.NullOrEmpty())
		{
			TaggedString taggedString3 = "RoyalTitleTooltipRequiredApparel".Translate();
			TaggedString taggedString4 = "Male".Translate().CapitalizeFirst() + ":\n" + RequiredApparelListForGender(Gender.Male, req.Pawn).ToLineList("  - ") + "\n\n" + "Female".Translate().CapitalizeFirst() + ":\n" + RequiredApparelListForGender(Gender.Female, req.Pawn).ToLineList("  - ");
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, taggedString3, "", "RoyalTitleRequiredApparelStatDescription".Translate() + ":\n\n" + taggedString4, 99998);
		}
		if (!bedroomRequirements.NullOrEmpty())
		{
			TaggedString taggedString5 = "RoyalTitleTooltipBedroomRequirements".Translate();
			string valueString2 = bedroomRequirements.Select((RoomRequirement r) => r.Label()).ToCommaList().CapitalizeFirst();
			string reportText2 = bedroomRequirements.Select((RoomRequirement r) => r.LabelCap()).ToLineList("  - ");
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, taggedString5, valueString2, reportText2, 99997);
		}
		if (!throneRoomRequirements.NullOrEmpty())
		{
			TaggedString taggedString6 = "RoyalTitleTooltipThroneroomRequirements".Translate();
			string valueString3 = throneRoomRequirements.Select((RoomRequirement r) => r.Label()).ToCommaList().CapitalizeFirst();
			string reportText3 = throneRoomRequirements.Select((RoomRequirement r) => r.LabelCap()).ToArray().ToLineList("  - ");
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, taggedString6, valueString3, reportText3, 99997);
		}
		IEnumerable<string> enumerable = from w in disabledWorkTags.GetAllSelectedItems<WorkTags>()
			where w != WorkTags.None
			select w.LabelTranslated();
		if (enumerable.Any())
		{
			TaggedString taggedString7 = "DisabledWorkTypes".Translate();
			string valueString4 = enumerable.ToCommaList().CapitalizeFirst();
			string reportText4 = enumerable.ToLineList(" -  ", capitalizeItems: true);
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, taggedString7, valueString4, reportText4, 99994);
		}
		if (foodRequirement.Defined && SatisfyingMeals().Any())
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "RoyalTitleRequiredMeals".Translate(), (from m in SatisfyingMeals()
				select m.label).ToCommaList().CapitalizeFirst(), "RoyalTitleRequiredMealsDesc".Translate(), 99995);
		}
	}

	private IEnumerable<string> RequiredApparelListForGender(Gender g, Pawn forPawn = null)
	{
		bool anyRequirementValid = false;
		foreach (ApparelRequirement item in requiredApparel)
		{
			string appendDisabledRequirement = null;
			if (forPawn != null && !ApparelUtility.IsRequirementActive(item, ApparelRequirementSource.Title, forPawn, out var disabledByLabel))
			{
				appendDisabledRequirement = " [" + "ApparelRequirementDisabledLabel".Translate() + ": " + disabledByLabel + "]";
			}
			else
			{
				anyRequirementValid = true;
			}
			foreach (TaggedString item2 in from a in item.AllRequiredApparel(g).Distinct()
				select a.LabelCap)
			{
				if (appendDisabledRequirement == null)
				{
					yield return item2;
				}
				else
				{
					yield return item2 + " " + appendDisabledRequirement;
				}
			}
		}
		if (anyRequirementValid)
		{
			yield return "ApparelRequirementAnyPrestigeArmor".Translate();
			yield return "ApparelRequirementAnyPsycasterApparel".Translate();
			if (ModsConfig.BiotechActive)
			{
				yield return "ApparelRequirementAnyMechlordApparel".Translate();
			}
		}
	}

	public IEnumerable<DefHyperlink> GetHyperlinks(Faction faction)
	{
		IEnumerable<DefHyperlink> enumerable = descriptionHyperlinks;
		return enumerable ?? (from t in faction.def.RoyalTitlesAllInSeniorityOrderForReading
			where t != this
			select new DefHyperlink(t, faction));
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		ModLister.CheckRoyalty("Royal title");
		if (awardThought != null && !typeof(Thought_MemoryRoyalTitle).IsAssignableFrom(awardThought.thoughtClass))
		{
			yield return $"Royal title {defName} has awardThought with thoughtClass {awardThought.thoughtClass.FullName} which is not deriving from Thought_MemoryRoyalTitle!";
		}
		if (lostThought != null && !typeof(Thought_MemoryRoyalTitle).IsAssignableFrom(lostThought.thoughtClass))
		{
			yield return $"Royal title {defName} has awardThought with thoughtClass {awardThought.thoughtClass.FullName} which is not deriving from Thought_MemoryRoyalTitle!";
		}
		if (disabledJoyKinds != null)
		{
			foreach (JoyKindDef disabledJoyKind in disabledJoyKinds)
			{
				if (disabledJoyKind.titleRequiredAny != null && disabledJoyKind.titleRequiredAny.Contains(this))
				{
					yield return $"Royal title {defName} disables joy kind {disabledJoyKind.defName} which requires the title!";
				}
			}
		}
		if (!typeof(RoyalTitleAwardWorker).IsAssignableFrom(awardWorkerClass))
		{
			yield return "awardWorkerClass does not derive from RoyalTitleAwardWorker";
		}
		if (Awardable && changeHeirQuestPoints < 0)
		{
			yield return "undefined changeHeirQuestPoints, it's required for awardable titles";
		}
		if (throneRoomRequirements.NullOrEmpty())
		{
			yield break;
		}
		foreach (RoomRequirement req in throneRoomRequirements)
		{
			foreach (string item2 in req.ConfigErrors())
			{
				yield return $"Room requirement {req.GetType().Name}: {item2}";
			}
		}
	}
}
