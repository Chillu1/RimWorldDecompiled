using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_SummonFleshbeastsCultist : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	private static readonly IntRange FleshbeastSpawnDelayTicks = new IntRange(180, 180);

	private static readonly IntRange PitBurrowEmergenceDelayRangeTicks = new IntRange(420, 420);

	private const int MaxIterationsGuard = 200;

	private static readonly LargeBuildingSpawnParms BurrowSpawnParms = new LargeBuildingSpawnParms
	{
		maxDistanceToColonyBuilding = -1f,
		minDistToEdge = 10,
		canSpawnOnImpassable = false
	};

	private static List<Pawn> tmpPawns = new List<Pawn>();

	protected PsychicRitualToil_SummonFleshbeastsCultist()
	{
	}

	public PsychicRitualToil_SummonFleshbeastsCultist(PsychicRitualRoleDef invokerRole)
	{
		this.invokerRole = invokerRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		ApplyOutcome(psychicRitual);
	}

	private void ApplyOutcome(PsychicRitual psychicRitual)
	{
		psychicRitual.def.CalculateMaxPower(psychicRitual.assignments, null, out var power);
		float num = (psychicRitual.power = ((PsychicRitualDef_SummonFleshbeasts)psychicRitual.def).fleshbeastPointsFromThreatPointsCurve.Evaluate(power));
		int num2 = 0;
		List<Thing> list = new List<Thing>();
		while (num > 0f)
		{
			if (!TryFindBurrowCell(psychicRitual.Map, out var cell) && !CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => x.Walkable(psychicRitual.Map), psychicRitual.Map, out cell))
			{
				num2++;
				if (num2 > 200)
				{
					break;
				}
				continue;
			}
			float num3 = Mathf.Min(num, 500f);
			num -= num3;
			list.Add(FleshbeastUtility.SpawnFleshbeastsFromPitBurrowEmergence(cell, psychicRitual.Map, num3, PitBurrowEmergenceDelayRangeTicks, FleshbeastSpawnDelayTicks));
			num2++;
			if (num2 > 200)
			{
				break;
			}
		}
		if (list.Count > 0 && !psychicRitual.def.letterAICompleteLabel.NullOrEmpty() && !psychicRitual.def.letterAICompleteText.NullOrEmpty())
		{
			Find.LetterStack.ReceiveLetter(psychicRitual.def.letterAICompleteLabel, psychicRitual.def.letterAICompleteText, LetterDefOf.ThreatBig, list);
		}
	}

	private bool TryFindBurrowCell(Map map, out IntVec3 cell)
	{
		tmpPawns.Clear();
		tmpPawns.AddRange(map.mapPawns.FreeColonistsSpawned);
		tmpPawns.Shuffle();
		LargeBuildingSpawnParms parms = BurrowSpawnParms.ForThing(ThingDefOf.PitBurrow);
		foreach (Pawn tmpPawn in tmpPawns)
		{
			if (LargeBuildingCellFinder.TryFindCellNear(tmpPawn.Position, map, 6, parms, out cell))
			{
				return true;
			}
		}
		cell = IntVec3.Invalid;
		return false;
	}

	public override void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		foreach (Pawn allAssignedPawn in psychicRitual.assignments.AllAssignedPawns)
		{
			SetPawnDuty(allAssignedPawn, psychicRitual, parent, DutyDefOf.Idle);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
	}
}
