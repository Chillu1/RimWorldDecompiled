using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public static class CreepJoinerUtility
{
	private static readonly List<CreepJoinerBaseDef> requires = new List<CreepJoinerBaseDef>();

	private static readonly List<CreepJoinerBaseDef> exclude = new List<CreepJoinerBaseDef>();

	private static readonly List<ICreepJoinerDef> temp = new List<ICreepJoinerDef>();

	public static void GetCreepjoinerSpecifics(Map map, ref CreepJoinerFormKindDef form, ref CreepJoinerBenefitDef benefit, ref CreepJoinerDownsideDef downside, ref CreepJoinerAggressiveDef aggressive, ref CreepJoinerRejectionDef rejection)
	{
		float combatPoints = StorytellerUtility.DefaultThreatPointsNow(map);
		if (form == null)
		{
			form = DefDatabase<CreepJoinerFormKindDef>.AllDefsListForReading.RandomElementByWeight((CreepJoinerFormKindDef x) => x.Weight);
		}
		requires.AddRange(form.Requires);
		exclude.AddRange(form.Excludes);
		if (benefit == null)
		{
			benefit = GetRandom(DefDatabase<CreepJoinerBenefitDef>.AllDefsListForReading, combatPoints, requires, exclude);
		}
		if (downside == null)
		{
			downside = GetRandom(DefDatabase<CreepJoinerDownsideDef>.AllDefsListForReading, combatPoints, requires, exclude);
		}
		if (aggressive == null)
		{
			aggressive = GetRandom(DefDatabase<CreepJoinerAggressiveDef>.AllDefsListForReading, combatPoints, requires, exclude);
		}
		if (rejection == null)
		{
			rejection = GetRandom(DefDatabase<CreepJoinerRejectionDef>.AllDefsListForReading, combatPoints, requires, exclude);
		}
		exclude.Clear();
		requires.Clear();
	}

	public static Pawn GenerateAndSpawn(Map map, float combatPoints)
	{
		CreepJoinerFormKindDef creepJoinerFormKindDef = DefDatabase<CreepJoinerFormKindDef>.AllDefsListForReading.RandomElementByWeight((CreepJoinerFormKindDef x) => x.Weight);
		requires.AddRange(creepJoinerFormKindDef.Requires);
		exclude.AddRange(creepJoinerFormKindDef.Excludes);
		CreepJoinerBenefitDef random = GetRandom(DefDatabase<CreepJoinerBenefitDef>.AllDefsListForReading, combatPoints, requires, exclude);
		CreepJoinerDownsideDef random2 = GetRandom(DefDatabase<CreepJoinerDownsideDef>.AllDefsListForReading, combatPoints, requires, exclude);
		CreepJoinerAggressiveDef random3 = GetRandom(DefDatabase<CreepJoinerAggressiveDef>.AllDefsListForReading, combatPoints, requires, exclude);
		CreepJoinerRejectionDef random4 = GetRandom(DefDatabase<CreepJoinerRejectionDef>.AllDefsListForReading, combatPoints, requires, exclude);
		exclude.Clear();
		requires.Clear();
		return GenerateAndSpawn(creepJoinerFormKindDef, random, random2, random3, random4, map);
	}

	public static Pawn GenerateAndSpawn(CreepJoinerFormKindDef form, CreepJoinerBenefitDef benefit, CreepJoinerDownsideDef downside, CreepJoinerAggressiveDef aggressive, CreepJoinerRejectionDef rejection, Map map)
	{
		PawnGenerationRequest request = new PawnGenerationRequest(form, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true);
		request.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
		request.ForceGenerateNewPawn = true;
		request.AllowFood = true;
		request.DontGiveWeapon = true;
		request.OnlyUseForcedBackstories = form.fixedAdultBackstories.Any();
		request.MaximumAgeTraits = 1;
		request.MinimumAgeTraits = 1;
		request.IsCreepJoiner = true;
		request.ForceNoIdeoGear = true;
		request.MustBeCapableOfViolence = true;
		request.CanGeneratePawnRelations = false;
		Pawn pawn = PawnGenerator.GeneratePawn(request);
		Pawn_CreepJoinerTracker creepjoiner = pawn.creepjoiner;
		creepjoiner.form = form;
		creepjoiner.benefit = benefit;
		creepjoiner.downside = downside;
		creepjoiner.aggressive = aggressive;
		creepjoiner.rejection = rejection;
		ApplyExtraTraits(pawn, benefit.traits);
		ApplyExtraTraits(pawn, downside.traits);
		ApplyExtraHediffs(pawn, benefit.hediffs);
		ApplyExtraHediffs(pawn, downside.hediffs);
		ApplySkillOverrides(pawn, benefit.skills);
		ApplyExtraAbilities(pawn, benefit.abilities);
		ApplyExtraAbilities(pawn, downside.abilities);
		pawn.guest.Recruitable = false;
		if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Friendly, allowFogged: false, (IntVec3 cell) => map.reachability.CanReachMapEdge(cell, TraverseParms.For(TraverseMode.PassDoors))))
		{
			return null;
		}
		GenSpawn.Spawn(pawn, result, map);
		if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out var result2))
		{
			return null;
		}
		LordMaker.MakeNewLord(pawn.Faction, new LordJob_CreepJoiner(result2, pawn), map).AddPawn(pawn);
		creepjoiner.Notify_Created();
		return pawn;
	}

	private static void ApplySkillOverrides(Pawn pawn, List<CreepJoinerBenefitDef.SkillValue> skills)
	{
		foreach (CreepJoinerBenefitDef.SkillValue skill2 in skills)
		{
			SkillRecord skill = pawn.skills.GetSkill(skill2.skill);
			skill.Level = skill2.range.RandomInRange;
			skill.xpSinceMidnight = 0f;
			skill.xpSinceLastLevel = 0f;
		}
	}

	private static void ApplyExtraTraits(Pawn pawn, List<BackstoryTrait> traits)
	{
		foreach (BackstoryTrait trait in traits)
		{
			if (!pawn.story.traits.HasTrait(trait.def))
			{
				pawn.story.traits.GainTrait(new Trait(trait.def, trait.degree, forced: true));
			}
		}
	}

	private static void ApplyExtraHediffs(Pawn pawn, List<HediffDef> hediffs)
	{
		foreach (HediffDef hediff in hediffs)
		{
			pawn.health.AddHediff(hediff);
		}
	}

	private static void ApplyExtraAbilities(Pawn pawn, List<AbilityDef> abilities)
	{
		foreach (AbilityDef ability in abilities)
		{
			pawn.abilities.GainAbility(ability);
		}
	}

	public static T GetRandom<T>(List<T> defs, float combatPoints, List<CreepJoinerBaseDef> requires, List<CreepJoinerBaseDef> exclude) where T : CreepJoinerBaseDef
	{
		T val;
		if (requires.Empty() && exclude.Empty())
		{
			val = defs.Where((T x) => combatPoints >= x.MinCombatPoints && x.CanOccurRandomly).RandomElementByWeight((T x) => x.Weight);
		}
		else
		{
			bool flag = false;
			foreach (T def in defs)
			{
				if (!(combatPoints < def.MinCombatPoints) && def.CanOccurRandomly && requires.Contains(def))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				foreach (T def2 in defs)
				{
					if (!(combatPoints < def2.MinCombatPoints) && def2.CanOccurRandomly && requires.Contains(def2))
					{
						temp.Add(def2);
					}
				}
			}
			else
			{
				foreach (T def3 in defs)
				{
					if (combatPoints >= def3.MinCombatPoints && def3.CanOccurRandomly)
					{
						temp.Add(def3);
					}
				}
			}
			for (int num = temp.Count - 1; num >= 0; num--)
			{
				if (exclude.Contains(temp[num]))
				{
					defs.RemoveAt(num);
				}
			}
			if (temp.Empty())
			{
				string text = defs.Select((T x) => x.label).ToCommaList();
				string text2 = requires.Select((CreepJoinerBaseDef x) => x.label).ToCommaList();
				string text3 = exclude.Select((CreepJoinerBaseDef x) => x.label).ToCommaList();
				Log.Error($"Attempted to create creepjoiner but blacklist removed all possible whitelist; combatPoints = {combatPoints}, defs = ({text}), whitelist = ({text2}), blacklist = ({text3})");
				val = defs.RandomElementByWeight((T x) => x.Weight);
			}
			else
			{
				val = temp.RandomElementByWeight((ICreepJoinerDef x) => x.Weight) as T;
			}
			temp.Clear();
		}
		exclude.AddRange(val.Excludes);
		requires.AddRange(val.Requires);
		return val;
	}
}
