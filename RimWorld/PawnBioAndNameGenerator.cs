using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public static class PawnBioAndNameGenerator
{
	private const float MinAgeForAdulthood = 20f;

	private const float SolidBioChance = 0.25f;

	private const float SolidNameChance = 0.5f;

	private const float TryPreferredNameChance_Bio = 0.5f;

	private const float TryPreferredNameChance_Name = 0.5f;

	private const float ShuffledNicknameChance = 0.15f;

	private const float ShuffledNicknameChanceImperial = 0.05f;

	private const float ShuffledNicknameChanceUnisex = 0.8f;

	public static readonly BackstoryCategoryFilter ChildCategoryGroup = new BackstoryCategoryFilter
	{
		categories = new List<string> { "Child" },
		commonality = 1f
	};

	private static readonly BackstoryCategoryFilter NewbornCategoryGroup = new BackstoryCategoryFilter
	{
		categories = new List<string> { "Newborn" },
		commonality = 1f
	};

	private static readonly BackstoryCategoryFilter FallbackCategoryGroup = new BackstoryCategoryFilter
	{
		categories = new List<string> { "Civil" },
		commonality = 1f
	};

	private static List<BackstoryDef> tmpBackstories = new List<BackstoryDef>();

	private static List<string> tmpNames = new List<string>();

	private static HashSet<string> usedNamesTmp = new HashSet<string>();

	public static void GiveAppropriateBioAndNameTo(Pawn pawn, FactionDef factionType, PawnGenerationRequest request, XenotypeDef xenotype = null)
	{
		List<BackstoryCategoryFilter> backstoryCategoryFiltersFor = GetBackstoryCategoryFiltersFor(pawn, factionType);
		bool flag = pawn.DevelopmentalStage.Baby();
		if (!request.ForceNoBackstory && !request.OnlyUseForcedBackstories && !flag && (Rand.Value < 0.25f || pawn.kindDef.factionLeader) && TryGiveSolidBioTo(pawn, request.FixedLastName, backstoryCategoryFiltersFor))
		{
			return;
		}
		GiveShuffledBioTo(pawn, factionType, request.FixedLastName, backstoryCategoryFiltersFor, request.ForceNoBackstory, forceNoNick: false, xenotype, request.OnlyUseForcedBackstories);
		if (flag && pawn.Name is NameTriple nameTriple)
		{
			if (pawn.Faction.IsPlayerSafe())
			{
				pawn.Name = new NameTriple("Baby".Translate().CapitalizeFirst(), null, nameTriple.Last);
			}
			else
			{
				pawn.Name = new NameTriple(nameTriple.First, null, nameTriple.Last);
			}
		}
	}

	private static void GiveShuffledBioTo(Pawn pawn, FactionDef factionType, string requiredLastName, List<BackstoryCategoryFilter> backstoryCategories, bool forceNoBackstory = false, bool forceNoNick = false, XenotypeDef xenotype = null, bool onlyForcedBackstories = false)
	{
		bool flag = pawn.kindDef.fixedChildBackstories.Any();
		bool flag2 = pawn.kindDef.fixedAdultBackstories.Any();
		bool flag3 = pawn.ageTracker.AgeBiologicalYearsFloat >= 20f;
		if (!forceNoBackstory)
		{
			if (flag)
			{
				pawn.story.Childhood = pawn.kindDef.fixedChildBackstories.RandomElement();
			}
			else if (!onlyForcedBackstories)
			{
				FillBackstorySlotShuffled(pawn, BackstorySlot.Childhood, backstoryCategories, factionType, flag3 ? new BackstorySlot?(BackstorySlot.Adulthood) : ((BackstorySlot?)null));
			}
			if (flag3 && flag2)
			{
				pawn.story.Adulthood = pawn.kindDef.fixedAdultBackstories.RandomElement();
			}
			else if (flag3 && !onlyForcedBackstories)
			{
				FillBackstorySlotShuffled(pawn, BackstorySlot.Adulthood, backstoryCategories, factionType);
			}
		}
		pawn.Name = GeneratePawnName(pawn, NameStyle.Full, requiredLastName, forceNoNick, xenotype);
	}

	public static void FillBackstorySlotShuffled(Pawn pawn, BackstorySlot slot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType, BackstorySlot? mustBeCompatibleTo = null)
	{
		BackstoryCategoryFilter categoryFilter = backstoryCategories.RandomElementByWeight((BackstoryCategoryFilter c) => c.commonality) ?? FallbackCategoryGroup;
		IEnumerable<BackstoryDef> source = DefDatabase<BackstoryDef>.AllDefs.Where((BackstoryDef bs) => bs.shuffleable && categoryFilter.Matches(bs));
		tmpBackstories.Clear();
		if (!mustBeCompatibleTo.HasValue)
		{
			tmpBackstories.AddRange(source.Where((BackstoryDef bs) => bs.slot == slot));
		}
		else
		{
			IEnumerable<BackstoryDef> compatibleBackstories = source.Where((BackstoryDef bs) => bs.slot == mustBeCompatibleTo.Value);
			tmpBackstories.AddRange(source.Where((BackstoryDef bs) => bs.slot == slot && compatibleBackstories.Any((BackstoryDef b) => !b.requiredWorkTags.OverlapsWithOnAnyWorkType(bs.workDisables))));
		}
		if (!(from bs in tmpBackstories.TakeRandom(20)
			where (slot != BackstorySlot.Adulthood || bs.requiredWorkTags == WorkTags.None || !bs.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.Childhood.workDisables)) ? true : false
			select bs).TryRandomElementByWeight(BackstorySelectionWeight, out var result))
		{
			Log.Error("No shuffled " + slot.ToString() + " found for " + pawn.ToStringSafe() + " of " + factionType.ToStringSafe() + ". Choosing random.");
			result = DefDatabase<BackstoryDef>.AllDefs.Where((BackstoryDef bs) => bs.slot == slot).RandomElement();
		}
		if (slot == BackstorySlot.Adulthood)
		{
			pawn.story.Adulthood = result;
		}
		else
		{
			pawn.story.Childhood = result;
		}
		tmpBackstories.Clear();
	}

	private static bool TryGiveSolidBioTo(Pawn pawn, string requiredLastName, List<BackstoryCategoryFilter> backstoryCategories)
	{
		if (!TryGetRandomUnusedSolidBioFor(backstoryCategories, pawn.kindDef, pawn.gender, requiredLastName, out var result))
		{
			return false;
		}
		if (result.rare && Rand.Value < 0.5f && !TryGetRandomUnusedSolidBioFor(backstoryCategories, pawn.kindDef, pawn.gender, requiredLastName, out result))
		{
			return false;
		}
		pawn.Name = result.name;
		pawn.story.Childhood = result.childhood;
		if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
		{
			pawn.story.Adulthood = result.adulthood;
		}
		return true;
	}

	private static bool IsBioUseable(PawnBio bio, BackstoryCategoryFilter categoryFilter, PawnKindDef kind, Gender gender, string requiredLastName)
	{
		if (bio.gender != GenderPossibility.Either)
		{
			if (gender == Gender.Male && bio.gender != GenderPossibility.Male)
			{
				return false;
			}
			if (gender == Gender.Female && bio.gender != GenderPossibility.Female)
			{
				return false;
			}
		}
		if (!requiredLastName.NullOrEmpty() && bio.name.Last != requiredLastName)
		{
			return false;
		}
		if (kind.factionLeader && !bio.pirateKing)
		{
			return false;
		}
		if (!categoryFilter.Matches(bio))
		{
			return false;
		}
		if (bio.name.UsedThisGame)
		{
			return false;
		}
		if (kind.requiredWorkTags != WorkTags.None)
		{
			if (bio.childhood != null && (bio.childhood.workDisables & kind.requiredWorkTags) != WorkTags.None)
			{
				return false;
			}
			if (bio.adulthood != null && (bio.adulthood.workDisables & kind.requiredWorkTags) != WorkTags.None)
			{
				return false;
			}
		}
		return true;
	}

	private static bool TryGetRandomUnusedSolidBioFor(List<BackstoryCategoryFilter> backstoryCategories, PawnKindDef kind, Gender gender, string requiredLastName, out PawnBio result)
	{
		BackstoryCategoryFilter categoryFilter = backstoryCategories.RandomElementByWeight((BackstoryCategoryFilter c) => c.commonality) ?? FallbackCategoryGroup;
		if (Rand.Value < 0.5f)
		{
			tmpNames.Clear();
			tmpNames.AddRange(Prefs.PreferredNames);
			tmpNames.Shuffle();
			foreach (string tmpName in tmpNames)
			{
				foreach (PawnBio allBio in SolidBioDatabase.allBios)
				{
					if (tmpName == allBio.name.ToString() && IsBioUseable(allBio, categoryFilter, kind, gender, requiredLastName))
					{
						result = allBio;
						return true;
					}
				}
			}
		}
		return (from bio in SolidBioDatabase.allBios.TakeRandom(20)
			where IsBioUseable(bio, categoryFilter, kind, gender, requiredLastName)
			select bio).TryRandomElementByWeight(BioSelectionWeight, out result);
	}

	public static NameTriple TryGetRandomUnusedSolidName(Gender gender, string requiredLastName = null, bool forceNoNick = false)
	{
		List<NameTriple> listForGender = PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Either);
		List<NameTriple> list = ((gender == Gender.Male) ? PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Male) : PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Female));
		float num = ((float)listForGender.Count + 0.1f) / ((float)(listForGender.Count + list.Count) + 0.1f);
		List<NameTriple> list2 = ((!(Rand.Value < num)) ? list : listForGender);
		if (list2.Count == 0)
		{
			Log.Error("Empty solid pawn name list for gender: " + gender.ToString() + ".");
			return null;
		}
		if (Rand.Value < 0.5f)
		{
			tmpNames.Clear();
			tmpNames.AddRange(Prefs.PreferredNames);
			tmpNames.Shuffle();
			foreach (string tmpName in tmpNames)
			{
				NameTriple nameTriple = NameTriple.FromString(tmpName, forceNoNick);
				if (list2.Contains(nameTriple) && !nameTriple.UsedThisGame && (requiredLastName == null || !(nameTriple.Last != requiredLastName)))
				{
					return nameTriple;
				}
			}
		}
		list2.Shuffle();
		return list2.Where(delegate(NameTriple name)
		{
			if (requiredLastName != null && name.Last != requiredLastName)
			{
				return false;
			}
			return !name.UsedThisGame;
		}).FirstOrDefault();
	}

	private static List<BackstoryCategoryFilter> GetBackstoryCategoryFiltersFor(Pawn pawn, FactionDef faction)
	{
		if (pawn.DevelopmentalStage.Baby())
		{
			return new List<BackstoryCategoryFilter> { NewbornCategoryGroup };
		}
		if (pawn.DevelopmentalStage.Child())
		{
			if (faction == FactionDefOf.PlayerTribe)
			{
				return LifeStageWorker_HumanlikeChild.ChildTribalBackstoryFilters;
			}
			return new List<BackstoryCategoryFilter> { ChildCategoryGroup };
		}
		if (!pawn.kindDef.backstoryFiltersOverride.NullOrEmpty())
		{
			return pawn.kindDef.backstoryFiltersOverride;
		}
		List<BackstoryCategoryFilter> list = new List<BackstoryCategoryFilter>();
		if (pawn.kindDef.backstoryFilters != null)
		{
			list.AddRange(pawn.kindDef.backstoryFilters);
		}
		if (faction != null && !faction.backstoryFilters.NullOrEmpty())
		{
			for (int i = 0; i < faction.backstoryFilters.Count; i++)
			{
				BackstoryCategoryFilter item = faction.backstoryFilters[i];
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		if (!list.NullOrEmpty())
		{
			return list;
		}
		Log.ErrorOnce("PawnKind " + pawn.kindDef?.ToString() + " generating with factionDef " + faction?.ToString() + ": no backstoryCategories in either.", 1871521);
		return new List<BackstoryCategoryFilter> { FallbackCategoryGroup };
	}

	public static Name GeneratePawnName(Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null, bool forceNoNick = false, XenotypeDef xenotype = null)
	{
		switch (style)
		{
		case NameStyle.Full:
		{
			CultureDef cultureDef = pawn.Faction?.ideos?.PrimaryCulture;
			if (Find.IdeoManager.classicMode && cultureDef != null && !pawn.Faction.def.allowedCultures.NullOrEmpty())
			{
				cultureDef = pawn.Faction.def.allowedCultures.RandomElement();
			}
			return GenerateFullPawnName(pawn.def, pawn.kindDef.GetNameMaker(pawn.gender), pawn.story, xenotype, pawn.RaceProps.GetNameGenerator(pawn.gender), cultureDef, pawn.IsCreepJoiner, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName, forceNoNick);
		}
		case NameStyle.Numeric:
			try
			{
				foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
				{
					if (item.Name is NameSingle nameSingle)
					{
						usedNamesTmp.Add(nameSingle.Name);
					}
				}
				int num = 1;
				string text;
				while (true)
				{
					text = pawn.KindLabel.CapitalizeFirst() + " " + num;
					if (!usedNamesTmp.Contains(text))
					{
						break;
					}
					num++;
				}
				return new NameSingle(text, numerical: true);
			}
			finally
			{
				usedNamesTmp.Clear();
			}
		default:
			throw new InvalidOperationException();
		}
	}

	public static Name GenerateFullPawnName(ThingDef genFor, RulePackDef pawnKindNameMaker = null, Pawn_StoryTracker story = null, XenotypeDef xenotype = null, RulePackDef nameGenner = null, CultureDef primaryCulture = null, bool creepjoiner = false, Gender gender = Gender.None, PawnNameCategory nameCategory = PawnNameCategory.HumanStandard, string forcedLastName = null, bool forceNoNick = false)
	{
		if (ModsConfig.AnomalyActive && creepjoiner)
		{
			string name = PawnNameDatabaseShuffled.BankOf(nameCategory).GetName(PawnNameSlot.First, gender);
			List<Rule> extraRules = new List<Rule>
			{
				new Rule_String("creepjoinerFirstname", name)
			};
			return NameResolvedFrom(RulePackDefOf.NamerPersonCreepjoiner, forceNoNick, extraRules);
		}
		if (ModsConfig.BiotechActive && xenotype != null)
		{
			RulePackDef nameMaker = xenotype.GetNameMaker(gender);
			if (nameMaker != null && Rand.Value < xenotype.chanceToUseNameMaker)
			{
				return NameResolvedFrom(nameMaker, forceNoNick);
			}
		}
		if (pawnKindNameMaker != null)
		{
			return NameResolvedFrom(pawnKindNameMaker, forceNoNick);
		}
		if (story != null)
		{
			if (story.Childhood?.nameMaker != null)
			{
				return NameResolvedFrom(story.Childhood.nameMaker, forceNoNick);
			}
			if (story.Adulthood?.nameMaker != null)
			{
				return NameResolvedFrom(story.Adulthood.nameMaker, forceNoNick);
			}
		}
		if (nameGenner != null)
		{
			return new NameSingle(NameGenerator.GenerateName(nameGenner, (string x) => !new NameSingle(x).UsedThisGame));
		}
		RulePackDef rulePackDef = primaryCulture?.GetPawnNameMaker(gender);
		if (rulePackDef != null)
		{
			return NameResolvedFrom(rulePackDef, forceNoNick);
		}
		if (nameCategory != PawnNameCategory.NoName)
		{
			if (Rand.Value < 0.5f)
			{
				NameTriple nameTriple = TryGetRandomUnusedSolidName(gender, forcedLastName, forceNoNick);
				if (nameTriple != null)
				{
					return nameTriple;
				}
			}
			return GeneratePawnName_Shuffled(nameCategory, gender, forcedLastName);
		}
		Log.Error("No name making method for " + genFor);
		return NameTriple.FromString(genFor.label);
	}

	private static Name NameResolvedFrom(RulePackDef nameMaker, bool forceNoNick = false, List<Rule> extraRules = null)
	{
		return NameTriple.FromString(NameGenerator.GenerateName(nameMaker, (string x) => !NameTriple.FromString(x).UsedThisGame, appendNumberIfNameUsed: false, null, null, extraRules), forceNoNick);
	}

	private static NameTriple GeneratePawnName_Shuffled(PawnNameCategory nType, Gender gender, string forcedLastName = null, bool forceNoNick = false)
	{
		if (nType == PawnNameCategory.NoName)
		{
			Log.Message("Can't create a name of type NoName. Defaulting to HumanStandard.");
			nType = PawnNameCategory.HumanStandard;
		}
		NameBank nameBank = PawnNameDatabaseShuffled.BankOf(nType);
		string name = nameBank.GetName(PawnNameSlot.First, gender);
		string text = forcedLastName ?? nameBank.GetName(PawnNameSlot.Last);
		string nick;
		if (!forceNoNick)
		{
			int num = 0;
			do
			{
				num++;
				if (Rand.Value < 0.15f)
				{
					Gender gender2 = gender;
					if (Rand.Value < 0.8f)
					{
						gender2 = Gender.None;
					}
					nick = nameBank.GetName(PawnNameSlot.Nick, gender2);
				}
				else if (Rand.Value < 0.5f)
				{
					nick = name;
				}
				else
				{
					nick = text;
				}
			}
			while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any((Name x) => x is NameTriple nameTriple && nameTriple.Nick == nick));
		}
		else
		{
			nick = null;
		}
		return new NameTriple(name, nick, text);
	}

	private static float BackstorySelectionWeight(BackstoryDef bs)
	{
		return SelectionWeightFactorFromWorkTagsDisabled(bs.workDisables);
	}

	private static float BioSelectionWeight(PawnBio bio)
	{
		return SelectionWeightFactorFromWorkTagsDisabled(bio.adulthood.workDisables | bio.childhood.workDisables);
	}

	private static float SelectionWeightFactorFromWorkTagsDisabled(WorkTags wt)
	{
		float num = 1f;
		if ((wt & WorkTags.ManualDumb) != WorkTags.None)
		{
			num *= 0.5f;
		}
		if ((wt & WorkTags.ManualSkilled) != WorkTags.None)
		{
			num *= 1f;
		}
		if ((wt & WorkTags.Violent) != WorkTags.None)
		{
			num *= 0.6f;
		}
		if ((wt & WorkTags.Social) != WorkTags.None)
		{
			num *= 0.7f;
		}
		if ((wt & WorkTags.Intellectual) != WorkTags.None)
		{
			num *= 0.4f;
		}
		if ((wt & WorkTags.Firefighting) != WorkTags.None)
		{
			num *= 0.8f;
		}
		return num;
	}
}
