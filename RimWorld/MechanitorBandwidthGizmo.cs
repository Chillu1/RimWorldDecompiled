using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class MechanitorBandwidthGizmo : Gizmo
{
	public const int InRectPadding = 6;

	private const int CellPadding = 2;

	private const float Width = 136f;

	private const int StartingBandwidthRows = 2;

	private static readonly Color EmptyBlockColor = new Color(0.3f, 0.3f, 0.3f, 1f);

	private static readonly Color FilledBlockColor = ColorLibrary.Yellow;

	private static readonly Color ExcessBlockColor = ColorLibrary.Red;

	private const int HeaderHeight = 20;

	private Pawn_MechanitorTracker tracker;

	public override bool Visible => Find.Selector.SelectedPawns.Count == 1;

	public MechanitorBandwidthGizmo(Pawn_MechanitorTracker tracker)
	{
		this.tracker = tracker;
		Order = -90f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		if (!ModLister.CheckBiotech("Mechanitor bandwidth gizmo"))
		{
			return new GizmoResult(GizmoState.Clear);
		}
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(6f);
		Widgets.DrawWindowBackground(rect);
		int totalBandwidth = tracker.TotalBandwidth;
		int usedBandwidth = tracker.UsedBandwidth;
		string text = usedBandwidth.ToString("F0") + " / " + totalBandwidth.ToString("F0");
		TaggedString taggedString = "Bandwidth".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + text + "\n\n" + "BandwidthGizmoTip".Translate();
		int usedBandwidthFromSubjects = tracker.UsedBandwidthFromSubjects;
		if (usedBandwidthFromSubjects > 0)
		{
			taggedString += string.Concat("\n\n" + ("BandwidthUsage".Translate() + ": "), usedBandwidthFromSubjects.ToString());
			IEnumerable<string> entries = from p in tracker.OverseenPawns
				where !p.IsGestating()
				group p by p.kindDef into p
				select string.Concat(p.Key.LabelCap + " x", p.Count().ToString(), " (+", p.Sum((Pawn mech) => mech.GetStatValue(StatDefOf.BandwidthCost)).ToString(), ")");
			taggedString += "\n\n" + entries.ToLineList(" - ");
		}
		int usedBandwidthFromGestation = tracker.UsedBandwidthFromGestation;
		if (usedBandwidthFromGestation > 0)
		{
			taggedString += string.Concat("\n\n" + "MechGestationBandwidthUsed".Translate() + ": ", usedBandwidthFromGestation.ToString());
			IEnumerable<string> entries2 = from p in tracker.OverseenPawns
				where p.IsGestating()
				group p by p.kindDef into p
				select string.Concat(p.Key.LabelCap + " x", p.Count().ToString(), " (+", p.Sum((Pawn mech) => mech.GetStatValue(StatDefOf.BandwidthCost)).ToString(), ")");
			taggedString += "\n\n" + entries2.ToLineList(" - ");
		}
		TooltipHandler.TipRegion(rect, taggedString);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, 20f);
		Widgets.Label(rect3, "Bandwidth".Translate());
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperRight;
		Widgets.Label(rect3, text);
		Text.Anchor = TextAnchor.UpperLeft;
		int num = Mathf.Max(usedBandwidth, totalBandwidth);
		Rect rect4 = new Rect(rect2.x, rect3.yMax + 6f, rect2.width, rect2.height - rect3.height - 6f);
		int num2 = 2;
		int num3 = Mathf.FloorToInt(rect4.height / (float)num2);
		int num4 = Mathf.FloorToInt(rect4.width / (float)num3);
		int num5 = 0;
		while (num2 * num4 < num)
		{
			num2++;
			num3 = Mathf.FloorToInt(rect4.height / (float)num2);
			num4 = Mathf.FloorToInt(rect4.width / (float)num3);
			num5++;
			if (num5 >= 1000)
			{
				Log.Error("Failed to fit bandwidth cells into gizmo rect.");
				return new GizmoResult(GizmoState.Clear);
			}
		}
		int num6 = Mathf.FloorToInt(rect4.width / (float)num3);
		int num7 = num2;
		float num8 = (rect4.width - (float)(num6 * num3)) / 2f;
		int num9 = 0;
		int usedBandwidthFromGestation2 = tracker.UsedBandwidthFromGestation;
		int num10 = ((num7 <= 2) ? 4 : 2);
		for (int num11 = 0; num11 < num7; num11++)
		{
			for (int num12 = 0; num12 < num6; num12++)
			{
				num9++;
				Rect rect5 = new Rect(rect4.x + (float)(num12 * num3) + num8, rect4.y + (float)(num11 * num3), num3, num3).ContractedBy(2f);
				if (num9 <= num)
				{
					if (num9 <= usedBandwidthFromGestation2)
					{
						Widgets.DrawRectFast(rect5, EmptyBlockColor);
						Widgets.DrawRectFast(rect5.ContractedBy(num10), FilledBlockColor);
					}
					else if (num9 <= usedBandwidth)
					{
						Widgets.DrawRectFast(rect5, (num9 <= totalBandwidth) ? FilledBlockColor : ExcessBlockColor);
					}
					else
					{
						Widgets.DrawRectFast(rect5, EmptyBlockColor);
					}
				}
			}
		}
		return new GizmoResult(GizmoState.Clear);
	}

	public override float GetWidth(float maxWidth)
	{
		return 136f;
	}
}
