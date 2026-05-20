using Verse;

namespace RimWorld;

public interface IHaulEnroute : ILoadReferenceable
{
	Map Map { get; }

	int SpaceRemainingFor(ThingDef stuff);
}
