using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld.QuestGen;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class RoyalTitleUtility
{
	private static List<TraitDef> ConceitedTraits;

	public static void FindLostAndGainedPermits(RoyalTitleDef currentTitle, RoyalTitleDef newTitle, out List<RoyalTitlePermitDef> gainedPermits, out List<RoyalTitlePermitDef> lostPermits)
	{
		gainedPermits = new List<RoyalTitlePermitDef>();
		lostPermits = new List<RoyalTitlePermitDef>();
		if (newTitle != null && newTitle.permits != null)
		{
			foreach (RoyalTitlePermitDef permit in newTitle.permits)
			{
				if (currentTitle == null || currentTitle.permits == null || !currentTitle.permits.Contains(permit))
				{
					gainedPermits.Add(permit);
				}
			}
		}
		if (currentTitle == null || currentTitle.permits == null)
		{
			return;
		}
		foreach (RoyalTitlePermitDef permit2 in currentTitle.permits)
		{
			if (newTitle == null || newTitle.permits == null || !newTitle.permits.Contains(permit2))
			{
				lostPermits.Add(permit2);
			}
		}
	}

	public static string BuildDifferenceExplanationText(RoyalTitleDef currentTitle, RoyalTitleDef newTitle, Faction faction, Pawn pawn)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = ShouldBecomeConceitedOnNewTitle(pawn);
		List<WorkTags> list = pawn.story.DisabledWorkTagsBackstoryTraitsAndGenes.GetAllSelectedItems<WorkTags>().ToList();
		List<WorkTags> obj = ((newTitle == null) ? new List<WorkTags>() : newTitle.disabledWorkTags.GetAllSelectedItems<WorkTags>().ToList());
		List<WorkTags> list2 = new List<WorkTags>();
		foreach (WorkTags item in obj)
		{
			if (!list.Contains(item))
			{
				list2.Add(item);
			}
		}
		int num = ((newTitle != null) ? faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(newTitle) : (-1));
		if (newTitle != null && flag)
		{
			stringBuilder.AppendLineTagged("LetterRoyalTitleConceitedTrait".Translate(pawn.Named("PAWN"), (from t in GetConceitedTraits(pawn)
				select t.Label).ToCommaList(useAnd: true)));
			stringBuilder.AppendLine();
			if (newTitle.minExpectation != null)
			{
				stringBuilder.AppendLineTagged("LetterRoyalTitleExpectation".Translate(pawn.Named("PAWN"), newTitle.minExpectation.label).CapitalizeFirst());
				stringBuilder.AppendLine();
			}
		}
		if (newTitle != null)
		{
			if (newTitle.canBeInherited)
			{
				Pawn heir = pawn.royalty.GetHeir(faction);
				TaggedString taggedString = ((heir != null) ? "LetterRoyalTitleHeir".Translate(pawn.Named("PAWN"), heir.Named("HEIR")) : "LetterRoyalTitleNoHeir".Translate(pawn.Named("PAWN")));
				stringBuilder.AppendTagged(taggedString);
				if (heir != null && heir.Faction != Faction.OfPlayer)
				{
					stringBuilder.AppendTagged(" " + "LetterRoyalTitleHeirFactionWarning".Translate(heir.Named("PAWN"), faction.Named("FACTION")));
				}
				stringBuilder.AppendLineTagged(" " + "LetterRoyalTitleChangingHeir".Translate(faction.Named("FACTION")));
			}
			else
			{
				stringBuilder.AppendTagged("LetterRoyalTitleCantBeInherited".Translate(newTitle.Named("TITLE")).CapitalizeFirst());
				stringBuilder.AppendTagged(" " + "LetterRoyalTitleNoHeir".Translate(pawn.Named("PAWN")));
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine();
			if (newTitle.permitPointsAwarded > 0 && pawn.royalty.NewHighestTitle(faction, newTitle))
			{
				stringBuilder.AppendLine("PermitPointsAwarded".Translate(newTitle.permitPointsAwarded));
				stringBuilder.AppendLine();
			}
		}
		if (flag && list2.Count > 0)
		{
			stringBuilder.AppendLine("LetterRoyalTitleDisabledWorkTag".Translate(pawn.Named("PAWN"), (from t in list2
				orderby FirstTitleDisablingWorkTags(t).seniority
				select t.LabelTranslated() + " (" + FirstTitleDisablingWorkTags(t).GetLabelFor(pawn) + ")").ToLineList("- ")).CapitalizeFirst());
			stringBuilder.AppendLine();
		}
		if (newTitle != null)
		{
			if ((int)newTitle.requiredMinimumApparelQuality > 0)
			{
				stringBuilder.AppendLine("LetterRoyalTitleApparelQualityRequirement".Translate(pawn.Named("PAWN"), newTitle.requiredMinimumApparelQuality.GetLabel().ToLower()).CapitalizeFirst());
				stringBuilder.AppendLine();
			}
			if (newTitle.requiredApparel != null && newTitle.requiredApparel.Count > 0)
			{
				bool flag2 = false;
				stringBuilder.AppendLine("LetterRoyalTitleApparelRequirement".Translate(pawn.Named("PAWN")).CapitalizeFirst());
				foreach (ApparelRequirement item2 in newTitle.requiredApparel)
				{
					int i = 0;
					stringBuilder.Append("- ");
					stringBuilder.Append(string.Join(", ", item2.AllRequiredApparelForPawn(pawn, ignoreGender: false, includeWorn: true).Select(delegate(ThingDef a)
					{
						string result = ((i == 0) ? a.LabelCap.Resolve() : a.label);
						i++;
						return result;
					}).ToArray()));
					if (!ApparelUtility.IsRequirementActive(item2, ApparelRequirementSource.Title, pawn, out var disabledByLabel))
					{
						stringBuilder.Append(" [" + "ApparelRequirementDisabledLabel".Translate() + ": " + disabledByLabel + "]");
					}
					else
					{
						flag2 = true;
					}
					stringBuilder.AppendLine();
				}
				if (flag2)
				{
					stringBuilder.AppendLine("- " + "ApparelRequirementAnyPrestigeArmor".Translate());
					stringBuilder.AppendLine("- " + "ApparelRequirementAnyPsycasterApparel".Translate());
					if (ModsConfig.BiotechActive)
					{
						stringBuilder.AppendLine("- " + "ApparelRequirementAnyMechlordApparel".Translate());
					}
				}
				stringBuilder.AppendLine();
			}
			if (!newTitle.throneRoomRequirements.NullOrEmpty())
			{
				stringBuilder.AppendLine("LetterRoyalTitleThroneroomRequirements".Translate(pawn.Named("PAWN"), "\n" + newTitle.throneRoomRequirements.Select((RoomRequirement r) => r.LabelCap()).ToLineList("- ")).CapitalizeFirst());
				stringBuilder.AppendLine();
			}
			if (!newTitle.GetBedroomRequirements(pawn).EnumerableNullOrEmpty())
			{
				stringBuilder.AppendLine("LetterRoyalTitleBedroomRequirements".Translate(pawn.Named("PAWN"), "\n" + (from r in newTitle.GetBedroomRequirements(pawn)
					select r.LabelCap()).ToLineList("- ")).CapitalizeFirst());
				stringBuilder.AppendLine();
			}
			if (flag && newTitle.foodRequirement.Defined && newTitle.SatisfyingMeals().Any() && (pawn.story == null || !pawn.story.traits.HasTrait(TraitDefOf.Ascetic)))
			{
				stringBuilder.AppendLine("LetterRoyalTitleFoodRequirements".Translate(pawn.Named("PAWN"), "\n" + (from m in newTitle.SatisfyingMeals(includeDrugs: false)
					select m.LabelCap.Resolve()).ToLineList("- ")).CapitalizeFirst());
				stringBuilder.AppendLine();
			}
		}
		FindLostAndGainedPermits(currentTitle, newTitle, out var _, out var lostPermits);
		if (newTitle != null && newTitle.permits != null)
		{
			stringBuilder.AppendLine("LetterRoyalTitlePermits".Translate(pawn.Named("PAWN")).CapitalizeFirst());
			foreach (RoyalTitlePermitDef item3 in newTitle.permits.OrderBy((RoyalTitlePermitDef p) => FirstTitleWithPermit(p)?.seniority))
			{
				RoyalTitleDef royalTitleDef = FirstTitleWithPermit(item3);
				if (royalTitleDef != null)
				{
					stringBuilder.AppendLine("- " + item3.LabelCap + " (" + royalTitleDef.GetLabelFor(pawn) + ")");
				}
			}
			stringBuilder.AppendLine();
		}
		if (lostPermits.Count > 0)
		{
			stringBuilder.AppendLine("LetterRoyalTitleLostPermits".Translate(pawn.Named("PAWN")).CapitalizeFirst());
			foreach (RoyalTitlePermitDef item4 in lostPermits)
			{
				stringBuilder.AppendLine("- " + item4.LabelCap);
			}
			stringBuilder.AppendLine();
		}
		if (newTitle != null)
		{
			if (newTitle.grantedAbilities.Contains(AbilityDefOf.Speech) && (currentTitle == null || !currentTitle.grantedAbilities.Contains(AbilityDefOf.Speech)))
			{
				stringBuilder.AppendLine("LetterRoyalTitleSpeechAbilityGained".Translate(pawn.Named("PAWN")).CapitalizeFirst());
				stringBuilder.AppendLine();
			}
			List<JoyKindDef> list3 = DefDatabase<JoyKindDef>.AllDefsListForReading.Where((JoyKindDef def) => def.titleRequiredAny != null && def.titleRequiredAny.Contains(newTitle)).ToList();
			if (list3.Count > 0)
			{
				stringBuilder.AppendLine("LetterRoyalTitleEnabledJoyKind".Translate(pawn.Named("PAWN")).CapitalizeFirst());
				foreach (JoyKindDef item5 in list3)
				{
					stringBuilder.AppendLine("- " + item5.LabelCap);
				}
				stringBuilder.AppendLine();
			}
			if (flag && !newTitle.disabledJoyKinds.NullOrEmpty())
			{
				stringBuilder.AppendLine("LetterRoyalTitleDisabledJoyKind".Translate(pawn.Named("PAWN")).CapitalizeFirst());
				foreach (JoyKindDef disabledJoyKind in newTitle.disabledJoyKinds)
				{
					stringBuilder.AppendLine("- " + disabledJoyKind.LabelCap);
				}
				stringBuilder.AppendLine();
			}
			if (faction.def.royalImplantRules != null)
			{
				List<RoyalImplantRule> list4 = new List<RoyalImplantRule>();
				foreach (RoyalImplantRule royalImplantRule in faction.def.royalImplantRules)
				{
					RoyalTitleDef minTitleForImplant = faction.GetMinTitleForImplant(royalImplantRule.implantHediff);
					int num2 = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(minTitleForImplant);
					if (num >= num2)
					{
						if (royalImplantRule.maxLevel == 0)
						{
							list4.Add(royalImplantRule);
						}
						else
						{
							list4.AddUnique(faction.GetMaxAllowedImplantLevel(royalImplantRule.implantHediff, newTitle));
						}
					}
				}
				if (list4.Count > 0)
				{
					stringBuilder.AppendLine("LetterRoyalTitleAllowedImplants".Translate(pawn.Named("PAWN"), "\n" + list4.Select((RoyalImplantRule royalImplantRule) => (royalImplantRule.maxLevel == 0) ? $"{royalImplantRule.implantHediff.LabelCap} ({faction.GetMinTitleForImplant(royalImplantRule.implantHediff).GetLabelFor(pawn)})" : $"{royalImplantRule.implantHediff.LabelCap}({royalImplantRule.maxLevel}x) ({royalImplantRule.minTitle.GetLabelFor(pawn)})").ToLineList("- ")).CapitalizeFirst());
					stringBuilder.AppendLine();
				}
			}
			if (currentTitle != null && newTitle.seniority < currentTitle.seniority)
			{
				List<Hediff> list5 = new List<Hediff>();
				if (pawn.health != null && pawn.health.hediffSet != null)
				{
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						if (hediff.def.HasComp(typeof(HediffComp_RoyalImplant)))
						{
							RoyalTitleDef minTitleForImplant2 = faction.GetMinTitleForImplant(hediff.def, HediffComp_RoyalImplant.GetImplantLevel(hediff));
							if (faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(minTitleForImplant2) > num)
							{
								list5.Add(hediff);
							}
						}
					}
				}
				if (list5.Count > 0)
				{
					stringBuilder.AppendLine("LetterRoyalTitleImplantsMustBeRemoved".Translate(pawn.Named("PAWN"), "\n" + list5.Select((Hediff hediff) => hediff.LabelCap).ToLineList("- ")).Resolve());
					stringBuilder.AppendLine("LetterRoyalTitleImplantGracePeriod".Translate());
					stringBuilder.AppendLine();
				}
			}
			if (pawn.royalty.NewHighestTitle(faction, newTitle) && !newTitle.rewards.NullOrEmpty())
			{
				stringBuilder.AppendLine("LetterRoyalTitleRewardGranted".Translate(pawn.Named("PAWN"), "\n" + newTitle.rewards.Select((ThingDefCountClass r) => r.Label).ToLineList("- ")).CapitalizeFirst());
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
		RoyalTitleDef FirstTitleDisablingWorkTags(WorkTags t)
		{
			return faction.def.RoyalTitlesAllInSeniorityOrderForReading.FirstOrDefault((RoyalTitleDef title) => (t & title.disabledWorkTags) != 0);
		}
		RoyalTitleDef FirstTitleWithPermit(RoyalTitlePermitDef permitDef)
		{
			return faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.FirstOrDefault((RoyalTitleDef t) => t.permits != null && t.permits.Contains(permitDef));
		}
	}

	public static RoyalTitleDef GetCurrentTitleIn(this Pawn p, Faction faction)
	{
		if (p.royalty != null)
		{
			return p.royalty.GetCurrentTitle(faction);
		}
		return null;
	}

	public static int GetCurrentTitleSeniorityIn(this Pawn p, Faction faction)
	{
		return p.GetCurrentTitleIn(faction)?.seniority ?? 0;
	}

	public static string GetTitleProgressionInfo(Faction faction, Pawn pawn = null)
	{
		TaggedString taggedString = "RoyalTitleTooltipTitlesEarnable".Translate(faction.Named("FACTION")) + ":";
		int num = 0;
		foreach (RoyalTitleDef item in faction.def.RoyalTitlesAwardableInSeniorityOrderForReading)
		{
			num += item.favorCost;
			taggedString += "\n  - " + ((pawn != null) ? item.GetLabelCapFor(pawn) : item.GetLabelCapForBothGenders()) + ": " + "RoyalTitleTooltipRoyalFavorAmount".Translate(item.favorCost, faction.def.royalFavorLabel) + " (" + "RoyalTitleTooltipRoyalFavorTotal".Translate(num.ToString()) + ")";
		}
		taggedString += "\n\n" + "RoyalTitleTooltipTitlesNonEarnable".Translate(faction.Named("FACTION")) + ":";
		foreach (RoyalTitleDef item2 in faction.def.RoyalTitlesAllInSeniorityOrderForReading.Where((RoyalTitleDef tit) => !tit.Awardable))
		{
			taggedString += "\n  - " + item2.GetLabelCapForBothGenders();
		}
		return taggedString.Resolve();
	}

	public static Building_Throne FindBestUnassignedThrone(Pawn pawn)
	{
		float num = float.PositiveInfinity;
		Building_Throne result = null;
		foreach (Thing item in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Throne))
		{
			if (item is Building_Throne building_Throne && building_Throne.CompAssignableToPawn.HasFreeSlot && building_Throne.Spawned && !building_Throne.IsForbidden(pawn) && pawn.CanReserveAndReach(building_Throne, PathEndMode.InteractionCell, pawn.NormalMaxDanger()) && RoomRoleWorker_ThroneRoom.Validate(building_Throne.GetRoom()) == null)
			{
				PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, building_Throne, TraverseParms.For(pawn), null, PathEndMode.InteractionCell);
				float num2 = (pawnPath.Found ? pawnPath.TotalCost : float.PositiveInfinity);
				pawnPath.ReleaseToPool();
				if (num > num2)
				{
					num = num2;
					result = building_Throne;
				}
			}
		}
		if (num >= float.PositiveInfinity)
		{
			return null;
		}
		return result;
	}

	public static Building_Throne FindBestUsableThrone(Pawn pawn)
	{
		Building_Throne building_Throne = pawn.ownership.AssignedThrone;
		if (building_Throne != null)
		{
			if (!building_Throne.Spawned || building_Throne.IsForbidden(pawn) || !pawn.CanReserveAndReach(building_Throne, PathEndMode.InteractionCell, pawn.NormalMaxDanger()))
			{
				return null;
			}
			if (RoomRoleWorker_ThroneRoom.Validate(building_Throne.GetRoom()) != null)
			{
				return null;
			}
		}
		else
		{
			building_Throne = FindBestUnassignedThrone(pawn);
			if (building_Throne == null)
			{
				return null;
			}
		}
		return building_Throne;
	}

	public static bool BedroomSatisfiesRequirements(Room room, RoyalTitle title)
	{
		foreach (RoomRequirement bedroomRequirement in title.def.bedroomRequirements)
		{
			if (!bedroomRequirement.MetOrDisabled(room))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsPawnConceited(Pawn p)
	{
		TraitSet traitSet = p.story?.traits;
		if (traitSet != null && traitSet.HasTrait(TraitDefOf.Ascetic))
		{
			return false;
		}
		if (p.Faction.IsPlayer && !p.IsQuestLodger())
		{
			if (traitSet != null)
			{
				if (!traitSet.HasTrait(TraitDefOf.Abrasive) && !traitSet.HasTrait(TraitDefOf.Greedy))
				{
					return traitSet.HasTrait(TraitDefOf.Jealous);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static IEnumerable<Trait> GetConceitedTraits(Pawn p)
	{
		TraitSet traits = p.story?.traits;
		if (traits == null)
		{
			yield break;
		}
		for (int i = 0; i < ConceitedTraits.Count; i++)
		{
			Trait trait = traits.GetTrait(ConceitedTraits[i]);
			if (trait != null)
			{
				yield return trait;
			}
		}
	}

	public static IEnumerable<Trait> GetTraitsAffectingPsylinkNegatively(Pawn p)
	{
		if (p.story == null || p.story.traits == null || p.story.traits.allTraits.NullOrEmpty())
		{
			yield break;
		}
		foreach (Trait allTrait in p.story.traits.allTraits)
		{
			if (!allTrait.Suppressed)
			{
				TraitDegreeData traitDegreeData = allTrait.def.DataAtDegree(allTrait.Degree);
				if ((traitDegreeData.statFactors != null && traitDegreeData.statFactors.Any((StatModifier f) => f.stat == StatDefOf.PsychicSensitivity && f.value < 1f)) || (traitDegreeData.statOffsets != null && traitDegreeData.statOffsets.Any((StatModifier f) => f.stat == StatDefOf.PsychicSensitivity && f.value < 0f)))
				{
					yield return allTrait;
				}
			}
		}
	}

	public static TaggedString GetPsylinkAffectedByTraitsNegativelyWarning(Pawn p)
	{
		if (p.HasPsylink || !GetTraitsAffectingPsylinkNegatively(p).Any())
		{
			return null;
		}
		return "RoyalWithTraitAffectingPsylinkNegatively".Translate(p.Named("PAWN"), p.Faction.Named("FACTION"), (from t in GetTraitsAffectingPsylinkNegatively(p)
			select t.Label).ToCommaList(useAnd: true));
	}

	public static bool ShouldBecomeConceitedOnNewTitle(Pawn p)
	{
		TraitSet traitSet = p.story?.traits;
		if (traitSet != null && traitSet.HasTrait(TraitDefOf.Ascetic))
		{
			return false;
		}
		if (p.Faction != null && p.Faction.IsPlayer && !p.IsQuestLodger())
		{
			return GetConceitedTraits(p).Any();
		}
		return true;
	}

	public static Quest GetCurrentBestowingCeremonyQuest(Pawn pawn, Faction faction)
	{
		foreach (Quest item in Find.QuestManager.QuestsListForReading)
		{
			if (!item.Historical)
			{
				QuestPart_BestowingCeremony questPart_BestowingCeremony = (QuestPart_BestowingCeremony)item.PartsListForReading.FirstOrDefault((QuestPart p) => p is QuestPart_BestowingCeremony);
				if (questPart_BestowingCeremony != null && questPart_BestowingCeremony.target == pawn && questPart_BestowingCeremony.bestower != null && questPart_BestowingCeremony.bestower.Faction == faction)
				{
					return item;
				}
			}
		}
		return null;
	}

	public static bool ShouldGetBestowingCeremonyQuest(Pawn pawn, out Faction faction)
	{
		faction = null;
		if (pawn.IsMutant && pawn.mutant.Def.disableTitles)
		{
			return false;
		}
		if (pawn.Faction != null && pawn.Faction.IsPlayer && pawn.royalty != null && pawn.royalty.CanUpdateTitleOfAnyFaction(out faction))
		{
			return GetCurrentBestowingCeremonyQuest(pawn, faction) == null;
		}
		return false;
	}

	public static bool ShouldGetBestowingCeremonyQuest(Pawn pawn, Faction faction)
	{
		if (pawn.IsMutant && pawn.mutant.Def.disableTitles)
		{
			return false;
		}
		if (pawn.Faction != null && pawn.Faction.IsPlayer && pawn.royalty != null && pawn.royalty.CanUpdateTitle(faction))
		{
			return GetCurrentBestowingCeremonyQuest(pawn, faction) == null;
		}
		return false;
	}

	public static void EndExistingBestowingCeremonyQuest(Pawn pawn, Faction faction)
	{
		foreach (Quest item in Find.QuestManager.QuestsListForReading)
		{
			if (!item.Historical && item.State != QuestState.Ongoing)
			{
				QuestPart_BestowingCeremony questPart_BestowingCeremony = (QuestPart_BestowingCeremony)item.PartsListForReading.FirstOrDefault((QuestPart p) => p is QuestPart_BestowingCeremony);
				if (questPart_BestowingCeremony != null && questPart_BestowingCeremony.target == pawn && questPart_BestowingCeremony.bestower.Faction == faction)
				{
					item.End(QuestEndOutcome.InvalidPreAcceptance, sendLetter: false);
				}
			}
		}
	}

	public static void GenerateBestowingCeremonyQuest(Pawn pawn, Faction faction)
	{
		if (pawn == null || pawn.Dead)
		{
			return;
		}
		Slate slate = new Slate();
		slate.Set("titleHolder", pawn);
		slate.Set("bestowingFaction", faction);
		if (QuestScriptDefOf.BestowingCeremony.CanRun(slate, pawn.MapHeld))
		{
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.BestowingCeremony, slate);
			if (quest.root.sendAvailableLetter)
			{
				QuestUtility.SendLetterQuestAvailable(quest);
			}
		}
	}

	public static void ResetStaticData()
	{
		ConceitedTraits = new List<TraitDef>
		{
			TraitDefOf.Abrasive,
			TraitDefOf.Greedy,
			TraitDefOf.Jealous
		};
	}

	public static void DoTable_IngestibleMaxSatisfiedTitle()
	{
		List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
		list.Add(new TableDataGetter<ThingDef>("name", (ThingDef f) => f.LabelCap));
		list.Add(new TableDataGetter<ThingDef>("max satisfied title", delegate(ThingDef t)
		{
			RoyalTitleDef royalTitleDef = t.ingestible.MaxSatisfiedTitle();
			return (royalTitleDef == null) ? "-" : ((string)royalTitleDef.LabelCap);
		}));
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef t) => t.ingestible != null && !t.IsCorpse && t.ingestible.HumanEdible), list.ToArray());
	}
}
