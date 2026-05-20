using Verse;

namespace RimWorld
{
	public class RitualSpectatorFilter_NatureFocus : RitualSpectatorFilter
	{
		public override bool Allowed(Pawn p)
		{
			return MeditationFocusDefOf.Natural.CanPawnUse(p);
		}
	}
}
