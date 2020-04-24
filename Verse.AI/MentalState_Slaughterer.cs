using RimWorld;

namespace Verse.AI
{
	public class MentalState_Slaughterer : MentalState
	{
		private int lastSlaughterTicks = -1;

		private int animalsSlaughtered;

		private const int NoAnimalToSlaughterCheckInterval = 600;

		private const int MinTicksBetweenSlaughter = 3750;

		private const int MaxAnimalsSlaughtered = 4;

		public bool SlaughteredRecently
		{
			get
			{
				if (lastSlaughterTicks >= 0)
				{
					return Find.TickManager.TicksGame - lastSlaughterTicks < 3750;
				}
				return false;
			}
		}

		protected override bool CanEndBeforeMaxDurationNow => lastSlaughterTicks >= 0;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastSlaughterTicks, "lastSlaughterTicks", 0);
			Scribe_Values.Look(ref animalsSlaughtered, "animalsSlaughtered", 0);
		}

		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (pawn.IsHashIntervalTick(600) && (pawn.CurJob == null || pawn.CurJob.def != JobDefOf.Slaughter) && SlaughtererMentalStateUtility.FindAnimal(pawn) == null)
			{
				RecoverFromState();
			}
		}

		public override void Notify_SlaughteredAnimal()
		{
			lastSlaughterTicks = Find.TickManager.TicksGame;
			animalsSlaughtered++;
			if (animalsSlaughtered >= 4)
			{
				RecoverFromState();
			}
		}
	}
}
