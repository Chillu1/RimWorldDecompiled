using System.Collections.Generic;
using RimWorld.Planet;

namespace RimWorld;

public class WorldObjectCompProperties_EscapeShip : WorldObjectCompProperties
{
	public WorldObjectCompProperties_EscapeShip()
	{
		compClass = typeof(EscapeShipComp);
	}

	public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
		{
			yield return parentDef.defName + " has WorldObjectCompProperties_EscapeShip but it's not MapParent.";
		}
	}
}
