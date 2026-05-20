using RimWorld;

namespace Verse
{
	public class RitualFocusProperties
	{
		public IntRange spectateDistance = new IntRange(2, 2);

		public SpectateRectSide allowedSpectateSides = SpectateRectSide.Horizontal;

		public bool consumable;
	}
}
