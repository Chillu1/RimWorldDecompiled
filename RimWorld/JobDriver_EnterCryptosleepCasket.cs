using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_EnterCryptosleepCasket : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		Toil toil = Toils_General.Wait(500);
		toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		toil.WithProgressBarToilDelay(TargetIndex.A);
		yield return toil;
		Toil enter = ToilMaker.MakeToil("MakeNewToils");
		_003C_003Ec__DisplayClass1_0 CS_0024_003C_003E8__locals5;
		enter.initAction = delegate
		{
			Building_CryptosleepCasket pod = (Building_CryptosleepCasket)((Pawn)(object)CS_0024_003C_003E8__locals5).CurJob.targetA.Thing;
			Action action = delegate
			{
				bool flag = ((Thing)(object)CS_0024_003C_003E8__locals5).DeSpawnOrDeselect(DestroyMode.Vanish);
				if (pod.TryAcceptThing((Thing)(object)CS_0024_003C_003E8__locals5) && flag)
				{
					Find.Selector.Select(CS_0024_003C_003E8__locals5, playSound: false, forceDesignatorDeselect: false);
				}
			};
			if (!pod.def.building.isPlayerEjectable)
			{
				if (base.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(CS_0024_003C_003E8__locals5.Named("PAWN")).AdjustedFor((Pawn)(object)CS_0024_003C_003E8__locals5), action));
				}
				else
				{
					action();
				}
			}
			else
			{
				action();
			}
		};
		enter.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return enter;
	}
}
