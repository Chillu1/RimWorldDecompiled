using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_Skyfaller : QuestPart
{
	public ThingDef skyfallerDef;

	public string inSignal;

	public IntVec3 dropSpot = IntVec3.Invalid;

	public MapParent mapParent;

	private List<Thing> items = new List<Thing>();

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (mapParent == null || !mapParent.HasMap || !quest.IsParentSuitableForQuest(mapParent))
		{
			mapParent = quest.TryFindNewSuitableMapParentForRetarget();
		}
		if (signal.tag == inSignal && mapParent != null && mapParent.HasMap)
		{
			items.RemoveAll((Thing x) => x.Destroyed);
			Map map = mapParent.Map;
			SkyfallerMaker.SpawnSkyfaller(skyfallerDef, items, dropSpot, map);
		}
	}
}
