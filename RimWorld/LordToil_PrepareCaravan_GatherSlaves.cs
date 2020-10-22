using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	[Obsolete]
	public class LordToil_PrepareCaravan_GatherSlaves : LordToil
	{
		private IntVec3 meetingPoint;

		public override float? CustomWakeThreshold => 0.5f;

		public override bool AllowRestingInBed => false;

		public LordToil_PrepareCaravan_GatherSlaves(IntVec3 meetingPoint)
		{
			this.meetingPoint = meetingPoint;
		}

		public override void UpdateAllDuties()
		{
		}
	}
}
