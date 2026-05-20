using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public sealed class TooltipGiverList
{
	private List<Thing> givers = new List<Thing>();

	public void Notify_ThingSpawned(Thing t)
	{
		if (t.def.hasTooltip || ShouldShowShotReport(t))
		{
			givers.Add(t);
		}
	}

	public void Notify_ThingDespawned(Thing t)
	{
		if (t.def.hasTooltip || ShouldShowShotReport(t))
		{
			givers.Remove(t);
		}
	}

	public void DispenseAllThingTooltips()
	{
		if (Event.current.type != EventType.Repaint || Find.WindowStack.FloatMenu != null || (Find.Targeter.IsTargeting && Find.Targeter.targetingSource != null && Find.Targeter.targetingSource.HidePawnTooltips))
		{
			return;
		}
		CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
		float cellSizePixels = Find.CameraDriver.CellSizePixels;
		Vector2 vector = new Vector2(cellSizePixels, cellSizePixels);
		Rect rect = new Rect(0f, 0f, vector.x, vector.y);
		for (int i = 0; i < givers.Count; i++)
		{
			Thing thing = givers[i];
			if (!currentViewRect.Contains(thing.Position) || thing.Position.Fogged(thing.Map))
			{
				continue;
			}
			Vector2 vector2 = thing.DrawPos.MapToUIPosition();
			rect.x = vector2.x - vector.x / 2f;
			rect.y = vector2.y - vector.y / 2f;
			if (!rect.Contains(Event.current.mousePosition))
			{
				continue;
			}
			string text = (ShouldShowShotReport(thing) ? TooltipUtility.ShotCalculationTipString(thing) : null);
			if (thing.def.hasTooltip || !text.NullOrEmpty())
			{
				if (thing is Pawn pawn && pawn.IsHiddenFromPlayer())
				{
					break;
				}
				TipSignal tooltip = thing.GetTooltip();
				if (!text.NullOrEmpty())
				{
					ref string text2 = ref tooltip.text;
					text2 = text2 + "\n\n" + text;
				}
				TooltipHandler.TipRegion(rect, tooltip);
			}
		}
	}

	private bool ShouldShowShotReport(Thing t)
	{
		if (!t.def.hasTooltip && !(t is Hive))
		{
			return t is IAttackTarget;
		}
		return true;
	}
}
