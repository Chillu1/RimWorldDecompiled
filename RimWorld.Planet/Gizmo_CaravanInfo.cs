using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class Gizmo_CaravanInfo : Gizmo
{
	private Caravan caravan;

	public Gizmo_CaravanInfo(Caravan caravan)
	{
		this.caravan = caravan;
		Order = -100f;
	}

	public override float GetWidth(float maxWidth)
	{
		return Mathf.Min(520f, maxWidth);
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		if (!caravan.Spawned)
		{
			return new GizmoResult(GizmoState.Clear);
		}
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Widgets.DrawWindowBackground(rect);
		Widgets.BeginGroup(rect);
		Rect rect2 = rect.AtZero();
		int? ticksToArrive = (caravan.pather.Moving ? new int?(CaravanArrivalTimeEstimator.EstimatedTicksToArrive(caravan, allowCaching: true)) : ((int?)null));
		StringBuilder stringBuilder = new StringBuilder();
		CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(tilesPerDay: TilesPerDayCalculator.ApproxTilesPerDay(caravan, stringBuilder), massUsage: caravan.MassUsage, massCapacity: caravan.MassCapacity, massCapacityExplanation: caravan.MassCapacityExplanation, tilesPerDayExplanation: stringBuilder.ToString(), daysWorthOfFood: caravan.DaysWorthOfFood, foragedFoodPerDay: caravan.forage.ForagedFoodPerDay, foragedFoodPerDayExplanation: caravan.forage.ForagedFoodPerDayExplanation, visibility: caravan.Visibility, visibilityExplanation: caravan.VisibilityExplanation), null, caravan.Tile, ticksToArrive, -9999f, rect2, lerpMassColor: true, null, multiline: true);
		Widgets.EndGroup();
		GenUI.AbsorbClicksInRect(rect);
		return new GizmoResult(GizmoState.Clear);
	}
}
