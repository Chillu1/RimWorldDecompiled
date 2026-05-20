using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Command_ColorIcon : Command_Action
	{
		private static readonly Texture2D ColorIndicatorTex = ContentFinder<Texture2D>.Get("UI/Icons/ColorIndicatorBulb");

		public Color32? color;

		private const int colorCircleDiameter = 16;

		private const float colorCircleGap = 4f;

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (color.HasValue)
			{
				Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
				RectDivider rectDivider = new RectDivider(rect.ContractedBy(4f), 1552930585);
				GUI.DrawTexture(rectDivider.NewCol(16f, HorizontalJustification.Right).NewRow(16f), ColorIndicatorTex, ScaleMode.ScaleToFit, alphaBlend: true, 1f, color.Value, 0f, 0f);
			}
			return result;
		}
	}
}
