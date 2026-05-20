using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_SelectXenogerm : Window
{
	private Pawn pawn;

	private List<Xenogerm> xenogerms = new List<Xenogerm>();

	private Xenogerm selected;

	private Vector2 scrollPosition;

	private float scrollViewHeight;

	private Action<Xenogerm> onSelect;

	private const float HeaderHeight = 35f;

	public const float MiniGeneIconSize = 22f;

	private const float XenogermElementHeight = 32f;

	private const int MaxDisplayedGenes = 10;

	private static readonly Vector2 ButSize = new Vector2(150f, 38f);

	private Dictionary<string, string> truncateCache = new Dictionary<string, string>();

	public override Vector2 InitialSize => new Vector2(500f, 600f);

	public Dialog_SelectXenogerm(Pawn pawn, Map map, Xenogerm initialSelected, Action<Xenogerm> onSelect)
	{
		this.pawn = pawn;
		this.onSelect = onSelect;
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.Xenogerm))
		{
			if (!item.PositionHeld.Fogged(map))
			{
				xenogerms.Add((Xenogerm)item);
			}
		}
		if (initialSelected != null && xenogerms.Contains(initialSelected))
		{
			selected = initialSelected;
		}
		closeOnAccept = false;
		absorbInputAroundWindow = true;
	}

	public override void PostOpen()
	{
		if (!ModLister.CheckBiotech("xenogerm"))
		{
			Close(doCloseSound: false);
		}
		else
		{
			base.PostOpen();
		}
	}

	public override void DoWindowContents(Rect rect)
	{
		Rect rect2 = rect;
		rect2.yMax -= ButSize.y + 4f;
		Text.Font = GameFont.Medium;
		Widgets.Label(rect2, "SelectXenogerm".Translate());
		Text.Font = GameFont.Small;
		rect2.yMin += 39f;
		DisplayXenogerms(rect2);
		Rect rect3 = rect;
		rect3.yMin = rect3.yMax - ButSize.y;
		if (selected != null)
		{
			if (Widgets.ButtonText(new Rect(rect3.xMax - ButSize.x, rect3.y, ButSize.x, ButSize.y), "Accept".Translate()))
			{
				Accept();
			}
			if (Widgets.ButtonText(new Rect(rect3.x, rect3.y, ButSize.x, ButSize.y), "Close".Translate()))
			{
				Close();
			}
		}
		else if (Widgets.ButtonText(new Rect((rect3.width - ButSize.x) / 2f, rect3.y, ButSize.x, ButSize.y), "Close".Translate()))
		{
			Close();
		}
	}

	private void DisplayXenogerms(Rect rect)
	{
		Widgets.DrawMenuSection(rect);
		rect = rect.ContractedBy(4f);
		GUI.BeginGroup(rect);
		Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
		float num = 0f;
		Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);
		for (int i = 0; i < xenogerms.Count; i++)
		{
			float num2 = rect.width;
			if (scrollViewHeight > rect.height)
			{
				num2 -= 16f;
			}
			DrawXenogerm(new Rect(0f, num, num2, 32f), i);
			num += 32f;
		}
		if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = num;
		}
		Widgets.EndScrollView();
		GUI.EndGroup();
	}

	private void DrawXenogerm(Rect rect, int index)
	{
		Xenogerm xenogerm = xenogerms[index];
		if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
		}
		if (selected == xenogerm)
		{
			Widgets.DrawHighlightSelected(rect);
		}
		Widgets.InfoCardButton(rect.xMax - 24f, rect.y + 4f, xenogerm);
		rect.xMax -= 36f;
		for (int num = Mathf.Min(xenogerm.GeneSet.GenesListForReading.Count, 10) - 1; num >= 0; num--)
		{
			GeneDef geneDef = xenogerm.GeneSet.GenesListForReading[num];
			Rect rect2 = new Rect(rect.xMax - 11f, rect.yMax - rect.height / 2f - 11f, 22f, 22f);
			Widgets.DefIcon(rect2, geneDef, null, 1.25f);
			Rect rect3 = rect2;
			rect3.yMin = rect.yMin;
			rect3.yMax = rect.yMax;
			if (Mouse.IsOver(rect3))
			{
				Widgets.DrawHighlight(rect3);
				TooltipHandler.TipRegion(rect3, geneDef.LabelCap + "\n\n" + geneDef.DescriptionFull);
			}
			rect.xMax -= 22f;
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, () => xenogerm.LabelCap + "\n\n" + "Genes".Translate().CapitalizeFirst() + ":\n" + xenogerm.GeneSet.GenesListForReading.Select((GeneDef x) => x.LabelCap.ToString()).ToLineList("  - "), 128921381);
		}
		rect.xMin += 4f;
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect, xenogerm.LabelCap.Truncate(rect.width, truncateCache));
		Text.Anchor = TextAnchor.UpperLeft;
		if (Widgets.ButtonInvisible(rect))
		{
			selected = xenogerm;
		}
	}

	private void Accept()
	{
		if (pawn != null)
		{
			int num = GeneUtility.MetabolismAfterImplanting(pawn, selected.GeneSet);
			if (num < GeneTuning.BiostatRange.TrueMin)
			{
				Messages.Message(string.Concat("OrderImplantationIntoPawn".Translate(pawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "ResultingMetTooLow".Translate() + " (", num.ToString(), ")"), pawn, MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			if (selected.PawnIdeoDisallowsImplanting(pawn))
			{
				Messages.Message("CannotGenericWorkCustom".Translate("OrderImplantationIntoPawn".Translate(pawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "IdeoligionForbids".Translate()), pawn, MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
		}
		if (onSelect != null)
		{
			onSelect(selected);
		}
		Close();
	}
}
