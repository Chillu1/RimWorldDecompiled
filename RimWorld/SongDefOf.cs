using Verse;

namespace RimWorld
{
	[DefOf]
	public static class SongDefOf
	{
		public static SongDef EntrySong;

		public static SongDef EndCreditsSong;

		static SongDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SongDefOf));
		}
	}
}
