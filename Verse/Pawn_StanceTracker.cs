using RimWorld;

namespace Verse
{
	public class Pawn_StanceTracker : IExposable
	{
		public Pawn pawn;

		public Stance curStance = new Stance_Mobile();

		private int staggerUntilTick = -1;

		public StunHandler stunner;

		public const int StaggerMeleeAttackTicks = 95;

		public const int StaggerBulletImpactTicks = 95;

		public const int StaggerExplosionImpactTicks = 95;

		public bool debugLog;

		public bool FullBodyBusy
		{
			get
			{
				if (!stunner.Stunned)
				{
					return curStance.StanceBusy;
				}
				return true;
			}
		}

		public bool Staggered => Find.TickManager.TicksGame < staggerUntilTick;

		public Pawn_StanceTracker(Pawn newPawn)
		{
			pawn = newPawn;
			stunner = new StunHandler(pawn);
		}

		public void StanceTrackerTick()
		{
			stunner.StunHandlerTick();
			if (!stunner.Stunned)
			{
				curStance.StanceTick();
			}
		}

		public void StanceTrackerDraw()
		{
			curStance.StanceDraw();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref staggerUntilTick, "staggerUntilTick", 0);
			Scribe_Deep.Look(ref stunner, "stunner", pawn);
			Scribe_Deep.Look(ref curStance, "curStance");
			if (Scribe.mode == LoadSaveMode.LoadingVars && curStance != null)
			{
				curStance.stanceTracker = this;
			}
		}

		public void StaggerFor(int ticks)
		{
			staggerUntilTick = Find.TickManager.TicksGame + ticks;
		}

		public void CancelBusyStanceSoft()
		{
			if (curStance is Stance_Warmup)
			{
				SetStance(new Stance_Mobile());
			}
		}

		public void CancelBusyStanceHard()
		{
			SetStance(new Stance_Mobile());
		}

		public void SetStance(Stance newStance)
		{
			if (debugLog)
			{
				Log.Message(Find.TickManager.TicksGame + " " + pawn + " SetStance " + curStance + " -> " + newStance);
			}
			newStance.stanceTracker = this;
			curStance = newStance;
			if (pawn.jobs.curDriver != null)
			{
				pawn.jobs.curDriver.Notify_StanceChanged();
			}
		}

		public void Notify_DamageTaken(DamageInfo dinfo)
		{
		}
	}
}
