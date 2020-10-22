using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
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

		private static readonly BackstoryCategoryFilter FallbackCategoryGroup = new BackstoryCategoryFilter
		{
			categories = new List<string>
			{
				"Civil"
			},
			commonality = 1f
		};

		private static List<string> tmpNames = new List<string>();

		private static HashSet<string> usedNamesTmp = new HashSet<string>();

		public static void GiveAppropriateBioAndNameTo(Pawn pawn, string requiredLastName, FactionDef factionType)
		{
			List<BackstoryCategoryFilter> backstoryCategoryFiltersFor = GetBackstoryCategoryFiltersFor(pawn, factionType);
			if ((!(Rand.Value < 0.25f) && !pawn.kindDef.factionLeader) || !TryGiveSolidBioTo(pawn, requiredLastName, backstoryCategoryFiltersFor))
			{
				GiveShuffledBioTo(pawn, factionType, requiredLastName, backstoryCategoryFiltersFor);
			}
		}

		private static void GiveShuffledBioTo(Pawn pawn, FactionDef factionType, string requiredLastName, List<BackstoryCategoryFilter> backstoryCategories)
		{
			FillBackstorySlotShuffled(pawn, BackstorySlot.Childhood, ref pawn.story.childhood, pawn.story.adulthood, backstoryCategories, factionType);
			if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
			{
				FillBackstorySlotShuffled(pawn, BackstorySlot.Adulthood, ref pawn.story.adulthood, pawn.story.childhood, backstoryCategories, factionType);
			}
			pawn.Name = GeneratePawnName(pawn, NameStyle.Full, requiredLastName);
		}

		private static void FillBackstorySlotShuffled(Pawn pawn, BackstorySlot slot, ref Backstory backstory, Backstory backstoryOtherSlot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType)
		{
			BackstoryCategoryFilter backstoryCategoryFilter = backstoryCategories.RandomElementByWeight((BackstoryCategoryFilter c) => c.commonality);
			if (backstoryCategoryFilter == null)
			{
				backstoryCategoryFilter = FallbackCategoryGroup;
			}
			if (!(from bs in BackstoryDatabase.ShuffleableBackstoryList(slot, backstoryCategoryFilter).TakeRandom(20)
				where (slot != BackstorySlot.Adulthood || !bs.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables)) ? true : false
				select bs).TryRandomElementByWeight(BackstorySelectionWeight, out backstory))
			{
				Log.Error(string.Concat("No shuffled ", slot, " found for ", pawn.ToStringSafe(), " of ", factionType.ToStringSafe(), ". Choosing random."));
				backstory = BackstoryDatabase.allBackstories.Where((KeyValuePair<string, Backstory> kvp) => kvp.Value.slot == slot).RandomElement().Value;
			}
		}

		private static bool TryGiveSolidBioTo(Pawn pawn, string requiredLastName, List<BackstoryCategoryFilter> backstoryCategories)
		{
			if (!TryGetRandomUnusedSolidBioFor(backstoryCategories, pawn.kindDef, pawn.gender, requiredLastName, out var result))
			{
				return false;
			}
			if (result.name.First == "Tynan" && result.name.Last == "Sylvester" && Rand.Value < 0.5f && !TryGetRandomUnusedSolidBioFor(backstoryCategories, pawn.kindDef, pawn.gender, requiredLastName, out result))
			{
				return false;
			}
			pawn.Name = result.name;
			pawn.story.childhood = result.childhood;
			if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
			{
				pawn.story.adulthood = result.adulthood;
			}
			return true;
		}

		private static bool IsBioUseable(PawnBio bio, BackstoryCategoryFilter categoryFilter, PawnKindDef kind, Gender gender, string requiredLastName)
		{
			if (bio.gender != GenderPossibility.Either)
			{
				if (gender == Gender.Male && bio.gender != 0)
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
			if (kind.requiredWorkTags != 0)
			{
				if (bio.childhood != null && (bio.childhood.workDisables & kind.requiredWorkTags) != 0)
				{
					return false;
				}
				if (bio.adulthood != null && (bio.adulthood.workDisables & kind.requiredWorkTags) != 0)
				{
					return false;
				}
			}
			return true;
		}

		private static bool TryGetRandomUnusedSolidBioFor(List<BackstoryCategoryFilter> backstoryCategories, PawnKindDef kind, Gender gender, string requiredLastName, out PawnBio result)
		{
			BackstoryCategoryFilter categoryFilter = backstoryCategories.RandomElementByWeight((BackstoryCategoryFilter c) => c.commonality);
			if (categoryFilter == null)
			{
				categoryFilter = FallbackCategoryGroup;
			}
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

		public static NameTriple TryGetRandomUnusedSolidName(Gender gender, string requiredLastName = null)
		{
			List<NameTriple> listForGender = PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Either);
			List<NameTriple> list = ((gender == Gender.Male) ? PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Male) : PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Female));
			float num = ((float)listForGender.Count + 0.1f) / ((float)(listForGender.Count + list.Count) + 0.1f);
			List<NameTriple> list2 = ((!(Rand.Value < num)) ? list : listForGender);
			if (list2.Count == 0)
			{
				Log.Error(string.Concat("Empty solid pawn name list for gender: ", gender, "."));
				return null;
			}
			if (Rand.Value < 0.5f)
			{
				tmpNames.Clear();
				tmpNames.AddRange(Prefs.PreferredNames);
				tmpNames.Shuffle();
				foreach (string tmpName in tmpNames)
				{
					NameTriple nameTriple = NameTriple.FromString(tmpName);
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
				return (!name.UsedThisGame) ? true : false;
			}).FirstOrDefault();
		}

		private static List<BackstoryCategoryFilter> GetBackstoryCategoryFiltersFor(Pawn pawn, FactionDef faction)
		{
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
			Log.ErrorOnce(string.Concat("PawnKind ", pawn.kindDef, " generating with factionDef ", faction, ": no backstoryCategories in either."), 1871521);
			return new List<BackstoryCategoryFilter>
			{
				FallbackCategoryGroup
			};
		}

		public static Name GeneratePawnName(Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
		{
			switch (style)
			{
			case NameStyle.Full:
			{
				if (pawn.story != null)
				{
					if (pawn.story.childhood != null && pawn.story.childhood.NameMaker != null)
					{
						return NameResolvedFrom(pawn.story.childhood.NameMaker, forcedLastName);
					}
					if (pawn.story.adulthood != null && pawn.story.adulthood.NameMaker != null)
					{
						return NameResolvedFrom(pawn.story.adulthood.NameMaker, forcedLastName);
					}
				}
				RulePackDef nameGenerator = pawn.RaceProps.GetNameGenerator(pawn.gender);
				if (nameGenerator != null)
				{
					return new NameSingle(NameGenerator.GenerateName(nameGenerator, (string x) => !new NameSingle(x).UsedThisGame));
				}
				if (pawn.Faction != null)
				{
					RulePackDef nameMaker = pawn.Faction.def.GetNameMaker(pawn.gender);
					if (nameMaker != null)
					{
						return NameResolvedFrom(nameMaker, forcedLastName);
					}
				}
				if (pawn.RaceProps.nameCategory != 0)
				{
					if (Rand.Value < 0.5f)
					{
						NameTriple nameTriple = TryGetRandomUnusedSolidName(pawn.gender, forcedLastName);
						if (nameTriple != null)
						{
							return nameTriple;
						}
					}
					return GeneratePawnName_Shuffled(pawn, forcedLastName);
				}
				Log.Error("No name making method for " + pawn);
				NameTriple nameTriple2 = NameTriple.FromString(pawn.def.label);
				nameTriple2.ResolveMissingPieces();
				return nameTriple2;
			}
			case NameStyle.Numeric:
				try
				{
					foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
					{
						NameSingle nameSingle = item.Name as NameSingle;
						if (nameSingle != null)
						{
							usedNamesTmp.Add(nameSingle.Name);
						}
					}
					int num = 1;
					string text;
					while (true)
					{
						text = $"{pawn.KindLabel} {num.ToString()}";
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

		private static Name NameResolvedFrom(RulePackDef nameMaker, string forcedLastName)
		{
			NameTriple nameTriple = NameTriple.FromString(NameGenerator.GenerateName(nameMaker, delegate(string x)
			{
				NameTriple nameTriple2 = NameTriple.FromString(x);
				nameTriple2.ResolveMissingPieces(forcedLastName);
				return !nameTriple2.UsedThisGame;
			}));
			nameTriple.CapitalizeNick();
			nameTriple.ResolveMissingPieces(forcedLastName);
			return nameTriple;
		}

		private static NameTriple GeneratePawnName_Shuffled(Pawn pawn, string forcedLastName = null)
		{
			PawnNameCategory pawnNameCategory = pawn.RaceProps.nameCategory;
			if (pawnNameCategory == PawnNameCategory.NoName)
			{
				Log.Message("Can't create a name of type NoName. Defaulting to HumanStandard.");
				pawnNameCategory = PawnNameCategory.HumanStandard;
			}
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(pawnNameCategory);
			string name = nameBank.GetName(PawnNameSlot.First, pawn.gender);
			string text = ((forcedLastName == null) ? nameBank.GetName(PawnNameSlot.Last) : forcedLastName);
			int num = 0;
			string nick;
			do
			{
				num++;
				if (Rand.Value < 0.15f)
				{
					Gender gender = pawn.gender;
					if (Rand.Value < 0.8f)
					{
						gender = Gender.None;
					}
					nick = nameBank.GetName(PawnNameSlot.Nick, gender);
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
			while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any(delegate(Name x)
			{
				NameTriple nameTriple = x as NameTriple;
				return nameTriple != null && nameTriple.Nick == nick;
			}));
			return new NameTriple(name, nick, text);
		}

		private static float BackstorySelectionWeight(Backstory bs)
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
			if ((wt & WorkTags.ManualDumb) != 0)
			{
				num *= 0.5f;
			}
			if ((wt & WorkTags.ManualSkilled) != 0)
			{
				num *= 1f;
			}
			if ((wt & WorkTags.Violent) != 0)
			{
				num *= 0.6f;
			}
			if ((wt & WorkTags.Social) != 0)
			{
				num *= 0.7f;
			}
			if ((wt & WorkTags.Intellectual) != 0)
			{
				num *= 0.4f;
			}
			if ((wt & WorkTags.Firefighting) != 0)
			{
				num *= 0.8f;
			}
			return num;
		}
	}
}
