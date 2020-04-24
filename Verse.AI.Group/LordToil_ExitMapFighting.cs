using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_ExitMapFighting : LordToil_ExitMap
	{
		public override DutyDef ExitDuty => DutyDefOf.ExitMapBestAndDefendSelf;

		public LordToil_ExitMapFighting(LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false, bool interruptCurrentJob = false)
			: base(locomotion, canDig, interruptCurrentJob)
		{
		}
	}
}
