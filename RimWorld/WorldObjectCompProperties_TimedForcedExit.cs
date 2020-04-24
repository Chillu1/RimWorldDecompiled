using RimWorld.Planet;
using System.Collections.Generic;

namespace RimWorld
{
	public class WorldObjectCompProperties_TimedForcedExit : WorldObjectCompProperties
	{
		public WorldObjectCompProperties_TimedForcedExit()
		{
			compClass = typeof(TimedForcedExit);
		}

		public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
			{
				yield return parentDef.defName + " has WorldObjectCompProperties_TimedForcedExit but it's not MapParent.";
			}
		}
	}
}
