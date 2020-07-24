using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Wear : JobDriver
	{
		private int duration;

		private int unequipBuffer;

		private const TargetIndex ApparelInd = TargetIndex.A;

		private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.A).Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref duration, "duration", 0);
			Scribe_Values.Look(ref unequipBuffer, "unequipBuffer", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Apparel, job, 1, -1, null, errorOnFailed);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
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
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				unequipBuffer++;
				TryUnequipSomething();
			};
			toil.WithProgressBarToilDelay(TargetIndex.A);
			toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = duration;
			yield return toil;
			yield return Toils_General.Do(delegate
			{
				Apparel apparel = Apparel;
				pawn.apparel.Wear(apparel);
				if (pawn.outfits != null && job.playerForced)
				{
					pawn.outfits.forcedHandler.SetForced(apparel, forced: true);
				}
			});
		}

		private void TryUnequipSomething()
		{
			Apparel apparel = Apparel;
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			int num = wornApparel.Count - 1;
			while (true)
			{
				if (num >= 0)
				{
					if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
					{
						break;
					}
					num--;
					continue;
				}
				return;
			}
			int num2 = (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
			if (unequipBuffer >= num2)
			{
				bool forbid = pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
				if (!pawn.apparel.TryDrop(wornApparel[num], out Apparel _, pawn.PositionHeld, forbid))
				{
					Log.Error(string.Concat(pawn, " could not drop ", wornApparel[num].ToStringSafe()));
					EndJobWith(JobCondition.Errored);
				}
			}
		}
	}
}
