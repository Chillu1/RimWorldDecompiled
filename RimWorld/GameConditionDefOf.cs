using Verse;

namespace RimWorld
{
	[DefOf]
	public static class GameConditionDefOf
	{
		public static GameConditionDef SolarFlare;

		public static GameConditionDef Eclipse;

		public static GameConditionDef PsychicDrone;

		public static GameConditionDef PsychicSoothe;

		public static GameConditionDef HeatWave;

		public static GameConditionDef ColdSnap;

		public static GameConditionDef Flashstorm;

		public static GameConditionDef VolcanicWinter;

		public static GameConditionDef ToxicFallout;

		public static GameConditionDef Aurora;

		[MayRequireRoyalty]
		public static GameConditionDef PsychicSuppression;

		[MayRequireRoyalty]
		public static GameConditionDef WeatherController;

		[MayRequireRoyalty]
		public static GameConditionDef EMIField;

		[MayRequireRoyalty]
		public static GameConditionDef ToxicSpewer;

		static GameConditionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GameConditionDefOf));
		}
	}
}
