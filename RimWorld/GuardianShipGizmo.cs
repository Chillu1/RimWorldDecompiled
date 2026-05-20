using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class GuardianShipGizmo : Gizmo
	{
		private QuestPart_GuardianShipDelay delay;

		public GuardianShipGizmo(QuestPart_GuardianShipDelay delay)
		{
			this.delay = delay;
			Order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return 110f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect2, "GuardianShipPayment".Translate(delay.TicksLeft.ToStringTicksToPeriod()));
			GenUI.ResetLabelAlign();
			return new GizmoResult(GizmoState.Clear);
		}
	}
}
