using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Command_ActionWithCooldown : Command_Action
	{
		public Func<float> cooldownPercentGetter;

		private static readonly Texture2D CooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			if (cooldownPercentGetter != null)
			{
				float num = cooldownPercentGetter();
				if (num < 1f)
				{
					Widgets.FillableBar(rect, Mathf.Clamp01(num), CooldownBarTex, null, doBorder: false);
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.UpperCenter;
					Widgets.Label(rect, num.ToStringPercent("F0"));
					Text.Anchor = TextAnchor.UpperLeft;
				}
			}
			return result;
		}
	}
}
