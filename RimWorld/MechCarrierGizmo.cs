using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class MechCarrierGizmo : Gizmo
{
	private CompMechCarrier carrier;

	private const float Width = 160f;

	private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

	private static readonly Texture2D BarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

	private static readonly Texture2D DragBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

	private const int Increments = 24;

	private static bool draggingBar;

	private float lastTargetValue;

	private float targetValue;

	private static List<float> bandPercentages;

	public MechCarrierGizmo(CompMechCarrier carrier)
	{
		this.carrier = carrier;
		targetValue = (float)carrier.maxToFill / (float)carrier.Props.maxIngredientCount;
		if (bandPercentages == null)
		{
			bandPercentages = new List<float>();
			int num = 12;
			for (int i = 0; i <= num; i++)
			{
				float item = 1f / (float)num * (float)i;
				bandPercentages.Add(item);
			}
		}
	}

	public override float GetWidth(float maxWidth)
	{
		return 160f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(10f);
		Widgets.DrawWindowBackground(rect);
		Text.Font = GameFont.Small;
		TaggedString labelCap = carrier.Props.fixedIngredient.LabelCap;
		float height = Text.CalcHeight(labelCap, rect2.width);
		Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, height);
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect3, labelCap);
		Text.Anchor = TextAnchor.UpperLeft;
		lastTargetValue = targetValue;
		float num = rect2.height - rect3.height;
		float num2 = num - 4f;
		float num3 = (num - num2) / 2f;
		Rect rect4 = new Rect(rect2.x, rect3.yMax + num3, rect2.width, num2);
		Widgets.DraggableBar(rect4, BarTex, BarHighlightTex, EmptyBarTex, DragBarTex, ref draggingBar, carrier.PercentageFull, ref targetValue, bandPercentages, 24);
		Text.Anchor = TextAnchor.MiddleCenter;
		rect4.y -= 2f;
		Widgets.Label(rect4, carrier.IngredientCount + " / " + carrier.Props.maxIngredientCount);
		Text.Anchor = TextAnchor.UpperLeft;
		TooltipHandler.TipRegion(rect4, () => GetResourceBarTip(), Gen.HashCombineInt(carrier.GetHashCode(), 34242369));
		if (lastTargetValue != targetValue)
		{
			carrier.maxToFill = Mathf.RoundToInt(targetValue * (float)carrier.Props.maxIngredientCount);
		}
		return new GizmoResult(GizmoState.Clear);
	}

	private string GetResourceBarTip()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Concat("MechCarrierAutofillResources".Translate() + " " + carrier.Props.fixedIngredient.label + ": ", carrier.maxToFill.ToString()));
		stringBuilder.AppendInNewLine("MechCarrierClickToSetAutofillAmount".Translate());
		stringBuilder.AppendLine();
		stringBuilder.AppendInNewLine("MechCarrierAutofillDesc".Translate(carrier.parent.def.label, carrier.Props.spawnPawnKind.labelPlural));
		return stringBuilder.ToString();
	}
}
