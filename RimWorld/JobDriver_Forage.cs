using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Forage : JobDriver
{
	private const TargetIndex SpotInd = TargetIndex.A;

	private const int ForageTicks = 240;

	private static readonly IntRange StackCountRange = new IntRange(10, 25);

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => !job.GetTarget(TargetIndex.A).Cell.GetTerrain(pawn.Map).natural);
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = Toils_General.WaitWith(TargetIndex.A, 240, useProgressBar: true, maintainPosture: true);
		toil.WithEffect(EffecterDefOf.BuryPawn, () => job.GetTarget(TargetIndex.A));
		yield return toil;
		yield return CompleteForageToil();
	}

	private Toil CompleteForageToil()
	{
		Toil toil = ToilMaker.MakeToil("CompleteForageToil");
		toil.initAction = delegate
		{
			Thing thing = ThingMaker.MakeThing(pawn.Map.Biome.foragedFood);
			thing.stackCount = StackCountRange.RandomInRange;
			GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
			Messages.Message("MessageAnimalForagedFood".Translate(pawn.Named("ANIMAL"), thing.Named("FOOD")), thing, MessageTypeDefOf.PositiveEvent);
		};
		return toil;
	}
}
