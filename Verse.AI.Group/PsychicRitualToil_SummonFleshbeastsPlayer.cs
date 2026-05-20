using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_SummonFleshbeastsPlayer : PsychicRitualToil
{
	public PsychicRitualRoleDef invokerRole;

	private static readonly IntRange FleshbeastSpawnDelayTicks = new IntRange(180, 180);

	private static readonly IntRange PitBurrowEmergenceDelayRangeTicks = new IntRange(420, 420);

	private const int MaxIterations = 100;

	private const int SpawnRadius = 10;

	private const int MinWalkableCells = 5;

	private static readonly LargeBuildingSpawnParms BurrowSpawnParms = new LargeBuildingSpawnParms
	{
		maxDistanceToColonyBuilding = -1f,
		minDistToEdge = 10,
		attemptSpawnLocationType = SpawnLocationType.Outdoors,
		attemptNotUnderBuildings = true,
		canSpawnOnImpassable = false
	};

	protected PsychicRitualToil_SummonFleshbeastsPlayer()
	{
	}

	public PsychicRitualToil_SummonFleshbeastsPlayer(PsychicRitualRoleDef invokerRole)
	{
		this.invokerRole = invokerRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		if (pawn != null)
		{
			IntVec3 intVec = FindSpawnCenter(psychicRitual, pawn);
			SpawnFleshbeasts(psychicRitual, intVec, pawn);
			Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), "SummonFleshbeastsPlayerCompleteText".Translate(pawn, psychicRitual.def.Named("RITUAL")), LetterDefOf.ThreatBig, new TargetInfo(intVec, psychicRitual.Map));
		}
	}

	private static IntVec3 FindSpawnCenter(PsychicRitual psychicRitual, Pawn invoker)
	{
		IntVec3 invalid = IntVec3.Invalid;
		List<Pawn> list = (from t in psychicRitual.Map.attackTargetsCache.TargetsHostileToColony
			where !t.Thing.Fogged() && t.Thing is Pawn pawn && !t.ThreatDisabled(invoker) && !pawn.IsOnHoldingPlatform
			select t.Thing as Pawn).ToList();
		IEnumerable<IAttackTarget> source = list.Where((Pawn h) => h.RaceProps.Humanlike && !h.IsSubhuman);
		if (!source.Any())
		{
			source = list.Where((Pawn h) => !h.IsSubhuman && !h.RaceProps.IsAnomalyEntity);
			if (!source.Any())
			{
				source = list;
			}
		}
		IAttackTarget[] array = source.ToArray();
		array.Shuffle();
		invalid = TryFindingValidHostileSpawnCenter(psychicRitual, array);
		if (invalid != IntVec3.Invalid)
		{
			return invalid;
		}
		CellFinder.TryFindRandomCell(psychicRitual.Map, (IntVec3 cell) => cell.Walkable(psychicRitual.Map) && !cell.Fogged(psychicRitual.Map) && !psychicRitual.Map.thingGrid.ThingsListAtFast(cell).Any(), out var result);
		return result;
	}

	private static IntVec3 TryFindingValidHostileSpawnCenter(PsychicRitual psychicRitual, IAttackTarget[] hostiles)
	{
		for (int i = 0; i < hostiles.Length && i <= 4; i++)
		{
			int num = 0;
			IntVec3 positionHeld = hostiles[i].Thing.PositionHeld;
			Map map = psychicRitual.Map;
			CellRect cellRect = CellRect.CenteredOn(positionHeld, 10);
			cellRect.ClipInsideMap(map);
			foreach (IntVec3 item in cellRect)
			{
				if (item != positionHeld && item.Walkable(map) && !item.Fogged(map))
				{
					num++;
				}
			}
			if (num >= 5)
			{
				return positionHeld;
			}
		}
		return IntVec3.Invalid;
	}

	private void SpawnFleshbeasts(PsychicRitual psychicRitual, IntVec3 spawnCenter, Pawn invoker)
	{
		float num = ((PsychicRitualDef_SummonFleshbeastsPlayer)psychicRitual.def).fleshbeastCombatPointsFromQualityCurve.Evaluate(psychicRitual.PowerPercent);
		int num2 = 0;
		while (num > 0f)
		{
			IntVec3 intVec = IntVec3.Invalid;
			if (LargeBuildingCellFinder.TryFindCellNear(spawnCenter, psychicRitual.Map, 10, BurrowSpawnParms.ForThing(ThingDefOf.PitBurrow), out var cell) && cell != spawnCenter)
			{
				intVec = cell;
			}
			if (intVec != IntVec3.Invalid)
			{
				float num3 = Mathf.Min(num, 500f);
				num -= num3;
				FleshbeastUtility.SpawnFleshbeastsFromPitBurrowEmergence(intVec, psychicRitual.Map, num3, PitBurrowEmergenceDelayRangeTicks, FleshbeastSpawnDelayTicks, assaultColony: false);
			}
			num2++;
			if (num2 > 100)
			{
				break;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
	}
}
