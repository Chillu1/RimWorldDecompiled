using Verse;

namespace RimWorld
{
	public abstract class ITab_PenBase : ITab
	{
		public CompAnimalPenMarker SelectedCompAnimalPenMarker => base.SelThing?.TryGetComp<CompAnimalPenMarker>();

		public override bool IsVisible => SelectedCompAnimalPenMarker != null;
	}
}
