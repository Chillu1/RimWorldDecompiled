using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Wear : JobDriver
{
	private int duration;

	private int unequipBuffer;

	private const TargetIndex ApparelInd = TargetIndex.A;

	private const TargetIndex ApparelSourceIndex = TargetIndex.B;

	private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.A).Thing;

	private bool TargetIsOnApparelSource
	{
		get
		{
			Apparel apparel = Apparel;
			if (apparel != null && !apparel.Spawned && apparel.ParentHolder is IApparelSource apparelSource)
			{
				return apparelSource is Thing;
			}
			return false;
		}
	}

	private IApparelSource ApparelSource => (IApparelSource)job.GetTarget(TargetIndex.B).Thing;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref duration, "duration", 0);
		Scribe_Values.Look(ref unequipBuffer, "unequipBuffer", 0);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (TargetIsOnApparelSource)
		{
			return pawn.Reserve((Thing)Apparel.ParentHolder, job, 1, -1, null, errorOnFailed);
		}
		return pawn.Reserve(Apparel, job, 1, -1, null, errorOnFailed);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		if (TargetIsOnApparelSource)
		{
			job.targetB = (Thing)Apparel.ParentHolder;
		}
		duration = (int)(Apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
		Apparel apparel = Apparel;
		List<Apparel> wornApparel = pawn.apparel.WornApparel;
		for (int num = wornApparel.Count - 1; num >= 0; num--)
		{
			if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
			{
				duration += (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
			}
		}
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnBurningImmobile(TargetIndex.A);
		bool usingSource = TargetIsOnApparelSource;
		if (usingSource)
		{
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.B);
		}
		else
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		}
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			unequipBuffer += delta;
			TryUnequipSomething();
			pawn.rotationTracker.FaceTarget(Apparel.PositionHeld);
		};
		toil.WithProgressBarToilDelay((!usingSource) ? TargetIndex.A : TargetIndex.B);
		toil.FailOnDespawnedNullOrForbidden((!usingSource) ? TargetIndex.A : TargetIndex.B);
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = duration;
		toil.handlingFacing = true;
		toil.PlaySustainerOrSound(GetCurrentWearSound);
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			Apparel apparel = Apparel;
			if (usingSource)
			{
				ApparelSource.RemoveApparel(apparel);
			}
			pawn.apparel.Wear(apparel);
			if (pawn.outfits != null && job.playerForced)
			{
				pawn.outfits.forcedHandler.SetForced(apparel, forced: true);
			}
		});
	}

	private SoundDef GetCurrentWearSound()
	{
		Apparel apparel = Apparel;
		List<Apparel> wornApparel = pawn.apparel.WornApparel;
		for (int num = wornApparel.Count - 1; num >= 0; num--)
		{
			if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
			{
				if (unequipBuffer >= (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f))
				{
					break;
				}
				return wornApparel[num].def.apparel.soundRemove;
			}
		}
		return apparel.def.apparel.soundWear;
	}

	private void TryUnequipSomething()
	{
		Apparel apparel = Apparel;
		List<Apparel> wornApparel = pawn.apparel.WornApparel;
		for (int num = wornApparel.Count - 1; num >= 0; num--)
		{
			if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
			{
				int num2 = (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
				if (unequipBuffer >= num2)
				{
					bool forbid = pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
					if (!pawn.apparel.TryDrop(wornApparel[num], out var _, pawn.PositionHeld, forbid))
					{
						Log.Error(pawn?.ToString() + " could not drop " + wornApparel[num].ToStringSafe());
						EndJobWith(JobCondition.Errored);
					}
				}
				break;
			}
		}
	}
}
