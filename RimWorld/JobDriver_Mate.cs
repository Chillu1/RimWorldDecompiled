using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Mate : JobDriver
{
	private const int MateDuration = 500;

	private const TargetIndex FemInd = TargetIndex.A;

	public const int TicksBetweenHeartMotes = 100;

	private Pawn Female => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnDowned(TargetIndex.A);
		this.FailOnNotCasualInterruptible(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		Toil toil = Toils_General.WaitWith(TargetIndex.A, 500);
		toil.tickIntervalAction = delegate(int delta)
		{
			if (pawn.IsHashIntervalTick(100, delta))
			{
				FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
			}
			if (Female.IsHashIntervalTick(100, delta))
			{
				FleckMaker.ThrowMetaIcon(Female.Position, pawn.Map, FleckDefOf.Heart);
			}
		};
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			PawnUtility.Mated(pawn, Female);
		});
	}
}
