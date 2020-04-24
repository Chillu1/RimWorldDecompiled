using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_IdleJoy : JobGiver_GetJoy
	{
		private const int GameStartNoIdleJoyTicks = 60000;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.needs.joy == null)
			{
				return null;
			}
			if (Find.TickManager.TicksGame < 60000)
			{
				return null;
			}
			if (JoyUtility.LordPreventsGettingJoy(pawn) || JoyUtility.TimetablePreventsGettingJoy(pawn))
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}
	}
}
