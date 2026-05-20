using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Gizmo_MechResurrectionCharges : Gizmo
	{
		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		private static readonly float Width = 110f;

		private CompAbilityEffect_ResurrectMech ability;

		public Gizmo_MechResurrectionCharges(CompAbilityEffect_ResurrectMech ability)
		{
			this.ability = ability;
			Order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return Width;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Rect rect3 = rect2;
			rect3.height = rect.height / 2f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rect3, "MechResurrectionCharges".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect4 = rect;
			rect4.y += rect3.height - 5f;
			rect4.height = rect.height / 2f;
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect4, ability.ChargesRemaining.ToString());
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			return new GizmoResult(GizmoState.Clear);
		}
	}
}
