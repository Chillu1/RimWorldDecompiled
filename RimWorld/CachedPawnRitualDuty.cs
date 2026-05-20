using Verse;
using Verse.AI;

namespace RimWorld
{
	public struct CachedPawnRitualDuty
	{
		public DutyDef duty;

		public IntVec3 spot;

		public Thing usedThing;

		public Rot4 overrideFacing;

		public LocalTargetInfo secondFocus;
	}
}
