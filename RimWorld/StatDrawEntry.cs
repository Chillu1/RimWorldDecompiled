using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StatDrawEntry
{
	public StatCategoryDef category;

	private int displayOrderWithinCategory;

	public StatDef stat;

	private float value;

	public StatRequest optionalReq;

	public bool hasOptionalReq;

	public bool forceUnfinalizedMode;

	public bool overridesHideStats;

	private IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks;

	private string labelInt;

	private string valueStringInt;

	private string overrideReportText;

	private string overrideReportTitle;

	private string explanationText;

	private ToStringNumberSense numberSense;

	public string LabelCap
	{
		get
		{
			if (labelInt != null)
			{
				return labelInt.CapitalizeFirst();
			}
			return stat.LabelCap;
		}
	}

	public string ValueString
	{
		get
		{
			if (numberSense == ToStringNumberSense.Factor)
			{
				return value.ToStringByStyle(ToStringStyle.PercentZero);
			}
			if (valueStringInt == null)
			{
				return stat.Worker.GetStatDrawEntryLabel(stat, value, numberSense, optionalReq, !forceUnfinalizedMode);
			}
			return valueStringInt;
		}
	}

	public int DisplayPriorityWithinCategory => displayOrderWithinCategory;

	public StatDrawEntry(StatCategoryDef category, StatDef stat, float value, StatRequest optionalReq, ToStringNumberSense numberSense = ToStringNumberSense.Undefined, int? overrideDisplayPriorityWithinCategory = null, bool forceUnfinalizedMode = false)
	{
		this.category = category;
		this.stat = stat;
		labelInt = null;
		this.value = value;
		valueStringInt = null;
		displayOrderWithinCategory = (overrideDisplayPriorityWithinCategory.HasValue ? overrideDisplayPriorityWithinCategory.Value : stat.displayPriorityInCategory);
		this.optionalReq = optionalReq;
		this.forceUnfinalizedMode = forceUnfinalizedMode;
		overridesHideStats = stat.overridesHideStats;
		hasOptionalReq = true;
		if (numberSense == ToStringNumberSense.Undefined)
		{
			this.numberSense = stat.toStringNumberSense;
		}
		else
		{
			this.numberSense = numberSense;
		}
	}

	public StatDrawEntry(StatCategoryDef category, StatDef stat, string value)
	{
		this.category = category;
		this.stat = stat;
		labelInt = null;
		valueStringInt = value;
		overridesHideStats = stat.overridesHideStats;
		hasOptionalReq = false;
		numberSense = ToStringNumberSense.Undefined;
	}

	public StatDrawEntry(StatCategoryDef category, string label, string valueString, string reportText, int displayPriorityWithinCategory, string overrideReportTitle = null, IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks = null, bool forceUnfinalizedMode = false, bool overridesHideStats = false)
	{
		this.category = category;
		stat = null;
		labelInt = label;
		value = 0f;
		valueStringInt = valueString;
		displayOrderWithinCategory = displayPriorityWithinCategory;
		numberSense = ToStringNumberSense.Absolute;
		overrideReportText = reportText;
		this.overrideReportTitle = overrideReportTitle;
		this.hyperlinks = hyperlinks;
		this.forceUnfinalizedMode = forceUnfinalizedMode;
		this.overridesHideStats = overridesHideStats;
	}

	public StatDrawEntry(StatCategoryDef category, StatDef stat)
	{
		this.category = category;
		this.stat = stat;
		labelInt = null;
		value = 0f;
		valueStringInt = "-";
		displayOrderWithinCategory = stat.displayPriorityInCategory;
		numberSense = ToStringNumberSense.Undefined;
	}

	public bool ShouldDisplay([CanBeNull] Thing thing = null)
	{
		if (thing != null && thing.def.hideStats && !overridesHideStats && !DebugSettings.showHiddenInfo)
		{
			return false;
		}
		if (stat != null)
		{
			return !Mathf.Approximately(value, stat.hideAtValue);
		}
		return true;
	}

	public IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks(StatRequest req)
	{
		if (hyperlinks != null)
		{
			return hyperlinks;
		}
		if (stat != null)
		{
			return stat.Worker.GetInfoCardHyperlinks(req);
		}
		return null;
	}

	public string GetExplanationText(StatRequest optionalReq)
	{
		if (explanationText == null)
		{
			WriteExplanationTextInt();
		}
		string text = null;
		if (optionalReq.Empty || stat == null)
		{
			return explanationText;
		}
		return $"{explanationText}\n\n{stat.Worker.GetExplanationFull(optionalReq, numberSense, value)}";
	}

	private void WriteExplanationTextInt()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!overrideReportTitle.NullOrEmpty())
		{
			stringBuilder.AppendLine(overrideReportTitle);
		}
		if (!overrideReportText.NullOrEmpty())
		{
			stringBuilder.AppendLine(overrideReportText);
		}
		else if (stat != null)
		{
			stringBuilder.AppendLine(stat.description);
		}
		stringBuilder.AppendLine();
		explanationText = stringBuilder.ToString().TrimEndNewlines();
	}

	public StatDrawEntry SetReportText(string reportText)
	{
		overrideReportText = reportText;
		return this;
	}

	public float Draw(float x, float y, float width, bool selected, bool highlightLabel, bool lowlightLabel, Action clickedCallback, Action mousedOverCallback, Vector2 scrollPosition, Rect scrollOutRect, string valueCached = null)
	{
		float num = width * 0.45f;
		string text = valueCached ?? ValueString;
		Rect rect = new Rect(8f, y, width, Text.CalcHeight(text, num));
		if (!(y - scrollPosition.y + rect.height < 0f) && !(y - scrollPosition.y > scrollOutRect.height))
		{
			GUI.color = Color.white;
			if (selected)
			{
				Widgets.DrawHighlightSelected(rect);
			}
			else if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (highlightLabel)
			{
				Widgets.DrawTextHighlight(rect);
			}
			if (lowlightLabel)
			{
				GUI.color = Color.grey;
			}
			Rect rect2 = rect;
			rect2.width -= num;
			Widgets.Label(rect2, LabelCap);
			Rect rect3 = rect;
			rect3.x = rect2.xMax;
			rect3.width = num;
			Widgets.Label(rect3, text);
			GUI.color = Color.white;
			if (stat != null && Mouse.IsOver(rect))
			{
				StatDef localStat = stat;
				TooltipHandler.TipRegion(rect, new TipSignal(() => localStat.LabelCap + ": " + localStat.description, stat.GetHashCode()));
			}
			if (Widgets.ButtonInvisible(rect))
			{
				clickedCallback();
			}
			if (Mouse.IsOver(rect))
			{
				mousedOverCallback();
			}
		}
		return rect.height;
	}

	public bool Same(StatDrawEntry other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (stat == other.stat)
		{
			return labelInt == other.labelInt;
		}
		return false;
	}

	public override string ToString()
	{
		return "(" + LabelCap + ": " + ValueString + ")";
	}
}
