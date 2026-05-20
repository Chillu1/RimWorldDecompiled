using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_Wait : JobDriver
{
	private const int TargetSearchInterval = 4;

	public override string GetReport()
	{
		if (job.def == JobDefOf.Wait_Combat && pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return "ReportStanding".Translate();
		}
		return base.GetReport();
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = (job.forceSleep ? Toils_LayDown.LayDown(TargetIndex.A, hasBed: false, lookForOtherJobs: false) : ToilMaker.MakeToil("MakeNewToils"));
		toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
		{
			base.Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
			pawn.pather?.StopDead();
			CheckForAutoAttack();
		});
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (job.expiryInterval == -1 && job.def == JobDefOf.Wait_Combat && !pawn.Drafted)
			{
				Log.Error(pawn?.ToString() + " in eternal WaitCombat without being drafted.");
				ReadyForNextToil();
			}
			else
			{
				if (job.forceSleep)
				{
					asleep = true;
				}
				if (GenTicks.IsTickIntervalDelta(pawn.thingIDNumber, 4, delta))
				{
					CheckForAutoAttack();
				}
			}
		});
		DecorateWaitToil(toil);
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		if (job.overrideFacing != Rot4.Invalid)
		{
			toil.handlingFacing = true;
			toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
			{
				pawn.rotationTracker.FaceTarget(pawn.Position + job.overrideFacing.FacingCell);
			});
		}
		else if (pawn.mindState != null && pawn.mindState.duty != null && pawn.mindState.duty.focus != null && job.def != JobDefOf.Wait_Combat)
		{
			LocalTargetInfo focusLocal = pawn.mindState.duty.focus;
			toil.handlingFacing = true;
			toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
			{
				pawn.rotationTracker.FaceTarget(focusLocal);
			});
		}
		yield return toil;
	}

	public virtual void DecorateWaitToil(Toil wait)
	{
	}

	public override void Notify_StanceChanged()
	{
		if (pawn.stances.curStance is Stance_Mobile)
		{
			CheckForAutoAttack();
		}
	}

	private void CheckForAutoAttack()
	{
		if (!base.pawn.kindDef.canMeleeAttack || base.pawn.Downed || base.pawn.stances.FullBodyBusy || base.pawn.IsCarryingPawn() || (!base.pawn.IsPlayerControlled && base.pawn.IsPsychologicallyInvisible()) || base.pawn.IsShambler)
		{
			return;
		}
		collideWithPawns = false;
		bool flag = !base.pawn.WorkTagIsDisabled(WorkTags.Violent);
		bool flag2 = base.pawn.RaceProps.ToolUser && base.pawn.Faction == Faction.OfPlayer && !base.pawn.WorkTagIsDisabled(WorkTags.Firefighting);
		if (!(flag || flag2))
		{
			return;
		}
		Fire fire = null;
		for (int i = 0; i < 9; i++)
		{
			IntVec3 c = base.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
			if (!c.InBounds(base.pawn.Map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (flag && base.pawn.kindDef.canMeleeAttack && thingList[j] is Pawn pawn && !pawn.ThreatDisabled(base.pawn) && base.pawn.HostileTo(pawn))
				{
					CompActivity comp = pawn.GetComp<CompActivity>();
					if ((comp == null || comp.IsActive) && !base.pawn.ThreatDisabledBecauseNonAggressiveRoamer(pawn) && GenHostility.IsActiveThreatTo(pawn, base.pawn.Faction, ignoreHives: false))
					{
						base.pawn.meleeVerbs.TryMeleeAttack(pawn);
						collideWithPawns = true;
						return;
					}
				}
				if (flag2 && thingList[j] is Fire fire2 && (fire == null || fire2.fireSize < fire.fireSize || i == 8) && (fire2.parent == null || fire2.parent != base.pawn))
				{
					fire = fire2;
				}
			}
		}
		if (fire != null && (!base.pawn.InMentalState || base.pawn.MentalState.def.allowBeatfire))
		{
			base.pawn.natives.TryBeatFire(fire);
		}
		else
		{
			if (!flag || !job.canUseRangedWeapon || job.def != JobDefOf.Wait_Combat || (base.pawn.drafter != null && !base.pawn.drafter.FireAtWill))
			{
				return;
			}
			Verb currentEffectiveVerb = base.pawn.CurrentEffectiveVerb;
			if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
			{
				TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
				if (currentEffectiveVerb.IsIncendiary_Ranged())
				{
					targetScanFlags |= TargetScanFlags.NeedNonBurning;
				}
				Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(base.pawn, targetScanFlags);
				if (thing != null)
				{
					base.pawn.TryStartAttack(thing);
					collideWithPawns = true;
				}
			}
		}
	}
}
