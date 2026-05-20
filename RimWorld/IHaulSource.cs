using Verse;

namespace RimWorld;

public interface IHaulSource : IStoreSettingsParent, IThingHolder
{
	Map Map { get; }

	bool HaulSourceEnabled { get; }
}
