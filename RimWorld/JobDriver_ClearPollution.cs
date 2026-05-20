using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ClearPollution : JobDriver
{
	private const float WorkAmount = 5600f;

	private const int MaxDistanceToClear = 10;

	private const int MaxDistanceToClearSquared = 100;

	private const int PollutionCellsToClearPerJob = 6;

	private const TargetIndex CleanCellIndex = TargetIndex.A;

	private float workDone;

	private static List<IntVec3> tmpCellsToClear = new List<IntVec3>();

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref workDone, "workDone", 0f);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => !base.TargetLocA.IsPolluted(base.Map));
		this.FailOn(() => !base.Map.areaManager.PollutionClear[base.TargetLocA]);
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			workDone += pawn.GetStatValue(StatDefOf.GeneralLaborSpeed) * (float)delta;
			if (workDone >= 5600f)
			{
				ClearPollutionAt(base.TargetLocA, pawn.Map);
				IntVec3 intVec = CellFinder.FindNoWipeSpawnLocNear(base.TargetLocA, pawn.Map, ThingDefOf.Wastepack, Rot4.North);
				GenSpawn.Spawn(ThingDefOf.Wastepack, intVec.IsValid ? intVec : base.TargetLocA, pawn.Map);
				ReadyForNextToil();
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
		toil.WithProgressBar(TargetIndex.A, () => workDone / 5600f, interpolateBetweenActorAndTarget: true);
		toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		toil.WithEffect(EffecterDefOf.Clean, TargetIndex.A);
		yield return toil;
	}

	private void ClearPollutionAt(IntVec3 root, Map map)
	{
		tmpCellsToClear.Clear();
		tmpCellsToClear.AddRange(map.areaManager.PollutionClear.ActiveCells.Where((IntVec3 c) => c.DistanceToSquared(root) < 100));
		tmpCellsToClear.SortBy((IntVec3 c) => c.DistanceToSquared(root));
		int num = 6;
		for (int num2 = 0; num2 < tmpCellsToClear.Count; num2++)
		{
			if (CanUnpollute(root, map, tmpCellsToClear[num2]))
			{
				tmpCellsToClear[num2].Unpollute(map);
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
		tmpCellsToClear.Clear();
		if (num > 0)
		{
			num -= UnpolluteRadially(root, map, num);
			if (num > 0)
			{
				UnpolluteRadially(root, map, num, ignoreOtherPawnsCleaningCell: true);
			}
		}
	}

	private int UnpolluteRadially(IntVec3 root, Map map, int maxToUnpollute = 5, bool ignoreOtherPawnsCleaningCell = false)
	{
		int num = 0;
		foreach (IntVec3 item in GenRadial.RadialCellsAround(root, 10f, useCenter: true))
		{
			if (CanUnpollute(root, map, item, ignoreOtherPawnsCleaningCell))
			{
				item.Unpollute(map);
				num++;
				if (num >= maxToUnpollute)
				{
					break;
				}
			}
		}
		return num;
	}

	private bool CanUnpollute(IntVec3 root, Map map, IntVec3 c, bool ignoreOtherPawnsCleaningCell = false)
	{
		if (!c.IsPolluted(map))
		{
			return false;
		}
		if (!ignoreOtherPawnsCleaningCell && AnyOtherPawnCleaning(pawn, c))
		{
			return false;
		}
		if (c.GetRoom(map) != null && root.GetRoom(map) != c.GetRoom(map))
		{
			return false;
		}
		if (c.DistanceToSquared(root) > 100)
		{
			return false;
		}
		return true;
	}

	private bool AnyOtherPawnCleaning(Pawn pawn, IntVec3 cell)
	{
		List<Pawn> freeColonistsSpawned = pawn.Map.mapPawns.FreeColonistsSpawned;
		for (int i = 0; i < freeColonistsSpawned.Count; i++)
		{
			if (freeColonistsSpawned[i] != pawn && freeColonistsSpawned[i].CurJobDef == JobDefOf.ClearPollution)
			{
				LocalTargetInfo target = freeColonistsSpawned[i].CurJob.GetTarget(TargetIndex.A);
				if (target.IsValid && target.Cell == cell)
				{
					return true;
				}
			}
		}
		return false;
	}
}
