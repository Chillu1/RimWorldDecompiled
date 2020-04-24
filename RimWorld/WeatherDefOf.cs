using Verse;

namespace RimWorld
{
	[DefOf]
	public static class WeatherDefOf
	{
		public static WeatherDef Clear;

		static WeatherDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(WeatherDefOf));
		}
	}
}
