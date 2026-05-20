using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnGroupMakerUtility
{
	private static readonly SimpleCurve PawnWeightFactorByMostExpensivePawnCostFractionCurve = new SimpleCurve
	{
		new CurvePoint(0.2f, 0.01f),
		new CurvePoint(0.3f, 0.3f),
		new CurvePoint(0.5f, 1f)
	};

	private const float FactionSelectionWeightFactor_RecentlyRaided = 0.4f;

	public static IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, bool warnOnZeroResults = true)
	{
		if (parms.groupKind == null)
		{
			Log.Error($"Tried to generate pawns with null pawn group kind def. parms={parms}");
			yield break;
		}
		if (parms.faction == null)
		{
			Log.Error($"Tried to generate pawn kinds with null faction. parms={parms}");
			yield break;
		}
		if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
		{
			Log.Error($"Faction {parms.faction} of def {parms.faction.def} has no PawnGroupMakers.");
			yield break;
		}
		if (!TryGetRandomPawnGroupMaker(parms, out var pawnGroupMaker, parms.ignoreGroupCommonality))
		{
			Log.Error($"Faction {parms.faction} of def {parms.faction.def} has no usable PawnGroupMakers for parms {parms}");
			yield break;
		}
		foreach (Pawn item in pawnGroupMaker.GeneratePawns(parms, warnOnZeroResults))
		{
			yield return item;
		}
	}

	public static IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms)
	{
		if (parms.groupKind == null)
		{
			Log.Error($"Tried to generate pawn kinds with null pawn group kind def. parms={parms}");
			yield break;
		}
		if (parms.faction == null)
		{
			Log.Error($"Tried to generate pawn kinds with null faction. parms={parms}");
			yield break;
		}
		if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
		{
			Log.Error($"Faction {parms.faction} of def {parms.faction.def} has no PawnGroupMakers.");
			yield break;
		}
		if (!TryGetRandomPawnGroupMaker(parms, out var pawnGroupMaker))
		{
			Log.Error($"Faction {parms.faction} of def {parms.faction.def} has no usable PawnGroupMakers for parms {parms}");
			yield break;
		}
		foreach (PawnKindDef item in pawnGroupMaker.GeneratePawnKindsExample(parms))
		{
			yield return item;
		}
	}

	public static bool TryGetRandomPawnGroupMaker(PawnGroupMakerParms parms, out PawnGroupMaker pawnGroupMaker, bool ignoreCommonality = false)
	{
		if (parms.seed.HasValue)
		{
			Rand.PushState(parms.seed.Value);
		}
		IEnumerable<PawnGroupMaker> source = parms.faction.def.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == parms.groupKind && gm.CanGenerateFrom(parms));
		bool result = ((!ignoreCommonality) ? source.TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out pawnGroupMaker) : source.TryRandomElement(out pawnGroupMaker));
		if (parms.seed.HasValue)
		{
			Rand.PopState();
		}
		return result;
	}

	public static bool PawnGenOptionValid(PawnGenOption o, PawnGroupMakerParms groupParms, List<PawnGenOptionWithXenotype> chosenOptions = null)
	{
		if (groupParms != null)
		{
			if (groupParms.generateFightersOnly && !o.kind.isFighter)
			{
				return false;
			}
			if (groupParms.dontUseSingleUseRocketLaunchers && o.kind.weaponTags != null && o.kind.weaponTags.Contains("GunSingleUse"))
			{
				return false;
			}
			if (groupParms.raidStrategy != null && !groupParms.raidStrategy.Worker.CanUsePawnGenOption(groupParms.points, o, chosenOptions, groupParms.faction))
			{
				return false;
			}
			if (groupParms.raidAgeRestriction != null && groupParms.raidAgeRestriction.Worker.ShouldApplyToKind(o.kind) && !groupParms.raidAgeRestriction.Worker.CanUseKind(o.kind))
			{
				return false;
			}
		}
		if (ModsConfig.BiotechActive && Find.BossgroupManager.ReservedByBossgroup(o.kind))
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && o.kind is CreepJoinerFormKindDef)
		{
			return false;
		}
		if (o.kind.maxPerGroup < int.MaxValue && ChosenKindCount(o.kind) >= o.kind.maxPerGroup)
		{
			return false;
		}
		return true;
		int ChosenKindCount(PawnKindDef d)
		{
			int num = 0;
			if (chosenOptions.NullOrEmpty())
			{
				return num;
			}
			for (int i = 0; i < chosenOptions.Count; i++)
			{
				if (chosenOptions[i].Option.kind == d)
				{
					num++;
				}
			}
			return num;
		}
	}

	public static List<PawnGenOptionWithXenotype> GetOptions(PawnGroupMakerParms groupParms, FactionDef faction, List<PawnGenOption> options, float pointsTotal, float pointsLeft, float? maxCost, List<PawnGenOptionWithXenotype> chosenOptions = null, bool leaderChosen = false)
	{
		List<PawnGenOptionWithXenotype> list = new List<PawnGenOptionWithXenotype>();
		bool flag = ModsConfig.BiotechActive && (faction?.humanlikeFaction ?? true);
		float num = maxCost ?? MaxPawnCost(groupParms?.faction, pointsTotal, groupParms?.raidStrategy, groupParms?.groupKind);
		for (int i = 0; i < options.Count; i++)
		{
			PawnGenOption pawnGenOption = options[i];
			if (flag)
			{
				foreach (KeyValuePair<XenotypeDef, float> item in PawnGenerator.XenotypesAvailableFor(pawnGenOption.kind, faction, groupParms?.faction))
				{
					if (CanUseOption(pawnGenOption, item.Key, groupParms, chosenOptions, pointsLeft, num * item.Key.combatPowerFactor, leaderChosen))
					{
						list.Add(new PawnGenOptionWithXenotype(pawnGenOption, item.Key, pawnGenOption.selectionWeight * item.Value));
					}
				}
			}
			else if (CanUseOption(pawnGenOption, null, groupParms, chosenOptions, pointsLeft, num, leaderChosen))
			{
				list.Add(new PawnGenOptionWithXenotype(pawnGenOption, null, pawnGenOption.selectionWeight));
			}
		}
		return list;
	}

	public static bool AnyOptions(PawnGroupMakerParms groupParms, FactionDef faction, List<PawnGenOption> options, float points)
	{
		bool flag = ModsConfig.BiotechActive && (faction?.humanlikeFaction ?? true);
		for (int i = 0; i < options.Count; i++)
		{
			PawnGenOption pawnGenOption = options[i];
			if (flag)
			{
				foreach (KeyValuePair<XenotypeDef, float> item in PawnGenerator.XenotypesAvailableFor(pawnGenOption.kind, faction, groupParms?.faction))
				{
					if (CanUseOption(pawnGenOption, item.Key, groupParms, null, points, points, leaderChosen: false))
					{
						return true;
					}
				}
			}
			else if (CanUseOption(pawnGenOption, null, groupParms, null, points, points, leaderChosen: false))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CanUseOption(PawnGenOption o, XenotypeDef xenotype, PawnGroupMakerParms groupParms, List<PawnGenOptionWithXenotype> chosenOptions, float pointsLeft, float maxOptionCost, bool leaderChosen)
	{
		float num = o.Cost;
		if (xenotype != null)
		{
			num *= xenotype.combatPowerFactor;
		}
		if (num > pointsLeft)
		{
			return false;
		}
		if (num > maxOptionCost)
		{
			return false;
		}
		if (leaderChosen && o.kind.factionLeader)
		{
			return false;
		}
		if (!PawnGenOptionValid(o, groupParms, chosenOptions))
		{
			return false;
		}
		if (!Find.Storyteller.difficulty.ChildRaidersAllowed && o.kind.pawnGroupDevelopmentStage.HasValue && o.kind.pawnGroupDevelopmentStage.Value != DevelopmentalStage.Adult)
		{
			return false;
		}
		return true;
	}

	public static IEnumerable<PawnGenOptionWithXenotype> ChoosePawnGenOptionsByPoints(float pointsTotal, List<PawnGenOption> options, PawnGroupMakerParms groupParms)
	{
		if (groupParms.seed.HasValue)
		{
			Rand.PushState(groupParms.seed.Value);
		}
		List<PawnGenOptionWithXenotype> list = new List<PawnGenOptionWithXenotype>();
		List<PawnGenOptionWithXenotype> chosenOptions = new List<PawnGenOptionWithXenotype>();
		float num = pointsTotal;
		bool leaderChosen = false;
		float highestCost = -1f;
		while (true)
		{
			list.Clear();
			foreach (PawnGenOptionWithXenotype option in GetOptions(groupParms, groupParms.faction.def, options, pointsTotal, num, null, chosenOptions, leaderChosen))
			{
				if (!(option.Cost > num))
				{
					if (option.Cost > highestCost)
					{
						highestCost = option.Cost;
					}
					list.Add(option);
				}
			}
			Func<PawnGenOptionWithXenotype, float> weightSelector = (PawnGenOptionWithXenotype gr) => (!PawnGenOptionValid(gr.Option, groupParms, chosenOptions)) ? 0f : (gr.SelectionWeight * PawnWeightFactorByMostExpensivePawnCostFractionCurve.Evaluate(gr.Cost / highestCost));
			if (!list.TryRandomElementByWeight(weightSelector, out var result))
			{
				break;
			}
			chosenOptions.Add(result);
			num -= result.Cost;
			if (result.Option.kind.factionLeader)
			{
				leaderChosen = true;
			}
		}
		list.Clear();
		if (chosenOptions.Count == 1 && num > pointsTotal / 2f)
		{
			Log.Warning($"Used only {pointsTotal - num} / {pointsTotal} points generating for {groupParms.faction}");
		}
		if (groupParms.seed.HasValue)
		{
			Rand.PopState();
		}
		return chosenOptions;
	}

	public static float MaxPawnCost(Faction faction, float totalPoints, RaidStrategyDef raidStrategy, PawnGroupKindDef groupKind)
	{
		if (faction == null)
		{
			return totalPoints;
		}
		float a = faction.def.maxPawnCostPerTotalPointsCurve.Evaluate(totalPoints);
		if (raidStrategy != null)
		{
			a = Mathf.Min(a, totalPoints / raidStrategy.minPawns);
		}
		a = Mathf.Max(a, faction.def.MinPointsToGeneratePawnGroup(groupKind) * 1.2f);
		if (raidStrategy != null)
		{
			a = Mathf.Max(a, raidStrategy.Worker.MinMaxAllowedPawnGenOptionCost(faction, groupKind) * 1.2f);
		}
		return a;
	}

	public static bool CanGenerateAnyNormalGroup(Faction faction, float points)
	{
		if (faction.def.pawnGroupMakers == null)
		{
			return false;
		}
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
		pawnGroupMakerParms.faction = faction;
		pawnGroupMakerParms.points = points;
		for (int i = 0; i < faction.def.pawnGroupMakers.Count; i++)
		{
			PawnGroupMaker pawnGroupMaker = faction.def.pawnGroupMakers[i];
			if (pawnGroupMaker.kindDef == PawnGroupKindDefOf.Combat && pawnGroupMaker.CanGenerateFrom(pawnGroupMakerParms))
			{
				return true;
			}
		}
		return false;
	}

	[DebugOutput]
	public static void PawnGroupsMade()
	{
		Dialog_DebugOptionListLister.ShowSimpleDebugMenu(Find.FactionManager.AllFactions.Where((Faction fac) => !fac.def.pawnGroupMakers.NullOrEmpty()), (Faction fac) => fac.Name + " (" + fac.def.defName + ")", delegate(Faction fac)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"FACTION: {fac.Name} ({fac.def.defName}) min={fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)}");
			Action<float> action = delegate(float points)
			{
				if (!(points < fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)))
				{
					PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
					{
						groupKind = PawnGroupKindDefOf.Combat,
						tile = Find.CurrentMap.Tile,
						points = points,
						faction = fac
					};
					sb.AppendLine($"Group with {pawnGroupMakerParms.points} points (max option cost: {MaxPawnCost(fac, points, RaidStrategyDefOf.ImmediateAttack, PawnGroupKindDefOf.Combat)})");
					float num = 0f;
					foreach (Pawn item in from pa in GeneratePawns(pawnGroupMakerParms, warnOnZeroResults: false)
						orderby pa.kindDef.combatPower
						select pa)
					{
						string text = ((item.equipment.Primary == null) ? "no-equipment" : item.equipment.Primary.Label);
						Apparel apparel = item.apparel.FirstApparelOnBodyPartGroup(BodyPartGroupDefOf.Torso);
						string text2 = ((apparel == null) ? "shirtless" : apparel.LabelCap);
						sb.AppendLine("  " + item.kindDef.combatPower.ToString("F0").PadRight(6) + item.kindDef.defName + ", " + text + ", " + text2);
						num += item.kindDef.combatPower;
					}
					sb.AppendLine($"         totalCost {num}");
					sb.AppendLine();
				}
			};
			foreach (float item2 in DebugActionsUtility.PointsOptions(extended: false))
			{
				action(item2);
			}
			Log.Message(sb.ToString());
		});
	}

	private static List<Faction> UsableFactions(float points, Predicate<Faction> validator = null, bool allowNonHostileToPlayer = false, bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true)
	{
		return Find.FactionManager.AllFactions.Where((Faction f) => (allowHidden || !f.Hidden) && !f.temporary && (allowDefeated || !f.defeated) && (allowNonHumanlike || f.def.humanlikeFaction) && (allowNonHostileToPlayer || f.HostileTo(Faction.OfPlayer)) && f.def.pawnGroupMakers != null && f.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat) && !f.def.raidsForbidden && (validator == null || validator(f)) && points >= f.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)).ToList();
	}

	public static bool TryGetRandomFactionForCombatPawnGroup(float points, out Faction faction, Predicate<Faction> validator = null, bool allowNonHostileToPlayer = false, bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true)
	{
		return UsableFactions(points, validator, allowNonHostileToPlayer, allowHidden, allowDefeated, allowNonHumanlike).TryRandomElementByWeight((Faction f) => f.def.RaidCommonalityFromPoints(points), out faction);
	}

	public static bool TryGetRandomFactionForCombatPawnGroupWeighted(IncidentParms parms, out Faction faction, Predicate<Faction> validator = null, bool allowNonHostileToPlayer = false, bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true)
	{
		float maxPoints = parms.points;
		if (maxPoints <= 0f)
		{
			maxPoints = 999999f;
		}
		return UsableFactions(maxPoints, validator, allowNonHostileToPlayer, allowHidden, allowDefeated, allowNonHumanlike).TryRandomElementByWeight(delegate(Faction f)
		{
			float num = 1f;
			if (parms.target != null && f == parms.target.StoryState.lastRaidFaction)
			{
				num = 0.4f;
			}
			return f.def.RaidCommonalityFromPoints(maxPoints) * num;
		}, out faction);
	}
}
