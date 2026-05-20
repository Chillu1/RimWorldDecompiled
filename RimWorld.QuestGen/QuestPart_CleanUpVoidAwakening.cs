using System.Collections.Generic;
using RimWorld.Utility;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_CleanUpVoidAwakening : QuestPart
{
	private string inSignal;

	private Map map;

	private static List<Thing> tmpToDestroy = new List<Thing>();

	public QuestPart_CleanUpVoidAwakening()
	{
	}

	public QuestPart_CleanUpVoidAwakening(string inSignal, Map map)
	{
		this.inSignal = inSignal;
		this.map = map;
	}

	private void DestroyThingsOfDef(ThingDef def)
	{
		tmpToDestroy.AddRange(map.listerThings.ThingsOfDef(def));
		for (int i = 0; i < tmpToDestroy.Count; i++)
		{
			if (!tmpToDestroy[i].Destroyed)
			{
				tmpToDestroy[i].Destroy(DestroyMode.QuestLogic);
			}
		}
		tmpToDestroy.Clear();
	}

	private void RemoveTerrainOfDef(TerrainDef def)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (allCell.GetTerrain(map) == def)
			{
				map.terrainGrid.RemoveTopLayer(allCell, doLeavings: false);
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!(signal.tag != inSignal))
		{
			Thing.allowDestroyNonDestroyable = true;
			DestroyThingsOfDef(ThingDefOf.VoidStructure);
			DestroyThingsOfDef(ThingDefOf.PitBurrowSpawner);
			DestroyThingsOfDef(ThingDefOf.PitBurrow);
			DestroyThingsOfDef(ThingDefOf.Fleshbulb);
			DestroyThingsOfDef(ThingDefOf.VoidmetalMassSmall);
			DestroyThingsOfDef(ThingDefOf.VoidmetalMassMedium);
			Thing.allowDestroyNonDestroyable = false;
			RemoveTerrainOfDef(TerrainDefOf.Voidmetal);
			VoidAwakeningUtility.KillAllFreeEntities(map);
			map.gameConditionManager.GetActiveCondition(GameConditionDefOf.UnnaturalDarkness)?.End();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref map, "map");
	}

	public override void Cleanup()
	{
		base.Cleanup();
		map = null;
	}
}
