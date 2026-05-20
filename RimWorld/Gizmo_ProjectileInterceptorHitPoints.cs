using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Gizmo_ProjectileInterceptorHitPoints : Gizmo
{
	public CompProjectileInterceptor interceptor;

	private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

	private const float Width = 140f;

	public const int InRectPadding = 6;

	public Gizmo_ProjectileInterceptorHitPoints()
	{
		Order = -100f;
	}

	public override float GetWidth(float maxWidth)
	{
		return 140f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(6f);
		Widgets.DrawWindowBackground(rect);
		bool num = interceptor.ChargingTicksLeft > 0;
		TaggedString label = ((!num) ? "ShieldEnergy".Translate() : "ShieldTimeToRecovery".Translate());
		float fillPercent = ((!num) ? ((float)interceptor.currentHitPoints / (float)interceptor.HitPointsMax) : ((float)interceptor.ChargingTicksLeft / (float)interceptor.Props.chargeDurationTicks));
		string label2 = ((!num) ? (interceptor.currentHitPoints + " / " + interceptor.HitPointsMax) : interceptor.ChargingTicksLeft.ToStringTicksToPeriod());
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect3 = new Rect(rect2.x, rect2.y - 2f, rect2.width, rect2.height / 2f);
		Widgets.Label(rect3, label);
		Rect rect4 = new Rect(rect2.x, rect3.yMax, rect2.width, rect2.height / 2f);
		Widgets.FillableBar(rect4, fillPercent, FullBarTex, EmptyBarTex, doBorder: false);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect4, label2);
		Text.Anchor = TextAnchor.UpperLeft;
		if (!interceptor.Props.gizmoTipKey.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect2, interceptor.Props.gizmoTipKey.Translate());
		}
		return new GizmoResult(GizmoState.Clear);
	}
}
