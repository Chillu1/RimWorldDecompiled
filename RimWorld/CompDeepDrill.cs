using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompDeepDrill : ThingComp
{
	private CompPowerTrader powerComp;

	private float portionProgress;

	private float portionYieldPct;

	private int lastUsedTick = -99999;

	private const float WorkPerPortionBase = 10000f;

	[Obsolete("Use WorkPerPortionBase constant directly.")]
	public static float WorkPerPortionCurrentDifficulty => 10000f;

	public float ProgressToNextPortionPercent => portionProgress / 10000f;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		powerComp = parent.TryGetComp<CompPowerTrader>();
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref portionProgress, "portionProgress", 0f);
		Scribe_Values.Look(ref portionYieldPct, "portionYieldPct", 0f);
		Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", 0);
	}

	public void DrillWorkDone(Pawn driller, int delta)
	{
		float num = driller.GetStatValue(StatDefOf.DeepDrillingSpeed) * (float)delta;
		portionProgress += num;
		portionYieldPct += num * driller.GetStatValue(StatDefOf.MiningYield) / 10000f;
		lastUsedTick = Find.TickManager.TicksGame;
		if (portionProgress > 10000f)
		{
			TryProducePortion(portionYieldPct, driller);
			portionProgress = 0f;
			portionYieldPct = 0f;
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		portionProgress = 0f;
		portionYieldPct = 0f;
		lastUsedTick = -99999;
	}

	private void TryProducePortion(float yieldPct, Pawn driller = null)
	{
		ThingDef resDef;
		int countPresent;
		IntVec3 cell;
		bool nextResource = GetNextResource(out resDef, out countPresent, out cell);
		if (resDef == null)
		{
			return;
		}
		int num = Mathf.Min(countPresent, resDef.deepCountPerPortion);
		if (nextResource)
		{
			parent.Map.deepResourceGrid.SetAt(cell, resDef, countPresent - num);
		}
		int stackCount = Mathf.Max(1, GenMath.RoundRandom((float)num * yieldPct));
		Thing thing = ThingMaker.MakeThing(resDef);
		thing.stackCount = stackCount;
		GenPlace.TryPlaceThing(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near, null, (IntVec3 p) => p != parent.Position && p != parent.InteractionCell);
		if (driller != null)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.Mined, driller.Named(HistoryEventArgsNames.Doer)));
		}
		if (!nextResource || ValuableResourcesPresent())
		{
			return;
		}
		if (DeepDrillUtility.GetBaseResource(parent.Map, parent.Position) == null)
		{
			Messages.Message("DeepDrillExhaustedNoFallback".Translate(), parent, MessageTypeDefOf.TaskCompletion);
			return;
		}
		Messages.Message("DeepDrillExhausted".Translate(Find.ActiveLanguageWorker.Pluralize(DeepDrillUtility.GetBaseResource(parent.Map, parent.Position).label)), parent, MessageTypeDefOf.TaskCompletion);
		for (int num2 = 0; num2 < 21; num2++)
		{
			IntVec3 c = cell + GenRadial.RadialPattern[num2];
			if (c.InBounds(parent.Map))
			{
				ThingWithComps firstThingWithComp = c.GetFirstThingWithComp<CompDeepDrill>(parent.Map);
				if (firstThingWithComp != null && !firstThingWithComp.GetComp<CompDeepDrill>().ValuableResourcesPresent())
				{
					firstThingWithComp.SetForbidden(value: true);
				}
			}
		}
	}

	private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
	{
		return DeepDrillUtility.GetNextResource(parent.Position, parent.Map, out resDef, out countPresent, out cell);
	}

	public bool CanDrillNow()
	{
		if (powerComp != null && !powerComp.PowerOn)
		{
			return false;
		}
		if (DeepDrillUtility.GetBaseResource(parent.Map, parent.Position) != null)
		{
			return true;
		}
		return ValuableResourcesPresent();
	}

	public bool ValuableResourcesPresent()
	{
		ThingDef resDef;
		int countPresent;
		IntVec3 cell;
		return GetNextResource(out resDef, out countPresent, out cell);
	}

	public bool UsedLastTick()
	{
		return lastUsedTick >= Find.TickManager.TicksGame - 1;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Produce portion (100% yield)";
			command_Action.action = delegate
			{
				TryProducePortion(1f);
			};
			yield return command_Action;
		}
	}

	public override string CompInspectStringExtra()
	{
		if (parent.Spawned)
		{
			GetNextResource(out var resDef, out var _, out var _);
			if (resDef == null)
			{
				return "DeepDrillNoResources".Translate();
			}
			return "ResourceBelow".Translate() + ": " + resDef.LabelCap + "\n" + "ProgressToNextPortion".Translate() + ": " + ProgressToNextPortionPercent.ToStringPercent("F0");
		}
		return null;
	}
}
