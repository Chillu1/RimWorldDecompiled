using RimWorld;
using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_Wait : JobDriver
	{
		private const int TargetSearchInterval = 4;

		public override string GetReport()
		{
			if (job.def == JobDefOf.Wait_Combat)
			{
				if (pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
				{
					return "ReportStanding".Translate();
				}
				return base.GetReport();
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				base.Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
				pawn.pather.StopDead();
				CheckForAutoAttack();
			};
			toil.tickAction = delegate
			{
				if (job.expiryInterval == -1 && job.def == JobDefOf.Wait_Combat && !pawn.Drafted)
				{
					Log.Error(string.Concat(pawn, " in eternal WaitCombat without being drafted."));
					ReadyForNextToil();
				}
				else if ((Find.TickManager.TicksGame + pawn.thingIDNumber) % 4 == 0)
				{
					CheckForAutoAttack();
				}
			};
			DecorateWaitToil(toil);
			toil.defaultCompleteMode = ToilCompleteMode.Never;
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
			if (base.pawn.Downed || base.pawn.stances.FullBodyBusy)
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
					if (flag)
					{
						Pawn pawn = thingList[j] as Pawn;
						if (pawn != null && !pawn.Downed && base.pawn.HostileTo(pawn) && GenHostility.IsActiveThreatTo(pawn, base.pawn.Faction))
						{
							base.pawn.meleeVerbs.TryMeleeAttack(pawn);
							collideWithPawns = true;
							return;
						}
					}
					if (flag2)
					{
						Fire fire2 = thingList[j] as Fire;
						if (fire2 != null && (fire == null || fire2.fireSize < fire.fireSize || i == 8) && (fire2.parent == null || fire2.parent != base.pawn))
						{
							fire = fire2;
						}
					}
				}
			}
			if (fire != null && (!base.pawn.InMentalState || base.pawn.MentalState.def.allowBeatfire))
			{
				base.pawn.natives.TryBeatFire(fire);
			}
			else
			{
				if (!flag || !job.canUseRangedWeapon || base.pawn.Faction == null || job.def != JobDefOf.Wait_Combat || (base.pawn.drafter != null && !base.pawn.drafter.FireAtWill))
				{
					return;
				}
				Verb currentEffectiveVerb = base.pawn.CurrentEffectiveVerb;
				if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
				{
					TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
					if (currentEffectiveVerb.IsIncendiary())
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
}
