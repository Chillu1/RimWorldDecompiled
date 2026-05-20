using System.Collections.Generic;
using RimWorld.Planet;

namespace RimWorld
{
	public class WorldObjectCompProperties_TimedMakeFactionHostile : WorldObjectCompProperties
	{
		public WorldObjectCompProperties_TimedMakeFactionHostile()
		{
			compClass = typeof(TimedMakeFactionHostile);
		}

		public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
			{
				yield return parentDef.defName + " has WorldObjectCompProperties_TimedMakeFactionHostile but it's not MapParent.";
			}
		}
	}
}
