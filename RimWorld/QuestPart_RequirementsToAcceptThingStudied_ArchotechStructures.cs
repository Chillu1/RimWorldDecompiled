using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptThingStudied_ArchotechStructures : QuestPart_RequirementsToAcceptThingStudied
{
	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (thing == null || thing.Spawned || thing.TryGetComp<CompStudiable>().Completed)
			{
				yield break;
			}
			foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
			{
				if (allWorldObject is AbandonedArchotechStructures abandonedArchotechStructures && !abandonedArchotechStructures.archotechStructures.NullOrEmpty() && abandonedArchotechStructures.archotechStructures.Contains(thing))
				{
					yield return allWorldObject;
					yield break;
				}
			}
		}
	}
}
