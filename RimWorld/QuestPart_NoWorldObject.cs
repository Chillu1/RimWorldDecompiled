using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_NoWorldObject : QuestPartActivable
{
	public WorldObject worldObject;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (worldObject != null)
			{
				yield return worldObject;
			}
		}
	}

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (worldObject == null || !worldObject.Spawned)
		{
			Complete();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref worldObject, "worldObject");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		Site site = Find.WorldObjects.Sites.FirstOrDefault();
		if (site != null)
		{
			worldObject = site;
			return;
		}
		Map randomPlayerHomeMap = Find.RandomPlayerHomeMap;
		if (randomPlayerHomeMap != null)
		{
			worldObject = randomPlayerHomeMap.Parent;
		}
	}
}
