using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Gizmo_RefuelableFuelStatus : Gizmo
	{
		public CompRefuelable refuelable;

		private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));

		private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

		private static readonly Texture2D TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated");

		private const float ArrowScale = 0.5f;

		public Gizmo_RefuelableFuelStatus()
		{
			order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Find.WindowStack.ImmediateWindow(1523289473, overRect, WindowLayer.GameUI, delegate
			{
				Rect rect;
				Rect rect2 = rect = overRect.AtZero().ContractedBy(6f);
				rect.height = overRect.height / 2f;
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect, refuelable.Props.FuelGizmoLabel);
				Rect rect3 = rect2;
				rect3.yMin = overRect.height / 2f;
				float fillPercent = refuelable.Fuel / refuelable.Props.fuelCapacity;
				Widgets.FillableBar(rect3, fillPercent, FullBarTex, EmptyBarTex, doBorder: false);
				if (refuelable.Props.targetFuelLevelConfigurable)
				{
					float num = refuelable.TargetFuelLevel / refuelable.Props.fuelCapacity;
					float x = rect3.x + num * rect3.width - (float)TargetLevelArrow.width * 0.5f / 2f;
					float y = rect3.y - (float)TargetLevelArrow.height * 0.5f;
					GUI.DrawTexture(new Rect(x, y, (float)TargetLevelArrow.width * 0.5f, (float)TargetLevelArrow.height * 0.5f), TargetLevelArrow);
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect3, refuelable.Fuel.ToString("F0") + " / " + refuelable.Props.fuelCapacity.ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
			});
			return new GizmoResult(GizmoState.Clear);
		}
	}
}
