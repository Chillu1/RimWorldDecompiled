using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IConstructible
	{
		List<ThingDefCountClass> MaterialsNeeded();

		ThingDef EntityToBuildStuff();
	}
}
