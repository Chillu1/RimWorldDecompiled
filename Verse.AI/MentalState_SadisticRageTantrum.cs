using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalState_SadisticRageTantrum : MentalState_TantrumRandom
	{
		private int hits;

		public const int MaxHits = 7;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref hits, "hits", 0);
		}

		protected override void GetPotentialTargets(List<Thing> outThings)
		{
			TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, outThings, GetCustomValidator());
		}

		protected override Predicate<Thing> GetCustomValidator()
		{
			return (Thing x) => TantrumMentalStateUtility.CanAttackPrisoner(pawn, x);
		}

		public override void Notify_AttackedTarget(LocalTargetInfo hitTarget)
		{
			base.Notify_AttackedTarget(hitTarget);
			if (target != null && hitTarget.Thing == target)
			{
				hits++;
				if (hits >= 7)
				{
					RecoverFromState();
				}
			}
		}
	}
}
