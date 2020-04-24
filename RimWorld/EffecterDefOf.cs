using Verse;

namespace RimWorld
{
	[DefOf]
	public static class EffecterDefOf
	{
		public static EffecterDef Clean;

		public static EffecterDef ConstructMetal;

		public static EffecterDef ConstructWood;

		public static EffecterDef ConstructDirt;

		public static EffecterDef RoofWork;

		public static EffecterDef EatMeat;

		public static EffecterDef ProgressBar;

		public static EffecterDef Mine;

		public static EffecterDef Deflect_Metal;

		public static EffecterDef Deflect_Metal_Bullet;

		public static EffecterDef Deflect_General;

		public static EffecterDef Deflect_General_Bullet;

		public static EffecterDef DamageDiminished_Metal;

		public static EffecterDef DamageDiminished_General;

		public static EffecterDef Drill;

		public static EffecterDef Research;

		public static EffecterDef ClearSnow;

		public static EffecterDef Sow;

		public static EffecterDef Harvest;

		public static EffecterDef Vomit;

		public static EffecterDef PlayPoker;

		public static EffecterDef Interceptor_BlockedProjectile;

		[MayRequireRoyalty]
		public static EffecterDef ActivatorProximityTriggered;

		[MayRequireRoyalty]
		public static EffecterDef DisabledByEMP;

		static EffecterDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(EffecterDefOf));
		}
	}
}
