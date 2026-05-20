using Verse;

namespace RimWorld;

public interface IApparelSource : IThingHolder
{
	Map Map { get; }

	bool ApparelSourceEnabled { get; }

	bool RemoveApparel(Apparel apparel);
}
