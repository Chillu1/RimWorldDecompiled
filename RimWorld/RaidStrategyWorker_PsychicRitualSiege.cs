using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RaidStrategyWorker_PsychicRitualSiege : RaidStrategyWorker_WithRequiredPawnKinds
{
	private static readonly IntRange PsychicRitualIterationsCountRange = new IntRange(3, 5);

	public override void MakeLords(IncidentParms parms, List<Pawn> pawns)
	{
		Map map = (Map)parms.target;
		Pawn pawn = BestInvoker(pawns);
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(pawn);
		}
		parms.spawnCenter = RCellFinder.FindSiegePositionFrom(parms.spawnCenter.IsValid ? parms.spawnCenter : pawn.PositionHeld, map, allowRoofed: true, errorOnFail: true, Validator, requireBuildableTerrain: false);
		Lord lord = LordMaker.MakeNewLord(parms.faction, MakeLordJob(parms, map, pawns, Rand.Int), map, pawns);
		lord.inSignalLeave = parms.inSignalEnd;
		QuestUtility.AddQuestTag(lord, parms.questTag);
		pawn.health.AddHediff(HediffDefOf.PsychicTrance);
		bool Validator(IntVec3 x)
		{
			using (IEnumerator<IntVec3> enumerator = CellRect.CenteredOn(x, 6).ClipInsideMap(map).Cells.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IntVec3 current = enumerator.Current;
					if (!current.Walkable(map))
					{
						return false;
					}
					return current.GetFirstBuilding(map)?.IsClearableFreeBuilding ?? true;
				}
			}
			return true;
		}
	}

	protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
	{
		Pawn pawn = BestInvoker(pawns);
		PsychicRitualRoleAssignments psychicRitualRoleAssignments = parms.psychicRitualDef.BuildRoleAssignments(new TargetInfo(parms.spawnCenter, map));
		psychicRitualRoleAssignments.AddForcedRole(pawn, PsychicRitualRoleDefOf.Invoker, PsychicRitualRoleDef.Context.NonPlayerFaction);
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i] != pawn)
			{
				psychicRitualRoleAssignments.AddForcedRole(pawns[i], PsychicRitualRoleDefOf.Defender, PsychicRitualRoleDef.Context.NonPlayerFaction);
			}
		}
		return new LordJob_PsychicRitualRepeating(parms.psychicRitualDef, psychicRitualRoleAssignments, PsychicRitualIterationsCountRange.RandomInRange, parms.spawnCenter, parms.points);
	}

	private Pawn BestInvoker(List<Pawn> pawns)
	{
		Pawn pawn = pawns[0];
		for (int i = 1; i < pawns.Count; i++)
		{
			if (!(pawns[i].GetStatValue(StatDefOf.PsychicSensitivity) <= 0f) && pawns[i].kindDef.isGoodPsychicRitualInvoker && pawns[i].kindDef.combatPower > pawn.kindDef.combatPower)
			{
				pawn = pawns[i];
			}
		}
		return pawn;
	}

	public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
	{
		if (!base.CanUseWith(parms, groupKind))
		{
			return false;
		}
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!parms.faction.def.canPsychicRitualSiege)
		{
			return false;
		}
		return true;
	}

	public override bool CanUsePawn(float pointsTotal, Pawn p, List<Pawn> otherPawns)
	{
		if (otherPawns.Count == 0)
		{
			if (p.kindDef.isGoodPsychicRitualInvoker)
			{
				return p.GetStatValue(StatDefOf.PsychicSensitivity) > 0f;
			}
			return false;
		}
		return true;
	}

	protected override bool MatchesRequiredPawnKind(PawnKindDef kind)
	{
		return kind.isGoodPsychicRitualInvoker;
	}

	protected override int MinRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
	{
		return 1;
	}
}
