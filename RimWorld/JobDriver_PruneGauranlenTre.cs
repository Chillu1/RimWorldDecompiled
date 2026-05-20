using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PruneGauranlenTre : JobDriver
{
	private int numPositions = 1;

	private const TargetIndex TreeIndex = TargetIndex.A;

	private const TargetIndex AdjacentCellIndex = TargetIndex.B;

	private const int DurationTicks = 2500;

	private const int MaxPositions = 8;

	private CompTreeConnection TreeConnection => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompTreeConnection>();

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		float num = TreeConnection.DesiredConnectionStrength - TreeConnection.ConnectionStrength;
		numPositions = Mathf.Min(8, Mathf.CeilToInt(num / TreeConnection.ConnectionStrengthGainPerHourOfPruning) + 1);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		int ticks = Mathf.RoundToInt(2500f / pawn.GetStatValue(StatDefOf.PruningSpeed));
		Toil findAdjacentCell = Toils_General.Do(delegate
		{
			job.targetB = GetAdjacentCell(job.GetTarget(TargetIndex.A).Thing);
		});
		Toil goToAdjacentCell = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell).FailOn(() => TreeConnection.ConnectionStrength >= TreeConnection.DesiredConnectionStrength);
		Toil prune = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: true).WithEffect(EffecterDefOf.Harvest_MetaOnly, TargetIndex.A).WithEffect(EffecterDefOf.GauranlenDebris, TargetIndex.A)
			.PlaySustainerOrSound(SoundDefOf.Interact_Prune);
		prune.AddPreTickIntervalAction(delegate(int delta)
		{
			TreeConnection.Prune(delta);
			pawn.skills?.Learn(SkillDefOf.Plants, 0.085f * (float)delta);
			if (TreeConnection.ConnectionStrength >= TreeConnection.DesiredConnectionStrength)
			{
				ReadyForNextToil();
			}
		});
		prune.activeSkill = () => SkillDefOf.Plants;
		for (int i = 0; i < numPositions; i++)
		{
			yield return findAdjacentCell;
			yield return goToAdjacentCell;
			yield return prune;
		}
	}

	private IntVec3 GetAdjacentCell(Thing treeThing)
	{
		if ((from x in GenAdj.CellsAdjacent8Way(treeThing)
			where x.InBounds(pawn.Map) && !x.Fogged(pawn.Map) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Some)
			select x).TryRandomElement(out var result))
		{
			return result;
		}
		return treeThing.Position;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref numPositions, "numPositions", 1);
	}
}
