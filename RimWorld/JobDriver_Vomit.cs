using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Vomit : JobDriver
	{
		private int ticksLeft;

		public override void SetInitialPosture()
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
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
				ticksLeft = Rand.Range(300, 900);
				int num = 0;
				IntVec3 c;
				do
				{
					c = pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
					num++;
					if (num > 12)
					{
						c = pawn.Position;
						break;
					}
				}
				while (!c.InBounds(pawn.Map) || !c.Standable(pawn.Map));
				job.targetA = c;
				pawn.pather.StopDead();
			};
			toil.tickAction = delegate
			{
				if (ticksLeft % 150 == 149)
				{
					FilthMaker.TryMakeFilth(job.targetA.Cell, base.Map, ThingDefOf.Filth_Vomit, pawn.LabelIndefinite());
					if (pawn.needs.food.CurLevelPercentage > 0.1f)
					{
						pawn.needs.food.CurLevel -= pawn.needs.food.MaxLevel * 0.04f;
					}
				}
				ticksLeft--;
				if (ticksLeft <= 0)
				{
					ReadyForNextToil();
					TaleRecorder.RecordTale(TaleDefOf.Vomited, pawn);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
			toil.PlaySustainerOrSound(() => SoundDefOf.Vomit);
			yield return toil;
		}
	}
}
