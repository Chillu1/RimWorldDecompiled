using System.Collections.Generic;
using RimWorld.Planet;

namespace RimWorld;

public class WorldObjectCompProperties_DefeatAllEnemiesQuest : WorldObjectCompProperties
{
	public WorldObjectCompProperties_DefeatAllEnemiesQuest()
	{
		compClass = typeof(DefeatAllEnemiesQuestComp);
	}

	public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
		{
			yield return parentDef.defName + " has WorldObjectCompProperties_DefeatAllEnemiesQuest but it's not MapParent.";
		}
	}
}
