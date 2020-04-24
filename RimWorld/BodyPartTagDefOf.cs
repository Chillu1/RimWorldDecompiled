using Verse;

namespace RimWorld
{
	[DefOf]
	public static class BodyPartTagDefOf
	{
		public static BodyPartTagDef BloodFiltrationSource;

		public static BodyPartTagDef BloodFiltrationLiver;

		public static BodyPartTagDef BloodFiltrationKidney;

		public static BodyPartTagDef BloodPumpingSource;

		public static BodyPartTagDef BreathingSource;

		public static BodyPartTagDef BreathingSourceCage;

		public static BodyPartTagDef BreathingPathway;

		public static BodyPartTagDef ConsciousnessSource;

		public static BodyPartTagDef EatingSource;

		public static BodyPartTagDef EatingPathway;

		public static BodyPartTagDef HearingSource;

		public static BodyPartTagDef MetabolismSource;

		public static BodyPartTagDef ManipulationLimbCore;

		public static BodyPartTagDef ManipulationLimbSegment;

		public static BodyPartTagDef ManipulationLimbDigit;

		public static BodyPartTagDef MovingLimbCore;

		public static BodyPartTagDef MovingLimbSegment;

		public static BodyPartTagDef MovingLimbDigit;

		public static BodyPartTagDef Pelvis;

		public static BodyPartTagDef SightSource;

		public static BodyPartTagDef Spine;

		public static BodyPartTagDef TalkingSource;

		public static BodyPartTagDef TalkingPathway;

		static BodyPartTagDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartTagDefOf));
		}
	}
}
