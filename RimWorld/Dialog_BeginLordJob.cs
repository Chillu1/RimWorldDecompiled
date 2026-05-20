using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class Dialog_BeginLordJob : Window
{
	protected IPawnRoleSelectionWidget participantsDrawer;

	private Vector2 scrollPositionQualityDesc;

	private float qualityDescHeight;

	private static readonly Texture2D questionMark = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark");

	private static readonly Texture2D QualityOffsetCheckOn = Resources.Load<Texture2D>("Textures/UI/Widgets/RitualQualityCheck_On");

	private static readonly Texture2D QualityOffsetCheckOff = Resources.Load<Texture2D>("Textures/UI/Widgets/RitualQualityCheck_Off");

	protected const float CategoryCaptionHeight = 32f;

	protected const float EntryHeight = 28f;

	protected const float ListWidth = 320f;

	protected const float QualityOffsetListWidth = 402f;

	private const int ContextHash = 798775645;

	private List<QualityFactor> tmpExpectedOutcomeEffects = new List<QualityFactor>();

	private static List<ILordJobOutcomePossibility> tmpOutcomes = new List<ILordJobOutcomePossibility>();

	public override Vector2 InitialSize => new Vector2(845f, 740f);

	protected virtual Vector2 ButtonSize => new Vector2(200f, 40f);

	protected string WarningText
	{
		get
		{
			if (BlockingIssues() == null)
			{
				return "";
			}
			string result = "";
			using (IEnumerator<string> enumerator = BlockingIssues().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					result = enumerator.Current;
				}
			}
			return result;
		}
	}

	public virtual bool CanBegin
	{
		get
		{
			IEnumerable<string> enumerable = BlockingIssues();
			if (enumerable == null)
			{
				return true;
			}
			return !enumerable.Any();
		}
	}

	public virtual TaggedString HeaderLabel => "";

	public virtual TaggedString DescriptionLabel => "";

	public virtual TaggedString ExtraExplanationLabel => "";

	public virtual TaggedString ExpectedQualityLabel => "ExpectedLordJobQuality".Translate();

	public virtual TaggedString OkButtonLabel => "OK".Translate();

	public virtual TaggedString CancelButtonLabel => "CancelButton".Translate();

	public virtual TaggedString QualityFactorsLabel => "QualityFactors".Translate();

	public virtual Texture2D Icon => null;

	protected virtual IEnumerable<string> BlockingIssues()
	{
		return null;
	}

	public virtual TaggedString ExpectedDurationLabel(FloatRange qualityRange)
	{
		return TaggedString.Empty;
	}

	public virtual string OutcomeChancesLabel(string qualityNumber)
	{
		return "LordJobOutcomeChances".Translate(qualityNumber);
	}

	public virtual string OutcomeToolTip(ILordJobOutcomePossibility possibility)
	{
		return possibility.ToolTip;
	}

	public virtual Color QualitySummaryColor(FloatRange qualityRange)
	{
		return Color.white;
	}

	protected virtual void Start()
	{
		Close();
	}

	protected virtual void Cancel()
	{
		Close();
	}

	public virtual void DoExtraHeaderInfo(ref RectDivider layout, ref RectDivider headerLabelRow)
	{
	}

	public virtual void DrawExtraOutcomeDescriptions(Rect viewRect, FloatRange qualityRange, string qualityNumber, ref float curY, ref float totalInfoHeight)
	{
	}

	public Dialog_BeginLordJob(IPawnRoleSelectionWidget participantsDrawer)
	{
		if (ModLister.CheckAnyExpansion("Ritual"))
		{
			this.participantsDrawer = participantsDrawer;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			forcePause = true;
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		using (TextBlock.Default())
		{
			RectDivider layout = new RectDivider(inRect, 798775645);
			layout.NewRow(0f, VerticalJustification.Bottom, 1f);
			DoHeader(ref layout);
			layout.NewRow(0f);
			DoDescription(ref layout);
			DoButtonRow(ref layout, CanBegin);
			layout.NewRow(0f, VerticalJustification.Top, 20f);
			layout.NewCol(20f, HorizontalJustification.Left, 0f);
			RectDivider layout2 = layout.NewCol(320f, HorizontalJustification.Left, 24f);
			RectDivider layout3 = layout;
			DoLeftColumn(ref layout2);
			DoRightColumn(ref layout3);
		}
	}

	public virtual void DoHeader(ref RectDivider layout)
	{
		RectDivider headerLabelRow;
		using (new TextBlock(GameFont.Medium))
		{
			Vector2 vector = Text.CalcSize(HeaderLabel);
			headerLabelRow = layout.NewRow(vector.y);
			if (Icon != null)
			{
				Widgets.DrawTextureFitted(headerLabelRow.NewCol(vector.y, HorizontalJustification.Left, 5f), Icon, 1f);
			}
			Widgets.Label(headerLabelRow.NewCol(vector.x), HeaderLabel);
		}
		DoExtraHeaderInfo(ref layout, ref headerLabelRow);
	}

	public virtual void DoDescription(ref RectDivider layout)
	{
		using (new ProfilerBlock("DoDescription"))
		{
			string text = DescriptionLabel;
			if (!text.NullOrEmpty())
			{
				float num = Text.CalcHeight(text, layout.Rect.width - 30f);
				RectDivider rectDivider = layout.NewRow(num + 17f, VerticalJustification.Top, 10f);
				rectDivider.NewRow(0f, VerticalJustification.Top, 10f);
				rectDivider.NewCol(10f, HorizontalJustification.Left, 0f);
				rectDivider.NewCol(20f, HorizontalJustification.Right, 0f);
				Widgets.Label(rectDivider, text);
			}
			string text2 = ExtraExplanationLabel;
			if (!text2.NullOrEmpty() || !text2.NullOrEmpty())
			{
				float num2 = Text.CalcHeight(text2, layout.Rect.width - 30f);
				RectDivider rectDivider2 = layout.NewRow(num2 + 17f, VerticalJustification.Top, 0f);
				rectDivider2.NewRow(0f, VerticalJustification.Top, 10f);
				rectDivider2.NewCol(10f, HorizontalJustification.Left, 0f);
				rectDivider2.NewCol(20f, HorizontalJustification.Right, 0f);
				Widgets.Label(rectDivider2, text2);
			}
		}
	}

	public virtual void DoButtonRow(ref RectDivider layout, bool canBegin)
	{
		RectDivider rectDivider = layout.NewRow(ButtonSize.y, VerticalJustification.Bottom, 0f);
		RectDivider rectDivider2 = rectDivider.NewCol(ButtonSize.x, HorizontalJustification.Right, 10f);
		RectDivider rectDivider3 = rectDivider.NewCol(ButtonSize.x);
		RectDivider rectDivider4 = rectDivider.NewCol(rectDivider.Rect.width, HorizontalJustification.Right);
		TextBlock textBlock = new TextBlock(canBegin ? Color.white : Color.gray);
		try
		{
			if (Widgets.ButtonText(rectDivider2, OkButtonLabel, drawBackground: true, doMouseoverSound: true, canBegin))
			{
				Start();
			}
		}
		finally
		{
			((IDisposable)textBlock/*cast due to .constrained prefix*/).Dispose();
		}
		if (Widgets.ButtonText(rectDivider3, CancelButtonLabel))
		{
			Cancel();
		}
		using (new TextBlock(TextAnchor.MiddleRight, ColorLibrary.RedReadable))
		{
			Widgets.Label(rectDivider4, WarningText);
		}
	}

	public virtual void DoLeftColumn(ref RectDivider layout)
	{
		layout.NewRow(10f, VerticalJustification.Bottom, 0f);
		using (new ProfilerBlock("DrawPawnList"))
		{
			participantsDrawer.DrawPawnList(layout);
		}
	}

	public virtual void DoRightColumn(ref RectDivider layout)
	{
		DrawQualityFactors(layout.NewCol(402f));
	}

	private float DrawQualityFactor(Rect viewRect, bool even, QualityFactor qualityFactor, float y)
	{
		if (qualityFactor == null)
		{
			return 0f;
		}
		Rect rect = new Rect(viewRect.x, y, viewRect.width, 25f);
		Rect rect2 = new Rect
		{
			x = viewRect.x,
			width = viewRect.width + 10f,
			y = y - 3f,
			height = 28f
		};
		if (even)
		{
			Widgets.DrawLightHighlight(rect2);
		}
		GUI.color = (qualityFactor.uncertainOutcome ? ColorLibrary.Yellow : (qualityFactor.positive ? ColorLibrary.Green : ColorLibrary.RedReadable));
		Rect rect3 = rect;
		rect3.width = 205f;
		Widgets.LabelEllipses(rect3, "  " + qualityFactor.label);
		using (new TextBlock(TextAnchor.UpperRight))
		{
			Widgets.Label(rect, qualityFactor.qualityChange);
		}
		if (!qualityFactor.noMiddleColumnInfo)
		{
			if (!qualityFactor.count.NullOrEmpty())
			{
				float x = Text.CalcSize(qualityFactor.count).x;
				Rect rect4 = new Rect(rect);
				rect4.xMin += 220f - x / 2f;
				rect4.width = x;
				Widgets.Label(rect4, qualityFactor.count);
			}
			else
			{
				GUI.color = Color.white;
				Texture2D image = (qualityFactor.uncertainOutcome ? questionMark : ((!qualityFactor.present) ? QualityOffsetCheckOff : QualityOffsetCheckOn));
				Rect rect5 = new Rect(rect);
				rect5.x += 208f;
				rect5.y -= 1f;
				rect5.width = 24f;
				rect5.height = 24f;
				if (!qualityFactor.present)
				{
					if (qualityFactor.uncertainOutcome)
					{
						TooltipHandler.TipRegion(rect5, () => "QualityFactorTooltipUncertain".Translate(), 238934347);
					}
					else
					{
						TooltipHandler.TipRegion(rect5, () => "QualityFactorTooltipNotFulfilled".Translate(), 238934347);
					}
				}
				GUI.DrawTexture(rect5, image);
			}
		}
		GUI.color = Color.white;
		if (qualityFactor.toolTip != null && Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect2);
			TooltipHandler.TipRegion(rect, () => qualityFactor.toolTip, 976091152);
		}
		return 28f;
	}

	public static string QualityNumberToString(FloatRange qualityRange)
	{
		if (qualityRange.min == qualityRange.max)
		{
			return qualityRange.min.ToStringPercent("F0");
		}
		return qualityRange.min.ToStringPercent("F0") + "-" + qualityRange.max.ToStringPercent("F0");
	}

	protected virtual List<QualityFactor> PopulateQualityFactors(out FloatRange qualityRange)
	{
		tmpExpectedOutcomeEffects.Clear();
		qualityRange = new FloatRange(0f, 0f);
		return tmpExpectedOutcomeEffects;
	}

	protected virtual List<ILordJobOutcomePossibility> PopulateOutcomePossibilities()
	{
		tmpOutcomes.Clear();
		return tmpOutcomes;
	}

	public virtual void DrawOutcomeChances(Rect viewRect, FloatRange qualityRange, string qualityNumber, ref float curY, ref float totalInfoHeight)
	{
		List<ILordJobOutcomePossibility> list = PopulateOutcomePossibilities();
		if (list.NullOrEmpty())
		{
			return;
		}
		Widgets.Label(new Rect(viewRect.x, curY, viewRect.width, 32f), OutcomeChancesLabel(qualityNumber) + ": ");
		curY += 28f;
		totalInfoHeight += 28f;
		float num = 0f;
		foreach (ILordJobOutcomePossibility item in list)
		{
			num += item.Weight(qualityRange);
		}
		foreach (ILordJobOutcomePossibility item2 in list)
		{
			float f = item2.Weight(qualityRange) / num;
			TaggedString taggedString = "  - " + item2.Label + ": " + f.ToStringPercent();
			Rect rect = new Rect(viewRect.x, curY, Text.CalcSize(taggedString).x + 4f, 32f);
			Rect rect2 = new Rect(rect);
			rect2.width = rect.width + 8f;
			rect2.height = 22f;
			Rect rect3 = rect2;
			if (Mouse.IsOver(rect3))
			{
				string desc = OutcomeToolTip(item2);
				Widgets.DrawLightHighlight(rect3);
				if (!desc.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect3, () => desc, 231134347);
				}
			}
			Widgets.Label(rect, taggedString);
			curY += Text.LineHeight;
			totalInfoHeight += Text.LineHeight;
		}
	}

	protected virtual void DrawQualityDescription(Rect outRectQualityDesc, FloatRange qualityRange, string qualityNumber, float totalInfoHeight)
	{
		Rect viewRect = new Rect(0f, 0f, outRectQualityDesc.width, qualityDescHeight);
		bool flag = qualityDescHeight > outRectQualityDesc.height;
		if (flag)
		{
			viewRect.width -= 16f;
		}
		float curY = 0f;
		Widgets.BeginScrollView(outRectQualityDesc, ref scrollPositionQualityDesc, viewRect, flag);
		TaggedString taggedString = ExpectedDurationLabel(qualityRange);
		if (!taggedString.NullOrEmpty())
		{
			float num = Text.CalcHeight(taggedString, viewRect.width);
			Widgets.Label(new Rect(viewRect.x, curY - 4f, viewRect.width, num), taggedString);
			curY += 17f + (num - 22f);
			totalInfoHeight += 17f + (num - 22f);
		}
		DrawOutcomeChances(viewRect, qualityRange, qualityNumber, ref curY, ref totalInfoHeight);
		DrawExtraOutcomeDescriptions(viewRect, qualityRange, qualityNumber, ref curY, ref totalInfoHeight);
		GUI.color = Color.white;
		qualityDescHeight = curY;
		Widgets.EndScrollView();
	}

	protected virtual void DrawQualityFactors(Rect viewRect)
	{
		float y = viewRect.y;
		float num = 0f;
		bool flag = true;
		FloatRange qualityRange;
		List<QualityFactor> list = PopulateQualityFactors(out qualityRange);
		if (list.NullOrEmpty())
		{
			return;
		}
		Widgets.Label(new Rect(viewRect.x, y + 3f, viewRect.width, 32f), QualityFactorsLabel);
		y += 32f;
		num += 32f;
		foreach (QualityFactor item in list.OrderByDescending((QualityFactor e) => e.priority))
		{
			float num2 = DrawQualityFactor(viewRect, flag, item, y);
			y += num2;
			num += num2;
			flag = !flag;
		}
		y += 2f;
		string text = QualityNumberToString(qualityRange);
		using (new TextBlock(QualitySummaryColor(qualityRange)))
		{
			Widgets.Label(new Rect(viewRect.x, y + 4f, viewRect.width, 25f), ExpectedQualityLabel + ":");
			using (new TextBlock(GameFont.Medium))
			{
				float x = Text.CalcSize(text).x;
				Widgets.Label(new Rect(viewRect.xMax - x, y - 2f, viewRect.width, 32f), text);
			}
			y += 28f;
			num += 28f;
		}
		Rect rect = viewRect;
		rect.width += 10f;
		rect.height = num;
		rect = rect.ExpandedBy(9f);
		using (new TextBlock(new Color(0.25f, 0.25f, 0.25f)))
		{
			Widgets.DrawBox(rect, 2);
		}
		y += 10f;
		num += 10f;
		Rect outRectQualityDesc = new Rect(viewRect.x, y, viewRect.width, viewRect.height - num);
		DrawQualityDescription(outRectQualityDesc, qualityRange, text, num);
	}

	public override void WindowUpdate()
	{
		base.WindowUpdate();
		participantsDrawer.WindowUpdate();
	}

	public override void OnAcceptKeyPressed()
	{
		Start();
	}
}
