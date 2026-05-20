using Verse;

namespace RimWorld
{
	public abstract class RitualSpectatorFilter
	{
		[MustTranslate]
		public string description;

		public abstract bool Allowed(Pawn p);
	}
}
