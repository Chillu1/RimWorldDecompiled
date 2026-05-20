using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class StartingPawnUtility
{
	private const float ChanceToHavePossessionsFromBackground = 0.25f;

	private const float ChanceToHavePossessionsFromTraits = 0.25f;

	private static readonly IntRange BabyFoodCountRange = new IntRange(30, 40);

	private static readonly IntRange HemogenCountRange = new IntRange(8, 12);

	private static readonly FloatRange ExcludeBiologicalAgeRange = new FloatRange(12.1f, 13f);

	private const float RightRectLeftPadding = 5f;

	public static readonly Vector2 PawnPortraitSize = new Vector2(92f, 128f);

	private const int SkillSummaryColumns = 4;

	private static List<PawnGenerationRequest> StartingAndOptionalPawnGenerationRequests = new List<PawnGenerationRequest>();

	private static float listScrollViewHeight = 0f;

	private static Vector2 listScrollPosition;

	private static int SkillsPerColumn = -1;

	private const int MaxPossessionsCount = 2;

	private static readonly FloatRange DaysSatisfied = new FloatRange(25f, 35f);

	private const float ChanceForRandomPossession = 0.06f;

	private static Dictionary<Pawn, List<ThingDefCount>> StartingPossessions => Find.GameInitData.startingPossessions;

	private static List<Pawn> StartingAndOptionalPawns => Find.GameInitData.startingAndOptionalPawns;

	private static PawnGenerationRequest DefaultStartingPawnRequest
	{
		get
		{
			PawnKindDef kind = Find.GameInitData.startingPawnKind ?? Faction.OfPlayer.def.basicMemberKind;
			Faction ofPlayer = Faction.OfPlayer;
			bool tutorialMode = TutorSystem.TutorialMode;
			XenotypeDef forcedXenotype = (ModsConfig.BiotechActive ? XenotypeDefOf.Baseliner : null);
			FloatRange? excludeBiologicalAgeRange = (ModsConfig.BiotechActive ? new FloatRange?(ExcludeBiologicalAgeRange) : ((FloatRange?)null));
			return new PawnGenerationRequest(kind, ofPlayer, PawnGenerationContext.PlayerStarter, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, tutorialMode, 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, forcedXenotype, null, null, 0f, DevelopmentalStage.Adult, null, excludeBiologicalAgeRange);
		}
	}

	public static void DrawPortraitArea(Rect rect, int pawnIndex, bool renderClothes, bool renderHeadgear)
	{
		Pawn pawn = StartingAndOptionalPawns[pawnIndex];
		Widgets.DrawMenuSection(rect);
		rect = rect.ContractedBy(17f);
		Rect position = new Rect(rect.center.x - PawnPortraitSize.x / 2f, rect.yMin - 24f, PawnPortraitSize.x, PawnPortraitSize.y);
		Pawn pawn2 = pawn;
		Vector2 pawnPortraitSize = PawnPortraitSize;
		Rot4 south = Rot4.South;
		bool renderClothes2 = renderClothes;
		bool renderHeadgear2 = renderHeadgear;
		RenderTexture image = PortraitsCache.Get(pawn2, pawnPortraitSize, south, default(Vector3), 1f, supersample: true, compensateForUIScale: true, renderHeadgear2, renderClothes2, null, null, stylingStation: true);
		GUI.DrawTexture(position, image);
		Rect rect2 = rect;
		rect2.width = 500f;
		CharacterCardUtility.DrawCharacterCard(rect2, pawn, delegate
		{
			RandomizePawn(pawnIndex);
		}, rect);
		pawn = StartingAndOptionalPawns[pawnIndex];
		bool num = SocialCardUtility.AnyRelations(pawn);
		List<ThingDefCount> list = StartingPossessions[pawn];
		bool flag = list.Any();
		int num2 = 1;
		if (num)
		{
			num2++;
		}
		if (flag)
		{
			num2++;
		}
		float height = (rect.height - 100f - (4f * (float)num2 - 1f)) / (float)num2;
		float y = rect.y;
		Rect rect3 = rect;
		rect3.yMin += 100f;
		rect3.xMin = rect2.xMax + 5f;
		rect3.height = height;
		if (!HealthCardUtility.AnyHediffsDisplayed(pawn, showBloodLoss: true))
		{
			GUI.color = Color.gray;
		}
		Widgets.Label(rect3, "Health".Translate().AsTipTitle());
		GUI.color = Color.white;
		rect3.yMin += 35f;
		HealthCardUtility.DrawHediffListing(rect3, pawn, showBloodLoss: true);
		y = rect3.yMax + 4f;
		if (num)
		{
			Rect rect4 = new Rect(rect3.x, y, rect3.width, height);
			Widgets.Label(rect4, "Relations".Translate().AsTipTitle());
			rect4.yMin += 35f;
			SocialCardUtility.DrawRelationsAndOpinions(rect4, pawn);
			y = rect4.yMax + 4f;
		}
		if (flag)
		{
			Rect rect5 = new Rect(rect3.x, y, rect3.width, height);
			Widgets.Label(rect5, "Possessions".Translate().AsTipTitle());
			rect5.yMin += 35f;
			DrawPossessions(rect5, pawn, list);
		}
	}

	private static void DrawPossessions(Rect rect, Pawn selPawn, List<ThingDefCount> possessions)
	{
		GUI.BeginGroup(rect);
		Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
		Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listScrollViewHeight);
		Rect rect2 = rect;
		if (viewRect.height > outRect.height)
		{
			rect2.width -= 16f;
		}
		Widgets.BeginScrollView(outRect, ref listScrollPosition, viewRect);
		float num = 0f;
		if (StartingPossessions.ContainsKey(selPawn))
		{
			for (int i = 0; i < possessions.Count; i++)
			{
				ThingDefCount thingDefCount = possessions[i];
				Rect rect3 = new Rect(0f, num, Text.LineHeight, Text.LineHeight);
				Widgets.DefIcon(rect3, thingDefCount.ThingDef);
				Rect rect4 = new Rect(rect3.xMax + 17f, num, rect.width - rect3.width - 17f - 24f, Text.LineHeight);
				Widgets.Label(rect4, thingDefCount.LabelCap);
				if (Mouse.IsOver(rect4))
				{
					Widgets.DrawHighlight(rect4);
					TooltipHandler.TipRegion(rect4, thingDefCount.ThingDef.LabelCap.ToString().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + thingDefCount.ThingDef.description);
				}
				Widgets.InfoCardButton(rect4.xMax, num, thingDefCount.ThingDef);
				num += Text.LineHeight;
			}
		}
		if (Event.current.type == EventType.Layout)
		{
			listScrollViewHeight = num;
		}
		Widgets.EndScrollView();
		GUI.EndGroup();
	}

	public static void DrawSkillSummaries(Rect rect)
	{
		Widgets.DrawMenuSection(rect);
		rect = rect.ContractedBy(10f);
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(rect.min, new Vector2(rect.width, 45f)), "TeamSkills".Translate());
		Text.Font = GameFont.Small;
		rect.yMin += 45f;
		rect = rect.LeftPart(0.25f);
		rect.height = 27f;
		rect.y -= 4f;
		List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
		if (SkillsPerColumn < 0)
		{
			SkillsPerColumn = Mathf.CeilToInt((float)allDefsListForReading.Count((SkillDef sd) => sd.pawnCreatorSummaryVisible) / 4f);
		}
		int num = 0;
		for (int num2 = 0; num2 < allDefsListForReading.Count; num2++)
		{
			SkillDef skillDef = allDefsListForReading[num2];
			if (skillDef.pawnCreatorSummaryVisible)
			{
				Rect r = rect;
				r.x = rect.x + rect.width * (float)(num / SkillsPerColumn);
				r.y = rect.y + rect.height * (float)(num % SkillsPerColumn);
				r.height = 24f;
				r.width -= 4f;
				Pawn pawn = FindBestSkillOwner(skillDef);
				SkillUI.DrawSkill(pawn.skills.GetSkill(skillDef), r.Rounded(), SkillUI.SkillDrawMode.Menu, pawn.Name.ToString().Colorize(ColoredText.TipSectionTitleColor));
				num++;
			}
		}
	}

	private static Pawn FindBestSkillOwner(SkillDef skill)
	{
		Pawn pawn = Find.GameInitData.startingAndOptionalPawns[0];
		SkillRecord skillRecord = pawn.skills.GetSkill(skill);
		for (int i = 1; i < Find.GameInitData.startingPawnCount; i++)
		{
			SkillRecord skill2 = Find.GameInitData.startingAndOptionalPawns[i].skills.GetSkill(skill);
			if (!skill2.TotallyDisabled && (skillRecord.TotallyDisabled || skill2.Level > skillRecord.Level || (skill2.Level == skillRecord.Level && (int)skill2.passion > (int)skillRecord.passion)))
			{
				pawn = Find.GameInitData.startingAndOptionalPawns[i];
				skillRecord = skill2;
			}
		}
		return pawn;
	}

	public static void RandomizePawn(int pawnIndex)
	{
		if (TutorSystem.AllowAction("RandomizePawn"))
		{
			int num = 0;
			do
			{
				Pawn pawn = StartingAndOptionalPawns[pawnIndex];
				SpouseRelationUtility.Notify_PawnRegenerated(pawn);
				RandomizeInPlace(pawn);
				num++;
			}
			while (num <= 20 && !WorkTypeRequirementsSatisfied());
			TutorSystem.Notify_Event("RandomizePawn");
		}
	}

	public static void ClearAllStartingPawns()
	{
		for (int num = StartingAndOptionalPawns.Count - 1; num >= 0; num--)
		{
			StartingAndOptionalPawns[num].relations.ClearAllRelations();
			if (Find.World != null)
			{
				PawnUtility.DestroyStartingColonistFamily(StartingAndOptionalPawns[num]);
				PawnComponentsUtility.RemoveComponentsOnDespawned(StartingAndOptionalPawns[num]);
				Find.WorldPawns.PassToWorld(StartingAndOptionalPawns[num], PawnDiscardDecideMode.Discard);
			}
			StartingPossessions.Remove(StartingAndOptionalPawns[num]);
			StartingAndOptionalPawns.RemoveAt(num);
		}
		StartingAndOptionalPawnGenerationRequests.Clear();
	}

	public static Pawn RandomizeInPlace(Pawn p)
	{
		return RegenerateStartingPawnInPlace(StartingAndOptionalPawns.IndexOf(p));
	}

	private static Pawn RegenerateStartingPawnInPlace(int index)
	{
		bool num = Current.ProgramState != ProgramState.Entry;
		Pawn pawn = StartingAndOptionalPawns[index];
		if (!num)
		{
			PawnUtility.TryDestroyStartingColonistFamily(pawn);
		}
		pawn.relations.ClearAllRelations();
		PawnComponentsUtility.RemoveComponentsOnDespawned(pawn);
		Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
		StartingPossessions.Remove(pawn);
		StartingAndOptionalPawns[index] = null;
		if (!num)
		{
			for (int i = 0; i < StartingAndOptionalPawns.Count; i++)
			{
				if (StartingAndOptionalPawns[i] != null)
				{
					PawnUtility.TryDestroyStartingColonistFamily(StartingAndOptionalPawns[i]);
				}
			}
		}
		Pawn pawn2 = NewGeneratedStartingPawn(index);
		StartingAndOptionalPawns[index] = pawn2;
		return pawn2;
	}

	public static PawnGenerationRequest GetGenerationRequest(int index)
	{
		EnsureGenerationRequestInRangeOf(index);
		return StartingAndOptionalPawnGenerationRequests[index];
	}

	public static void SetGenerationRequest(int index, PawnGenerationRequest request)
	{
		EnsureGenerationRequestInRangeOf(index);
		StartingAndOptionalPawnGenerationRequests[index] = request;
	}

	public static void ReorderRequests(int from, int to)
	{
		EnsureGenerationRequestInRangeOf((from > to) ? from : to);
		PawnGenerationRequest generationRequest = GetGenerationRequest(from);
		StartingAndOptionalPawnGenerationRequests.Insert(to, generationRequest);
		StartingAndOptionalPawnGenerationRequests.RemoveAt((from < to) ? from : (from + 1));
	}

	private static void EnsureGenerationRequestInRangeOf(int index)
	{
		while (StartingAndOptionalPawnGenerationRequests.Count <= index)
		{
			StartingAndOptionalPawnGenerationRequests.Add(DefaultStartingPawnRequest);
		}
	}

	public static int PawnIndex(Pawn pawn)
	{
		return Mathf.Max(StartingAndOptionalPawns.IndexOf(pawn), 0);
	}

	public static Pawn NewGeneratedStartingPawn(int index = -1)
	{
		PawnGenerationRequest request = ((index < 0) ? DefaultStartingPawnRequest : GetGenerationRequest(index));
		Pawn pawn = null;
		try
		{
			pawn = PawnGenerator.GeneratePawn(request);
		}
		catch (Exception ex)
		{
			Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + ex);
			pawn = PawnGenerator.GeneratePawn(request);
		}
		pawn.relations.everSeenByPlayer = true;
		PawnComponentsUtility.AddComponentsForSpawn(pawn);
		GeneratePossessions(pawn);
		return pawn;
	}

	public static void GeneratePossessions(Pawn pawn)
	{
		if (!StartingPossessions.ContainsKey(pawn))
		{
			StartingPossessions.Add(pawn, new List<ThingDefCount>());
		}
		else
		{
			StartingPossessions[pawn].Clear();
		}
		if (Find.Scenario.AllParts.Any((ScenPart x) => x is ScenPart_NoPossessions) || pawn.IsSubhuman)
		{
			return;
		}
		if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Baby())
		{
			StartingPossessions[pawn].Add(new ThingDefCount(ThingDefOf.BabyFood, BabyFoodCountRange.RandomInRange));
			return;
		}
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			if (StartingPossessions[pawn].Count >= 2)
			{
				return;
			}
			if (hediff is Hediff_Addiction { Need: var need } hediff_Addiction)
			{
				ThingDef thingDef = GetDrugFor(hediff_Addiction.Chemical);
				if (need != null && thingDef != null)
				{
					int count = GenMath.RoundRandom(need.def.fallPerDay * DaysSatisfied.RandomInRange / thingDef.GetCompProperties<CompProperties_Drug>().needLevelOffset);
					StartingPossessions[pawn].Add(new ThingDefCount(thingDef, count));
				}
			}
		}
		if (ModsConfig.BiotechActive)
		{
			foreach (Hediff hediff2 in pawn.health.hediffSet.hediffs)
			{
				if (StartingPossessions[pawn].Count >= 2)
				{
					return;
				}
				if (hediff2 is Hediff_ChemicalDependency { LinkedGene: not null } hediff_ChemicalDependency && hediff_ChemicalDependency.LinkedGene.Active)
				{
					ThingDef thingDef2 = GetDrugFor(hediff_ChemicalDependency.chemical);
					if (thingDef2 != null)
					{
						float num = hediff_ChemicalDependency.def.CompProps<HediffCompProperties_SeverityPerDay>()?.severityPerDay ?? 1f;
						StartingPossessions[pawn].Add(new ThingDefCount(thingDef2, GenMath.RoundRandom(DaysSatisfied.RandomInRange * num)));
					}
				}
			}
		}
		if (StartingPossessions[pawn].Count >= 2)
		{
			return;
		}
		if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Hemogenic))
		{
			StartingPossessions[pawn].Add(new ThingDefCount(ThingDefOf.HemogenPack, HemogenCountRange.RandomInRange));
			if (StartingPossessions[pawn].Count >= 2)
			{
				return;
			}
		}
		if (Rand.Value < 0.25f)
		{
			BackstoryDef backstory = pawn.story.GetBackstory(BackstorySlot.Adulthood);
			if (backstory != null)
			{
				foreach (PossessionThingDefCountClass possession in backstory.possessions)
				{
					if (StartingPossessions[pawn].Count >= 2)
					{
						return;
					}
					StartingPossessions[pawn].Add(new ThingDefCount(possession.key, Mathf.Clamp(possession.value.RandomInRange, 1, possession.key.stackLimit)));
				}
			}
		}
		if (StartingPossessions[pawn].Count >= 2)
		{
			return;
		}
		if (Rand.Value < 0.25f)
		{
			foreach (Trait allTrait in pawn.story.traits.allTraits)
			{
				if (allTrait.Suppressed)
				{
					continue;
				}
				foreach (PossessionThingDefCountClass possession2 in allTrait.CurrentData.possessions)
				{
					if (StartingPossessions[pawn].Count >= 2)
					{
						return;
					}
					StartingPossessions[pawn].Add(new ThingDefCount(possession2.key, Mathf.Clamp(possession2.value.RandomInRange, 1, possession2.key.stackLimit)));
				}
			}
		}
		if (StartingPossessions[pawn].Count < 2 && Rand.Value < 0.06f && DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.possessionCount > 0).TryRandomElement(out var result))
		{
			StartingPossessions[pawn].Add(new ThingDefCount(result, Mathf.Min(result.stackLimit, result.possessionCount)));
		}
		static ThingDef GetDrugFor(ChemicalDef chemical)
		{
			if (DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.GetCompProperties<CompProperties_Drug>()?.chemical == chemical).TryRandomElementByWeight((ThingDef x) => x.generateCommonality, out var result2))
			{
				return result2;
			}
			return null;
		}
	}

	public static void AddNewPawn(int index = -1)
	{
		Pawn pawn = NewGeneratedStartingPawn(index);
		StartingAndOptionalPawns.Add(pawn);
		GeneratePossessions(pawn);
	}

	public static bool WorkTypeRequirementsSatisfied()
	{
		if (StartingAndOptionalPawns.Count == 0)
		{
			return false;
		}
		List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			WorkTypeDef workTypeDef = allDefsListForReading[i];
			if (!workTypeDef.requireCapableColonist)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < Find.GameInitData.startingPawnCount; j++)
			{
				if (!StartingAndOptionalPawns[j].WorkTypeIsDisabled(workTypeDef))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (TutorSystem.TutorialMode && StartingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount).Any((Pawn p) => p.WorkTagIsDisabled(WorkTags.Violent)))
		{
			return false;
		}
		return true;
	}

	public static IEnumerable<WorkTypeDef> RequiredWorkTypesDisabledForEveryone()
	{
		List<WorkTypeDef> workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		for (int i = 0; i < workTypes.Count; i++)
		{
			WorkTypeDef workTypeDef = workTypes[i];
			if (!workTypeDef.requireCapableColonist)
			{
				continue;
			}
			bool flag = false;
			List<Pawn> startingAndOptionalPawns = StartingAndOptionalPawns;
			for (int j = 0; j < Find.GameInitData.startingPawnCount; j++)
			{
				if (!startingAndOptionalPawns[j].WorkTypeIsDisabled(workTypeDef))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				yield return workTypeDef;
			}
		}
	}

	public static Thing GenerateStartingPossession(ThingDefCount t)
	{
		Thing thing = ThingMaker.MakeThing(t.ThingDef, GenStuff.RandomStuffFor(t.ThingDef));
		if (thing.def.Minifiable)
		{
			thing = thing.MakeMinified();
		}
		if (t.ThingDef.IsIngestible && t.ThingDef.ingestible.IsMeal)
		{
			FoodUtility.GenerateGoodIngredients(thing, Faction.OfPlayer.ideos.PrimaryIdeo);
		}
		thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);
		thing.stackCount = t.Count;
		return thing;
	}
}
