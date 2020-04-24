using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class AutoUndrafter : IExposable
	{
		private Pawn pawn;

		private int lastNonWaitingTick;

		private const int UndraftDelay = 10000;

		public AutoUndrafter(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastNonWaitingTick, "lastNonWaitingTick", 0);
		}

		public void AutoUndraftTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0 && pawn.Drafted)
			{
				if ((pawn.jobs.curJob != null && pawn.jobs.curJob.def != JobDefOf.Wait_Combat) || AnyHostilePreventingAutoUndraft())
				{
					lastNonWaitingTick = Find.TickManager.TicksGame;
				}
				if (ShouldAutoUndraft())
				{
					pawn.drafter.Drafted = false;
				}
			}
		}

		public void Notify_Drafted()
		{
			lastNonWaitingTick = Find.TickManager.TicksGame;
		}

		private bool ShouldAutoUndraft()
		{
			if (Find.TickManager.TicksGame - lastNonWaitingTick < 10000)
			{
				return false;
			}
			if (AnyHostilePreventingAutoUndraft())
			{
				return false;
			}
			return true;
		}

		private bool AnyHostilePreventingAutoUndraft()
		{
			List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				if (GenHostility.IsActiveThreatToPlayer(potentialTargetsFor[i]))
				{
					return true;
				}
			}
			return false;
		}
	}
}
