using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_TalkCreepJoiner : JobDriver
{
	private bool notified;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(base.TargetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
		{
			if (!notified)
			{
				notified = true;
				base.TargetPawnA.GetLord()?.ReceiveMemo("SpokenTo");
			}
		});
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.Do(delegate
		{
			base.TargetPawnA.creepjoiner.Notify_CreepJoinerSpokenTo(pawn);
		});
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref notified, "notified", defaultValue: false);
	}
}
