using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_DropEquipment : JobDriver
{
	private const int DurationTicks = 30;

	private ThingWithComps TargetEquipment => (ThingWithComps)base.TargetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			pawn.pather.StopDead();
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 30;
		yield return toil;
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			if (!pawn.equipment.TryDropEquipment(TargetEquipment, out var _, pawn.Position))
			{
				EndJobWith(JobCondition.Incompletable);
			}
		};
		yield return toil2;
	}
}
