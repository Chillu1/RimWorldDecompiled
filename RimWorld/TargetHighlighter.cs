using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class TargetHighlighter
{
	private static List<Vector3> arrowPositions = new List<Vector3>();

	private static List<Pair<Vector3, float>> circleOverlays = new List<Pair<Vector3, float>>();

	public static void Highlight(GlobalTargetInfo target, bool arrow = true, bool colonistBar = true, bool circleOverlay = false)
	{
		if (!target.IsValid)
		{
			return;
		}
		if (arrow)
		{
			GlobalTargetInfo adjustedTarget = CameraJumper.GetAdjustedTarget(target);
			if (adjustedTarget.IsMapTarget && adjustedTarget.Map != null && adjustedTarget.Map == Find.CurrentMap)
			{
				Vector3 centerVector = ((TargetInfo)adjustedTarget).CenterVector3;
				if (!arrowPositions.Contains(centerVector))
				{
					arrowPositions.Add(centerVector);
				}
			}
		}
		if (colonistBar)
		{
			if (target.Thing is Pawn)
			{
				Find.ColonistBar.Highlight((Pawn)target.Thing);
			}
			else if (target.Thing is Corpse)
			{
				Find.ColonistBar.Highlight(((Corpse)target.Thing).InnerPawn);
			}
			else if (target.WorldObject is Caravan)
			{
				Caravan caravan = (Caravan)target.WorldObject;
				if (caravan != null)
				{
					for (int i = 0; i < caravan.pawns.Count; i++)
					{
						Find.ColonistBar.Highlight(caravan.pawns[i]);
					}
				}
			}
		}
		if (!circleOverlay || target.Thing == null || !target.Thing.Spawned || target.Thing.Map != Find.CurrentMap)
		{
			return;
		}
		float num;
		if (target.Thing is Pawn pawn)
		{
			if (pawn.RaceProps.Humanlike)
			{
				num = 1.6f;
			}
			else
			{
				num = pawn.ageTracker.CurLifeStage.bodySizeFactor * pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.y;
				num = Mathf.Max(num, 1f);
			}
		}
		else
		{
			num = Mathf.Max(target.Thing.def.size.x, target.Thing.def.size.z);
		}
		Pair<Vector3, float> item = new Pair<Vector3, float>(target.Thing.DrawPos, num * 0.5f);
		if (!circleOverlays.Contains(item))
		{
			circleOverlays.Add(item);
		}
	}

	public static void TargetHighlighterUpdate()
	{
		if (WorldRendererUtility.DrawingMap)
		{
			for (int i = 0; i < arrowPositions.Count; i++)
			{
				GenDraw.DrawArrowPointingAt(arrowPositions[i]);
			}
		}
		arrowPositions.Clear();
		if (WorldRendererUtility.DrawingMap)
		{
			for (int j = 0; j < circleOverlays.Count; j++)
			{
				GenDraw.DrawCircleOutline(circleOverlays[j].First, circleOverlays[j].Second);
			}
		}
		circleOverlays.Clear();
	}
}
