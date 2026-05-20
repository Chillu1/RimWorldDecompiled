using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class ActivityGizmo : Gizmo_Slider
{
	private readonly ThingWithComps thing;

	private static bool draggingBar;

	private static readonly Texture2D IconTex = ContentFinder<Texture2D>.Get("UI/Icons/SuppressionToggle");

	private CompActivity Comp => thing.GetComp<CompActivity>();

	protected override string Title => "ActivityGizmo".Translate();

	protected override float ValuePercent => Comp.ActivityLevel;

	protected override bool IsDraggable => Comp.CanBeSuppressed;

	protected override FloatRange DragRange => new FloatRange(0.1f, 0.9f);

	protected override string HighlightTag => "ActivityGizmo";

	protected override bool DraggingBar
	{
		get
		{
			return draggingBar;
		}
		set
		{
			draggingBar = value;
		}
	}

	protected override float Target
	{
		get
		{
			return Comp.suppressIfAbove;
		}
		set
		{
			Comp.suppressIfAbove = value;
		}
	}

	public ActivityGizmo(ThingWithComps thing)
	{
		this.thing = thing;
	}

	protected override string GetTooltip()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("ActivitySuppressionTooltipTitle".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor));
		stringBuilder.AppendLine();
		stringBuilder.Append("ActivitySuppressionTooltipDesc".Translate(thing.LabelNoParenthesis, Comp.suppressIfAbove.ToStringPercent("0").Colorize(ColoredText.TipSectionTitleColor).Named("LEVEL")).Resolve());
		Comp.Props.Worker.GetSummary(thing, stringBuilder);
		if (!Comp.suppressionEnabled)
		{
			stringBuilder.Append("\n\n" + "ActivitySuppressionTooltipDisabled".Translate(thing.LabelNoParenthesis).CapitalizeFirst().Colorize(ColoredText.FactionColor_Hostile));
		}
		foreach (IActivity comp in thing.GetComps<IActivity>())
		{
			if (!comp.ActivityTooltipExtra().NullOrEmpty())
			{
				stringBuilder.Append("\n\n" + comp.ActivityTooltipExtra());
			}
		}
		return stringBuilder.ToString();
	}

	protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
	{
		headerRect.xMax -= 24f;
		if (Comp.CanBeSuppressed)
		{
			Rect rect = new Rect(headerRect.xMax, headerRect.y, 24f, 24f);
			GUI.DrawTexture(rect, IconTex);
			GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), Comp.suppressionEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
			if (Widgets.ButtonInvisible(rect))
			{
				Comp.suppressionEnabled = !Comp.suppressionEnabled;
				if (Comp.suppressionEnabled)
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
				TooltipHandler.TipRegion(rect, GetTooltipDesc, 828267373);
				mouseOverElement = true;
			}
		}
		base.DrawHeader(headerRect, ref mouseOverElement);
	}

	private string GetTooltipDesc()
	{
		string arg = (Comp.suppressionEnabled ? "On" : "Off").Translate().ToString().UncapitalizeFirst();
		string arg2 = Comp.suppressIfAbove.ToStringPercent("0").Colorize(ColoredText.TipSectionTitleColor);
		return "ActivitySuppressionToggleTooltipDesc".Translate(thing.LabelNoParenthesis, arg2.Named("LEVEL"), arg.Named("ONOFF")).Resolve();
	}
}
