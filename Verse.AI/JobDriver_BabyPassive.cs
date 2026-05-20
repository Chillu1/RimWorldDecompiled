using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_BabyPassive : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Passive();
		}

		private static Toil Passive()
		{
			Toil toil = ToilMaker.MakeToil("Passive");
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			return toil;
		}
	}
}
