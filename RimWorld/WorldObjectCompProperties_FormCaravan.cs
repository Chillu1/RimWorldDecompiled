using RimWorld.Planet;
using System.Collections.Generic;

namespace RimWorld
{
	public class WorldObjectCompProperties_FormCaravan : WorldObjectCompProperties
	{
		public WorldObjectCompProperties_FormCaravan()
		{
			compClass = typeof(FormCaravanComp);
		}

		public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
			{
				yield return parentDef.defName + " has WorldObjectCompProperties_FormCaravan but it's not MapParent.";
			}
		}
	}
}
