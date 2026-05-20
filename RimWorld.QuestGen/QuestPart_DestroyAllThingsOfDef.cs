using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_DestroyAllThingsOfDef : QuestPart
{
	public string inSignal;

	public MapParent mapParent;

	public List<ThingDef> defsToDestroy = new List<ThingDef>();

	private static List<Thing> tmpToDestroy = new List<Thing>();

	public QuestPart_DestroyAllThingsOfDef()
	{
	}

	public QuestPart_DestroyAllThingsOfDef(string inSignal, MapParent mapParent, List<ThingDef> defsToDestroy)
	{
		this.inSignal = inSignal;
		this.mapParent = mapParent;
		this.defsToDestroy = defsToDestroy;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignal)
		{
			DestroyThings();
		}
	}

	private void DestroyThingsOfDef(ThingDef def)
	{
		tmpToDestroy.AddRange(mapParent.Map.listerThings.ThingsOfDef(def));
		for (int i = 0; i < tmpToDestroy.Count; i++)
		{
			if (!tmpToDestroy[i].Destroyed)
			{
				tmpToDestroy[i].Destroy(DestroyMode.QuestLogic);
			}
		}
		tmpToDestroy.Clear();
	}

	private void DestroyThings()
	{
		if (mapParent?.Map == null)
		{
			return;
		}
		foreach (ThingDef item in defsToDestroy)
		{
			DestroyThingsOfDef(item);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DestroyThings();
		mapParent = null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Collections.Look(ref defsToDestroy, "defsToDestroy", LookMode.Def);
	}
}
