using System.Collections.Generic;
using RimWorld.Planet;

namespace RimWorld;

public class WorldObjectCompProperties_TimedDetectionRaids : WorldObjectCompProperties
{
	public WorldObjectCompProperties_TimedDetectionRaids()
	{
		compClass = typeof(TimedDetectionRaids);
	}

	public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
		{
			yield return parentDef.defName + " has WorldObjectCompProperties_TimedDetectionRaids but it's not MapParent.";
		}
	}
}
