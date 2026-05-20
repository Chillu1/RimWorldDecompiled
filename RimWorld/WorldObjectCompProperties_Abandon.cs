using System.Collections.Generic;
using RimWorld.Planet;

namespace RimWorld;

public class WorldObjectCompProperties_Abandon : WorldObjectCompProperties
{
	public WorldObjectCompProperties_Abandon()
	{
		compClass = typeof(AbandonComp);
	}

	public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
		{
			yield return parentDef.defName + " has WorldObjectCompProperties_Abandon but it's not MapParent.";
		}
	}
}
