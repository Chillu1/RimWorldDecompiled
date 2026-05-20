using System.Collections.Generic;
using Verse;

namespace RimWorld;

public interface IConstructible
{
	List<ThingDefCountClass> TotalMaterialCost();

	bool IsCompleted();

	int ThingCountNeeded(ThingDef stuff);

	ThingDef EntityToBuildStuff();
}
