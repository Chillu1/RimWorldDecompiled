using Verse;

namespace RimWorld
{
	[DefOf]
	public static class BodyDefOf
	{
		public static BodyDef Human;

		public static BodyDef MechanicalCentipede;

		static BodyDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BodyDefOf));
		}
	}
}
