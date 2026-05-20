using Verse;

namespace RimWorld
{
	[DefOf]
	public static class MechWorkModeDefOf
	{
		[MayRequireBiotech]
		public static MechWorkModeDef Work;

		[MayRequireBiotech]
		public static MechWorkModeDef SelfShutdown;

		[MayRequireBiotech]
		public static MechWorkModeDef Escort;

		[MayRequireBiotech]
		public static MechWorkModeDef Recharge;
	}
}
