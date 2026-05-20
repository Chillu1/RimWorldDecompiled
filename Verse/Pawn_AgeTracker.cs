using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Pawn_AgeTracker : IExposable
{
	public enum AgeReversalReason
	{
		Initial,
		Recruited,
		ViaTreatment
	}

	private Pawn pawn;

	private long ageBiologicalTicksInt = -1L;

	private long birthAbsTicksInt = -1L;

	private long nextGrowthCheckTick = -1L;

	private float growth = -1f;

	private float progressToNextBiologicalTick;

	private long ageReversalDemandedAtAgeTicks;

	public long vatGrowTicks;

	private AgeReversalReason lastAgeReversalReason;

	private bool initializedAgeReversalDemand;

	private bool lifeStageChange;

	public float growthPoints = -1f;

	public bool canGainGrowthPoints = true;

	public int lockedLifeStageIndex = -1;

	private int cachedLifeStageIndex = -1;

	private const float BornAtLongitude = 0f;

	private const int GrowthInterval = 240;

	public const int AgeReversalDemandMinAgeYears = 25;

	private const int TreatedPawnAgeReversalDemandInDays = 60;

	public const int MinGrowthBirthday = 3;

	public const int MaxGrowthBirthday = 13;

	private const float NicknameGainChance = 0.5f;

	private readonly IntRange NewPawnAgeReversalDemandInDays = new IntRange(20, 40);

	private readonly IntRange RecruitedPawnAgeReversalDemandInDays = new IntRange(15, 20);

	private const int ColonistDevelopmentStageLessonInterval = 1000;

	private const float GrowthPointsSuspended = 2f;

	private const float YoungAgeCutoff = 7f;

	private const float GrowthPointsFactor_Young = 0.75f;

	private const float GeneratedGrowthPointsFactor = 0.25f;

	private const float MaxAgeChildFactorAge = 11f;

	private const float MinAgeAdultFactorAge = 20f;

	private static List<int> growthMomentAges = null;

	private static List<WorkTypeDef> tmpEnabledWorkTypes = new List<WorkTypeDef>();

	private static List<HediffDef> tmpHediffsGained = new List<HediffDef>();

	public long BirthAbsTicks
	{
		get
		{
			return birthAbsTicksInt;
		}
		set
		{
			birthAbsTicksInt = value;
		}
	}

	public int AgeBiologicalYears => (int)(ageBiologicalTicksInt / 3600000);

	public float AgeBiologicalYearsFloat => (float)ageBiologicalTicksInt / 3600000f;

	public long AgeBiologicalTicks
	{
		get
		{
			return ageBiologicalTicksInt;
		}
		set
		{
			ageBiologicalTicksInt = value;
			CalculateInitialGrowth();
			RecalculateLifeStageIndex();
		}
	}

	public long AgeChronologicalTicks
	{
		get
		{
			return GenTicks.TicksAbs - birthAbsTicksInt;
		}
		set
		{
			BirthAbsTicks = GenTicks.TicksAbs - value;
		}
	}

	public int AgeChronologicalYears => (int)(AgeChronologicalTicks / 3600000);

	public float AgeChronologicalYearsFloat => (float)AgeChronologicalTicks / 3600000f;

	public int BirthYear => GenDate.Year(birthAbsTicksInt, 0f);

	public int BirthDayOfSeasonZeroBased => GenDate.DayOfSeason(birthAbsTicksInt, 0f);

	public int BirthDayOfYear => GenDate.DayOfYear(birthAbsTicksInt, 0f);

	public Quadrum BirthQuadrum => GenDate.Quadrum(birthAbsTicksInt, 0f);

	public string AgeNumberString
	{
		get
		{
			string text = AgeBiologicalYearsFloat.ToStringApproxAge();
			if (AgeChronologicalYears != AgeBiologicalYears)
			{
				text = text + " (" + AgeChronologicalYears + ")";
			}
			return text;
		}
	}

	public string AgeTooltipString
	{
		get
		{
			ageBiologicalTicksInt.TicksToPeriod(out var years, out var quadrums, out var days, out var hoursFloat);
			(GenTicks.TicksAbs - birthAbsTicksInt).TicksToPeriod(out var years2, out var quadrums2, out var days2, out hoursFloat);
			string text = "FullDate".Translate(Find.ActiveLanguageWorker.OrdinalNumber(BirthDayOfSeasonZeroBased + 1), BirthQuadrum.Label(), BirthYear);
			string text2 = "Born".Translate(text) + "\n" + "AgeChronological".Translate(years2, quadrums2, days2) + "\n" + "AgeBiological".Translate(years, quadrums, days);
			if (Prefs.DevMode)
			{
				text2 += "\n\nDev mode info:";
				text2 = text2 + "\nageBiologicalTicksInt: " + ageBiologicalTicksInt;
				text2 = text2 + "\nbirthAbsTicksInt: " + birthAbsTicksInt;
				text2 = text2 + "\nBiologicalTicksPerTick: " + BiologicalTicksPerTick;
				text2 = text2 + "\ngrowth: " + growth;
				text2 = text2 + "\nage reversal demand deadline: " + ((int)Math.Abs(AgeReversalDemandedDeadlineTicks)).ToStringTicksToPeriod() + ((AgeReversalDemandedDeadlineTicks < 0) ? " past deadline" : " in future") + "(" + AgeReversalDemandedDeadlineTicks + ")";
				text2 = text2 + "\nlife stage: " + CurLifeStage;
				text2 = text2 + "\nsterile: " + pawn.Sterile();
			}
			return text2;
		}
	}

	public int CurLifeStageIndex
	{
		get
		{
			if (cachedLifeStageIndex < 0)
			{
				RecalculateLifeStageIndex();
			}
			return cachedLifeStageIndex;
		}
	}

	public LifeStageDef CurLifeStage => CurLifeStageRace.def;

	public LifeStageAge CurLifeStageRace => GetLifeStageAge(CurLifeStageIndex);

	public PawnKindLifeStage CurKindLifeStage
	{
		get
		{
			if (pawn.RaceProps.Humanlike)
			{
				Log.ErrorOnce("Tried to get CurKindLifeStage from humanlike pawn " + pawn, 8888811);
				return null;
			}
			return pawn.kindDef.lifeStages[CurLifeStageIndex];
		}
	}

	public int MaxRaceLifeStageIndex => pawn.RaceProps.lifeStageAges.Count - 1;

	public float Growth => growth;

	public float AdultMinAge
	{
		get
		{
			if (pawn.RaceProps.Humanlike)
			{
				return pawn.RaceProps.lifeStageAges.FirstOrDefault((LifeStageAge lifeStageAge) => lifeStageAge.def.developmentalStage.Adult())?.minAge ?? 0f;
			}
			if (pawn.RaceProps.lifeStageAges.Count <= 0)
			{
				return 0f;
			}
			return pawn.RaceProps.lifeStageAges.Last().minAge;
		}
	}

	public long AdultMinAgeTicks => Mathf.FloorToInt(AdultMinAge * 3600000f);

	private long TicksToAdulthood => AdultMinAgeTicks - AgeBiologicalTicks;

	public bool Adult => TicksToAdulthood <= 0;

	public long AgeReversalDemandedDeadlineTicks => ageReversalDemandedAtAgeTicks - AgeBiologicalTicks;

	public AgeReversalReason LastAgeReversalReason => lastAgeReversalReason;

	public float BiologicalTicksPerTick
	{
		get
		{
			if (!pawn.RaceProps.Humanlike)
			{
				return 1f;
			}
			if (ModsConfig.BiotechActive && pawn.ParentHolder != null && pawn.ParentHolder is Building_GrowthVat)
			{
				return 1f;
			}
			float num = 1f;
			float ageBiologicalYearsFloat = AgeBiologicalYearsFloat;
			if (ageBiologicalYearsFloat <= 11f)
			{
				num *= ChildAgingMultiplier;
			}
			else if (ageBiologicalYearsFloat >= 20f)
			{
				num *= AdultAgingMultiplier;
			}
			else
			{
				float t = Mathf.InverseLerp(11f, 20f, ageBiologicalYearsFloat);
				num *= Mathf.Lerp(ChildAgingMultiplier, AdultAgingMultiplier, t);
			}
			if (pawn.genes != null)
			{
				num *= pawn.genes.BiologicalAgeTickFactor;
			}
			return num;
		}
	}

	public float AdultAgingMultiplier => Find.Storyteller.difficulty.adultAgingRate;

	public float ChildAgingMultiplier => Find.Storyteller.difficulty.childAgingRate;

	public bool AtMaxGrowthTier => GrowthTier >= GrowthUtility.GrowthTiers.Length - 1;

	public float PercentToNextGrowthTier
	{
		get
		{
			if (growthPoints <= 0f)
			{
				return 0f;
			}
			if (AtMaxGrowthTier)
			{
				return 1f;
			}
			int growthTier = GrowthTier;
			return Mathf.InverseLerp(GrowthUtility.GrowthTiers[growthTier].pointsRequirement, GrowthUtility.GrowthTiers[growthTier + 1].pointsRequirement, growthPoints);
		}
	}

	public int GrowthTier
	{
		get
		{
			for (int num = GrowthUtility.GrowthTiers.Length - 1; num >= 0; num--)
			{
				if (growthPoints >= GrowthUtility.GrowthTiers[num].pointsRequirement)
				{
					return num;
				}
			}
			return 0;
		}
	}

	public float GrowthPointsPerDay
	{
		get
		{
			if (!ModsConfig.BiotechActive)
			{
				return 0f;
			}
			if (pawn.ParentHolder is Building_GrowthVat)
			{
				return 2f;
			}
			if (pawn.Suspended)
			{
				return 0f;
			}
			Need_Learning need_Learning = pawn.needs?.learning;
			if (need_Learning != null && !need_Learning.Suspended)
			{
				return GrowthPointsPerDayAtLearningLevel(need_Learning.CurLevel);
			}
			return 0f;
		}
	}

	private float GrowthPointsFactor
	{
		get
		{
			if ((float)AgeBiologicalYears < 7f)
			{
				return 0.75f;
			}
			return 1f;
		}
	}

	public Pawn_AgeTracker(Pawn newPawn)
	{
		pawn = newPawn;
	}

	public float LifeStageMinAge(LifeStageDef lifeStage)
	{
		foreach (LifeStageAge lifeStageAge in pawn.RaceProps.lifeStageAges)
		{
			if (lifeStageAge.def == lifeStage)
			{
				return lifeStageAge.minAge;
			}
		}
		Log.Error($"Life stage def {lifeStage} not found while searching for min age of {pawn}");
		return 0f;
	}

	private float GrowthPointsPerDayAtLearningLevel(float level)
	{
		return level * GrowthPointsFactor * pawn.ageTracker.ChildAgingMultiplier;
	}

	public void TrySimulateGrowthPoints()
	{
		if (!ModsConfig.BiotechActive || !pawn.RaceProps.Humanlike || AgeBiologicalYears >= 13)
		{
			return;
		}
		if (growthMomentAges == null)
		{
			growthMomentAges = new List<int>();
			growthMomentAges.Add(3);
			growthMomentAges.AddRange(GrowthUtility.GrowthMomentAges);
		}
		growthPoints = 0f;
		int ageBiologicalYears = AgeBiologicalYears;
		for (int num = growthMomentAges.Count - 1; num >= 0; num--)
		{
			if (ageBiologicalYears >= growthMomentAges[num])
			{
				float num2 = GrowthPointsPerDayAtLearningLevel(Rand.Range(0.2f, 0.5f)) * (((float)growthMomentAges[num] < 7f) ? 0.75f : 1f);
				int num3 = growthMomentAges[num] * 3600000;
				long num4 = AgeBiologicalTicks;
				while (num4 > num3)
				{
					num4 -= 60000;
					growthPoints += num2;
				}
				break;
			}
		}
		growthPoints *= 0.25f;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref ageBiologicalTicksInt, "ageBiologicalTicks", 0L);
		Scribe_Values.Look(ref birthAbsTicksInt, "birthAbsTicks", 0L);
		Scribe_Values.Look(ref growth, "growth", -1f);
		Scribe_Values.Look(ref progressToNextBiologicalTick, "progressToNextBiologicalTick", 0f);
		Scribe_Values.Look(ref nextGrowthCheckTick, "nextGrowthCheckTick", -1L);
		Scribe_Values.Look(ref vatGrowTicks, "vatGrowTicks", 0L);
		Scribe_Values.Look(ref ageReversalDemandedAtAgeTicks, "ageReversalDemandedAtAgeTicks", long.MaxValue);
		Scribe_Values.Look(ref lastAgeReversalReason, "lastAgeReversalReason", AgeReversalReason.Initial);
		Scribe_Values.Look(ref initializedAgeReversalDemand, "initializedAgeReversalDemand", defaultValue: false);
		Scribe_Values.Look(ref lifeStageChange, "lifeStageChange", defaultValue: false);
		Scribe_Values.Look(ref growthPoints, "growthPoints", -1f);
		Scribe_Values.Look(ref canGainGrowthPoints, "canGainGrowthPoints", defaultValue: true);
		Scribe_Values.Look(ref lockedLifeStageIndex, "lockedLifeStageIndex", -1);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			cachedLifeStageIndex = -1;
			if (growthPoints < 0f)
			{
				TrySimulateGrowthPoints();
			}
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && AgeBiologicalYearsFloat > 25f && (float)AgeReversalDemandedDeadlineTicks > 5400000f)
		{
			ResetAgeReversalDemand(AgeReversalReason.Initial);
		}
		if (ageReversalDemandedAtAgeTicks == long.MaxValue)
		{
			ResetAgeReversalDemand(AgeReversalReason.Initial);
		}
	}

	private void TickBiologicalAge(int interval)
	{
		progressToNextBiologicalTick += BiologicalTicksPerTick * (float)interval;
		int num = Mathf.FloorToInt(progressToNextBiologicalTick);
		if (num > 0)
		{
			progressToNextBiologicalTick -= num;
			ageBiologicalTicksInt += num;
		}
	}

	public void AgeTickInterval(int delta)
	{
		if (lifeStageChange)
		{
			PostResolveLifeStageChange();
		}
		int ageBiologicalYears = AgeBiologicalYears;
		TickBiologicalAge(delta);
		if (lockedLifeStageIndex >= 0 || (pawn.IsMutant && pawn.mutant.Def.disableAging))
		{
			return;
		}
		if (Find.TickManager.TicksGame >= nextGrowthCheckTick)
		{
			CalculateGrowth(240);
		}
		if (pawn.IsHashIntervalTick(60000, delta))
		{
			CheckAgeReversalDemand();
		}
		if (ageBiologicalYears < AgeBiologicalYears)
		{
			BirthdayBiological(AgeBiologicalYears);
		}
		if (initializedAgeReversalDemand && pawn.MapHeld != null && pawn.IsHashIntervalTick(2500, delta) && ExpectationsUtility.CurrentExpectationFor(pawn.MapHeld).order < ThoughtDefOf.AgeReversalDemanded.minExpectation.order)
		{
			ageReversalDemandedAtAgeTicks += 2500L;
		}
		if (pawn.IsFreeColonist && pawn.IsHashIntervalTick(1000, delta))
		{
			if (CurLifeStage.developmentalStage == DevelopmentalStage.Baby)
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.Babies, OpportunityType.Important);
			}
			else if (CurLifeStage.developmentalStage == DevelopmentalStage.Child)
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.Children, OpportunityType.Important);
			}
		}
	}

	public void AgeTickMothballed(int interval)
	{
		long num = ageBiologicalTicksInt;
		TickBiologicalAge(interval);
		CalculateGrowth(interval);
		CheckAgeReversalDemand();
		for (int i = (int)(num / 3600000) + 1; i <= AgeBiologicalYears; i++)
		{
			BirthdayBiological(i);
		}
	}

	public void Notify_TickedInGrowthVat(int ticks)
	{
		vatGrowTicks++;
		AgeTickMothballed(ticks);
	}

	public void LockCurrentLifeStageIndex(int index)
	{
		lockedLifeStageIndex = index;
		RecalculateLifeStageIndex();
	}

	private void CheckAgeReversalDemand()
	{
		if (ModsConfig.IdeologyActive && !initializedAgeReversalDemand && pawn.Faction == Faction.OfPlayer && pawn.MapHeld != null)
		{
			ResetAgeReversalDemand(lastAgeReversalReason);
			initializedAgeReversalDemand = true;
		}
	}

	private void CalculateInitialGrowth()
	{
		growth = Mathf.Clamp01(AgeBiologicalYearsFloat / pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].minAge);
		nextGrowthCheckTick = Find.TickManager.TicksGame + 240;
	}

	private void CalculateGrowth(int interval)
	{
		if (growth >= 1f)
		{
			nextGrowthCheckTick = long.MaxValue;
			return;
		}
		growth += PawnUtility.BodyResourceGrowthSpeed(pawn) * (float)interval / Mathf.Max(AdultMinAgeTicks, 1f);
		growth = Mathf.Min(growth, 1f);
		nextGrowthCheckTick = Find.TickManager.TicksGame + 240;
		RecalculateLifeStageIndex();
	}

	private void RecalculateLifeStageIndex()
	{
		int num = -1;
		if (lockedLifeStageIndex >= 0)
		{
			num = lockedLifeStageIndex;
		}
		else if (growth < 0f)
		{
			CalculateInitialGrowth();
		}
		float num2 = ((!pawn.RaceProps.Humanlike) ? Mathf.Lerp(0f, pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].minAge, growth) : ((float)AgeBiologicalYears));
		if (num < 0)
		{
			List<LifeStageAge> lifeStageAges = pawn.RaceProps.lifeStageAges;
			for (int num3 = lifeStageAges.Count - 1; num3 >= 0; num3--)
			{
				if (lifeStageAges[num3].minAge <= num2 + 1E-06f)
				{
					num = num3;
					break;
				}
			}
		}
		if (num == -1)
		{
			num = 0;
		}
		int index = cachedLifeStageIndex;
		bool num4 = cachedLifeStageIndex != num;
		cachedLifeStageIndex = num;
		if (num4)
		{
			lifeStageChange = true;
			if (!pawn.RaceProps.Humanlike || ModsConfig.BiotechActive)
			{
				pawn.Drawer.renderer.SetAllGraphicsDirty();
				CheckChangePawnKindName();
			}
			CurLifeStage.Worker.Notify_LifeStageStarted(pawn, GetLifeStageAge(index)?.def);
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
			}
		}
	}

	private void BirthdayBiological(int birthdayAge)
	{
		tmpHediffsGained.Clear();
		tmpEnabledWorkTypes.Clear();
		bool flag = (float)birthdayAge == AdultMinAge;
		bool flag2 = pawn.DevelopmentalStage.Child() && (float)birthdayAge == CurLifeStageRace.minAge;
		if (flag || flag2)
		{
			RecalculateLifeStageIndex();
		}
		float age = (float)birthdayAge / pawn.GetStatValue(StatDefOf.LifespanFactor);
		foreach (HediffGiver_Birthday item in AgeInjuryUtility.RandomHediffsToGainOnBirthday(pawn, age))
		{
			if (item.TryApply(pawn))
			{
				tmpHediffsGained.Add(item.hediff);
			}
		}
		Building_Bed ownedBed = pawn.ownership.OwnedBed;
		if (ownedBed != null && pawn.ageTracker.CurLifeStage.bodySizeFactor > ownedBed.def.building.bed_maxBodySize)
		{
			pawn.ownership.UnclaimBed();
		}
		if (pawn.RaceProps.Humanlike)
		{
			List<LifeStageWorkSettings> lifeStageWorkSettings = pawn.RaceProps.lifeStageWorkSettings;
			for (int i = 0; i < lifeStageWorkSettings.Count; i++)
			{
				if (lifeStageWorkSettings[i].minAge == birthdayAge)
				{
					tmpEnabledWorkTypes.Add(lifeStageWorkSettings[i].workType);
				}
			}
			if (tmpEnabledWorkTypes.Count > 0)
			{
				pawn.Notify_DisabledWorkTypesChanged();
			}
			TryChildGrowthMoment(birthdayAge, out var newPassionOptions, out var newTraitOptions, out var passionGainsCount);
			bool flag3 = !flag2 && (!tmpEnabledWorkTypes.NullOrEmpty() || passionGainsCount > 0 || newTraitOptions > 0);
			if (!PawnUtility.ShouldSendNotificationAbout(pawn) || pawn.Faction != Faction.OfPlayer || pawn.IsQuestLodger())
			{
				if (passionGainsCount > 0)
				{
					SkillDef skillDef = ChoiceLetter_GrowthMoment.PassionOptions(pawn, passionGainsCount, checkGenes: true).FirstOrFallback();
					if (skillDef != null)
					{
						SkillRecord skill = pawn.skills.GetSkill(skillDef);
						if (skill != null)
						{
							skill.passion = skill.passion.IncrementPassion();
						}
					}
				}
				if (newTraitOptions > 0)
				{
					Trait trait = PawnGenerator.GenerateTraitsFor(pawn, 1, null, growthMomentTrait: true).FirstOrFallback();
					if (trait != null)
					{
						pawn.story.traits.GainTrait(trait);
					}
				}
			}
			else if (tmpHediffsGained.Count > 0 || flag3)
			{
				TaggedString text = "LetterBirthdayBiological".Translate(pawn, birthdayAge);
				if (tmpHediffsGained.Count > 0)
				{
					text += "\n\n" + "BirthdayBiologicalAgeInjuries".Translate(pawn);
					text += ":\n\n" + tmpHediffsGained.Select((HediffDef h) => h.LabelCap.Resolve()).ToLineList("  - ");
				}
				if (ModsConfig.BiotechActive && flag3 && pawn.Spawned && (pawn.DevelopmentalStage.Juvenile() || flag))
				{
					EffecterDefOf.Birthday.SpawnAttached(pawn, pawn.Map);
				}
				if (tmpHediffsGained.Count > 0)
				{
					LetterDef negativeEvent = LetterDefOf.NegativeEvent;
					Find.LetterStack.ReceiveLetter("LetterLabelBirthday".Translate(), text, negativeEvent, (TargetInfo)pawn);
				}
				if (ModsConfig.BiotechActive && flag3)
				{
					Name name = pawn.Name;
					if (pawn.Name is NameTriple { NickSet: false } nameTriple)
					{
						Rand.PushState(Gen.HashCombine(pawn.thingIDNumber, birthdayAge));
						try
						{
							if (Rand.Chance(0.5f))
							{
								string name2 = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard).GetName(PawnNameSlot.Nick, pawn.gender);
								pawn.Name = new NameTriple(nameTriple.First, name2, nameTriple.Last);
							}
						}
						finally
						{
							Rand.PopState();
						}
					}
					LetterDef negativeEvent = (flag ? LetterDefOf.ChildToAdult : LetterDefOf.ChildBirthday);
					ChoiceLetter_GrowthMoment choiceLetter_GrowthMoment = (ChoiceLetter_GrowthMoment)LetterMaker.MakeLetter(negativeEvent);
					List<string> enabledWorkTypes = tmpEnabledWorkTypes.Select((WorkTypeDef w) => w.labelShort.CapitalizeFirst()).ToList();
					choiceLetter_GrowthMoment.ConfigureGrowthLetter(pawn, newPassionOptions, newTraitOptions, passionGainsCount, enabledWorkTypes, name);
					choiceLetter_GrowthMoment.Label = (flag ? "LetterLabelBecameAdult".Translate(pawn) : "BirthdayGrowthMoment".Translate(pawn, name.ToStringShort.Named("PAWNNAME")));
					choiceLetter_GrowthMoment.StartTimeout(120000);
					canGainGrowthPoints = false;
					Find.LetterStack.ReceiveLetter(choiceLetter_GrowthMoment);
				}
				tmpEnabledWorkTypes.Clear();
			}
		}
		tmpHediffsGained.Clear();
	}

	public void TryChildGrowthMoment(int birthdayAge, out int newPassionOptions, out int newTraitOptions, out int passionGainsCount)
	{
		newPassionOptions = 0;
		newTraitOptions = 0;
		passionGainsCount = 0;
		if (ModsConfig.BiotechActive && GrowthUtility.IsGrowthBirthday(birthdayAge))
		{
			int growthTier = GrowthTier;
			newPassionOptions = GrowthUtility.GrowthTiers[growthTier].passionChoices;
			newTraitOptions = GrowthUtility.GrowthTiers[growthTier].traitChoices;
			passionGainsCount = Mathf.Min(pawn.skills.skills.Count((SkillRecord s) => (int)s.passion < 2), GrowthUtility.GrowthTiers[growthTier].PassionGainFor(pawn));
		}
	}

	public void ResetAgeReversalDemand(AgeReversalReason reason, bool cancelInitialization = false)
	{
		long num = reason switch
		{
			AgeReversalReason.Recruited => RecruitedPawnAgeReversalDemandInDays.RandomInRange, 
			AgeReversalReason.ViaTreatment => 60, 
			_ => NewPawnAgeReversalDemandInDays.RandomInRange, 
		} * 60000;
		long num2 = Math.Max(AgeBiologicalTicks, 90000000L) + num;
		if (reason != AgeReversalReason.Recruited || num2 >= ageReversalDemandedAtAgeTicks)
		{
			ageReversalDemandedAtAgeTicks = num2;
			lastAgeReversalReason = reason;
			if (cancelInitialization)
			{
				initializedAgeReversalDemand = false;
			}
		}
	}

	public void PostResolveLifeStageChange()
	{
		pawn.health.CheckForStateChange(null, null);
		lifeStageChange = false;
		MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
	}

	public void DebugForceAgeReversalDemandNow()
	{
		ageReversalDemandedAtAgeTicks = AgeBiologicalTicks;
	}

	public void DebugResetAgeReversalDemand()
	{
		ResetAgeReversalDemand(AgeReversalReason.Initial);
	}

	public void Notify_IdeoChanged()
	{
		Ideo ideo = pawn.Ideo;
		if (ideo != null && ideo.HasPrecept(PreceptDefOf.AgeReversal_Demanded))
		{
			ResetAgeReversalDemand(AgeReversalReason.Recruited);
		}
	}

	public void DebugForceBirthdayBiological()
	{
		BirthdayBiological(AgeBiologicalYears);
	}

	public void DebugSetAge(long finalAgeTicks)
	{
		long num = finalAgeTicks - AgeBiologicalTicks;
		while (num > 0)
		{
			int ageBiologicalYears = AgeBiologicalYears;
			long num2 = NextBirthdayTick(AgeBiologicalTicks) - AgeBiologicalTicks;
			long num3 = ((num2 > num) ? num : num2);
			AgeBiologicalTicks += num3;
			num -= num3;
			if (AgeBiologicalYears > ageBiologicalYears)
			{
				BirthdayBiological(AgeBiologicalYears);
			}
		}
		static long NextBirthdayTick(long ageTicks)
		{
			return (ageTicks / 3600000 + 1) * 3600000;
		}
	}

	public void DebugSetGrowth(float val)
	{
		growth = val;
		RecalculateLifeStageIndex();
	}

	public void CheckChangePawnKindName()
	{
		if (!(pawn.Name is NameSingle { Numerical: not false } nameSingle))
		{
			return;
		}
		string text = pawn.KindLabel.CapitalizeFirst();
		if (!(nameSingle.NameWithoutNumber == text))
		{
			int number = nameSingle.Number;
			string text2 = pawn.KindLabel.CapitalizeFirst() + " " + number;
			if (!NameUseChecker.NameSingleIsUsed(text2))
			{
				pawn.Name = new NameSingle(text2, numerical: true);
			}
			else
			{
				pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Numeric);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private LifeStageAge GetLifeStageAge(int index)
	{
		List<LifeStageAge> lifeStageAges = pawn.def.race.lifeStageAges;
		if (index < 0 || index >= lifeStageAges.Count)
		{
			return null;
		}
		return lifeStageAges[index];
	}
}
