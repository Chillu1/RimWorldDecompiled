using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Gizmo_GrowthTier : Gizmo
{
	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(GenUI.FillableBar_Empty);

	private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1254902f, 46f / 85f, 0f));

	private const float Spacing = 8f;

	private const float LabelWidthPercent = 0.55f;

	private const float BarMarginY = 2f;

	private const int GrowthTierTooltipId = 837825001;

	private Pawn child;

	private float Width => 190f;

	private int GrowthTier => child.ageTracker.GrowthTier;

	public override bool Visible
	{
		get
		{
			if (!child.IsColonistPlayerControlled && !child.IsPrisonerOfColony)
			{
				return child.IsSlaveOfColony;
			}
			return true;
		}
	}

	public override float GetWidth(float maxWidth)
	{
		return Width;
	}

	public Gizmo_GrowthTier(Pawn child)
	{
		this.child = child;
		Order = -100f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(8f);
		Widgets.DrawWindowBackground(rect);
		Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, rect2.height / 2f);
		Rect rect4 = new Rect(rect2.x, rect3.yMax, rect2.width, rect3.height);
		rect3.yMax -= 2f;
		rect4.yMin += 2f;
		DrawGrowthTier(rect3);
		DrawLearning(rect4);
		return new GizmoResult(GizmoState.Clear);
	}

	private string GrowthTierTooltip(int tier)
	{
		TaggedString taggedString = ("StatsReport_GrowthTier".Translate() + ": ").AsTipTitle() + tier + "\n" + "StatsReport_GrowthTierDesc".Translate().Colorize(ColoredText.SubtleGrayColor) + "\n\n";
		if (child.ageTracker.AtMaxGrowthTier)
		{
			taggedString += ("MaxTier".Translate() + ": ").AsTipTitle() + "MaxTierDesc".Translate(child.Named("PAWN"));
		}
		else
		{
			TaggedString taggedString2 = taggedString;
			string text = ("ProgressToNextGrowthTier".Translate() + ": ").AsTipTitle();
			string text2 = Mathf.FloorToInt(child.ageTracker.growthPoints).ToString();
			float pointsRequirement = GrowthUtility.GrowthTiers[tier + 1].pointsRequirement;
			taggedString = taggedString2 + (text + text2 + " / " + pointsRequirement);
			if (child.ageTracker.canGainGrowthPoints)
			{
				taggedString += string.Format(" (+{0})", "PerDay".Translate(child.ageTracker.GrowthPointsPerDay.ToStringByStyle(ToStringStyle.FloatMaxTwo)));
			}
		}
		if (child.ageTracker.AgeBiologicalYears < 13)
		{
			int num = 0;
			for (int i = child.ageTracker.AgeBiologicalYears + 1; i <= 13; i++)
			{
				if (GrowthUtility.IsGrowthBirthday(i))
				{
					num = i;
					break;
				}
			}
			taggedString += "\n\n" + ("NextGrowthMomentAt".Translate() + ": ").AsTipTitle() + num;
		}
		GrowthUtility.GrowthTier growthTier = GrowthUtility.GrowthTiers[tier];
		taggedString += "\n\n" + ("ThisGrowthTier".Translate(tier) + ":").AsTipTitle();
		if (growthTier.passionGainsRange.TrueMax > 0)
		{
			taggedString += "\n  - " + "NumPassionsFromOptions".Translate(growthTier.passionGainsRange.ToString(), growthTier.passionChoices);
		}
		taggedString += "\n  - " + "NumTraitsFromOptions".Translate(growthTier.traitGains, growthTier.traitChoices);
		if (!child.ageTracker.AtMaxGrowthTier)
		{
			GrowthUtility.GrowthTier growthTier2 = GrowthUtility.GrowthTiers[tier + 1];
			taggedString += "\n\n" + ("NextGrowthTier".Translate(tier + 1) + ":").AsTipTitle();
			if (growthTier2.passionGainsRange.TrueMax > 0)
			{
				taggedString += "\n  - " + "NumPassionsFromOptions".Translate(growthTier2.passionGainsRange.ToString(), growthTier2.passionChoices);
			}
			taggedString += "\n  - " + "NumTraitsFromOptions".Translate(growthTier2.traitGains, growthTier2.traitChoices);
		}
		return taggedString.Resolve();
	}

	private void DrawGrowthTier(Rect rect)
	{
		int growthTier = GrowthTier;
		Rect rect2 = rect;
		rect2.xMax = rect.x + rect.width * 0.55f;
		string label = string.Concat("StatsReport_GrowthTier".Translate() + ": ", growthTier.ToString());
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect2, label);
		Text.Anchor = TextAnchor.UpperLeft;
		float percentToNextGrowthTier = child.ageTracker.PercentToNextGrowthTier;
		Rect rect3 = rect;
		rect3.xMin = rect2.xMax;
		rect3.yMin += 2f;
		rect3.yMax -= 2f;
		Widgets.FillableBar(rect3, percentToNextGrowthTier, BarTex, EmptyBarTex, doBorder: true);
		Text.Anchor = TextAnchor.MiddleCenter;
		float pointsRequirement = GrowthUtility.GrowthTiers[GrowthUtility.GrowthTiers.Length - 1].pointsRequirement;
		string text2;
		if (!child.ageTracker.AtMaxGrowthTier)
		{
			string text = Mathf.FloorToInt(child.ageTracker.growthPoints).ToString();
			float pointsRequirement2 = GrowthUtility.GrowthTiers[growthTier + 1].pointsRequirement;
			text2 = text + " / " + pointsRequirement2;
		}
		else
		{
			text2 = pointsRequirement + " / " + pointsRequirement;
		}
		string label2 = text2;
		Widgets.Label(rect3, label2);
		Text.Anchor = TextAnchor.UpperLeft;
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, new TipSignal(GrowthTierTooltip(growthTier), child.thingIDNumber ^ 0x31F031E9));
		}
	}

	private void DrawLearning(Rect rect)
	{
		if (child.needs.learning != null)
		{
			Rect rect2 = rect;
			rect2.xMax = rect.x + rect.width * 0.55f;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect2, NeedDefOf.Learning.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect3 = rect;
			rect3.xMin = rect2.xMax;
			rect3.yMin += 2f;
			rect3.yMax -= 2f;
			Widgets.FillableBar(rect3, child.needs.learning.CurLevelPercentage, Widgets.BarFullTexHor, EmptyBarTex, doBorder: true);
			Text.Anchor = TextAnchor.MiddleCenter;
			string label = child.needs.learning.CurLevelPercentage.ToStringPercent();
			Widgets.Label(rect3, label);
			Text.Anchor = TextAnchor.UpperLeft;
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
				TooltipHandler.TipRegion(rect, child.needs.learning.GetTipString());
			}
		}
	}
}
