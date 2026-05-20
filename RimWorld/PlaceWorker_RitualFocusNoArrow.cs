using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PlaceWorker_RitualFocusNoArrow : PlaceWorker_RitualFocus
	{
		protected override bool UseArrow => false;
	}
}
