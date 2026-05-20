using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_NociosphereDepart : JobDriver
{
	private static readonly int LeaveTimeTicks = 720;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = Toils_General.Wait(LeaveTimeTicks);
		toil.WithEffect(EffecterDefOf.NociosphereDeparting, TargetIndex.A);
		toil.PlaySustainerOrSound(SoundDefOf.Pawn_Nociosphere_Departing);
		Toil toil2 = toil;
		toil2.initAction = (Action)Delegate.Combine(toil2.initAction, (Action)delegate
		{
			Messages.Message(toil.actor.GetComp<CompNociosphere>().Props.departingMessage, toil.actor, MessageTypeDefOf.NeutralEvent);
		});
		yield return toil;
		Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
		toil3.initAction = (Action)Delegate.Combine(toil3.initAction, (Action)delegate
		{
			toil.actor.GetComp<CompActivity>()?.EnterPassiveState();
		});
		toil3.AddFinishAction(delegate
		{
			EffecterDefOf.Skip_Entry.SpawnMaintained(toil.actor.PositionHeld, toil.actor.MapHeld, 2f);
			EffecterDefOf.NociosphereDepartComplete.Spawn(toil.actor.PositionHeld, toil.actor.MapHeld).Cleanup();
		});
		yield return toil3;
	}
}
