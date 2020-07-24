using Verse;

namespace RimWorld
{
	public class CompProperties_GiveThoughtToAllMapPawnsOnDestroy : CompProperties
	{
		public ThoughtDef thought;

		[MustTranslate]
		public string message;

		public CompProperties_GiveThoughtToAllMapPawnsOnDestroy()
		{
			compClass = typeof(CompGiveThoughtToAllMapPawnsOnDestroy);
		}
	}
}
