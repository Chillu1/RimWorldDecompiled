using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_IdleWhileDespawned : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			AddEndCondition(() => (!pawn.Spawned) ? JobCondition.Ongoing : JobCondition.Succeeded);
			yield return IdleWhileDespawned();
		}

		private static Toil IdleWhileDespawned()
		{
			Toil toil = ToilMaker.MakeToil("IdleWhileDespawned");
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			return toil;
		}

		public override string GetReport()
		{
			Log.ErrorOnce(pawn.ToStringSafe() + " reported job string for IdleWhileDespawned.  An inspectible despawned pawn should have a different job.", 1560125928);
			return base.GetReport();
		}
	}
}
