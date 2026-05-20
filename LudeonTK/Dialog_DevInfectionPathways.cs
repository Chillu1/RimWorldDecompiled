using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LudeonTK;

[StaticConstructorOnStartup]
public class Dialog_DevInfectionPathways : Window_Dev
{
	private Vector2 windowPosition;

	private Vector2 scroll;

	private float lastHeight;

	private const string Title = "Infection Pathway Debugger";

	private const float ButtonHeight = 30f;

	public override bool IsDebug => true;

	protected override float Margin => 4f;

	public override Vector2 InitialSize => new Vector2(230f, 330f);

	public Dialog_DevInfectionPathways()
	{
		draggable = true;
		focusWhenOpened = false;
		drawShadow = false;
		closeOnAccept = false;
		closeOnCancel = false;
		preventCameraMotion = false;
		drawInScreenshotMode = false;
		windowPosition = Prefs.DevPalettePosition;
		onlyDrawInDevMode = true;
		doCloseX = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		Rect rect = new Rect(inRect.x, inRect.y, inRect.width, 24f);
		Rect rect2 = new Rect(inRect.x, inRect.height / 2f - 12f, inRect.width, 24f);
		DevGUI.Label(rect, "Infection Pathway Debugger");
		Text.Font = GameFont.Tiny;
		List<Pawn> selectedPawns = Find.Selector.SelectedPawns;
		if (selectedPawns.Count == 0 || !selectedPawns[0].RaceProps.Humanlike)
		{
			DevGUI.Label(rect2, "No valid humanlike selected");
			return;
		}
		Pawn pawn = Find.Selector.SelectedPawns[0];
		if (pawn.infectionVectors.PathwaysCount == 0)
		{
			DevGUI.Label(rect2, "No vectors");
			return;
		}
		Rect outRect = inRect;
		outRect.yMin = rect.height + 6f;
		Rect rect3 = inRect;
		rect3.y = 0f;
		rect3.height = lastHeight;
		Widgets.BeginScrollView(outRect, ref scroll, rect3);
		float y = 0f;
		int num = 0;
		PrintLabel("Def name", "Age", 1, rect3, ref y);
		foreach (InfectionPathway pathway in pawn.infectionVectors.Pathways)
		{
			PrintLabel(pathway, num++, rect3, ref y);
		}
		lastHeight = y;
		Widgets.EndScrollView();
	}

	private void PrintLabel(InfectionPathway pathway, int row, Rect container, ref float y)
	{
		PrintLabel(pathway.Def.label, pathway.AgeTicks.ToStringSecondsFromTicks(), row, container, ref y);
	}

	private void PrintLabel(string key, string value, int row, Rect container, ref float y)
	{
		Rect rect = new Rect(container.x, y, container.width, 20f);
		float num = container.width * 0.6f;
		Rect rect2 = new Rect(container.x, y, num, 20f);
		Rect rect3 = new Rect(container.x + num, y, num, 20f);
		if (row % 2 == 0)
		{
			DevGUI.DrawLightHighlight(rect);
		}
		DevGUI.Label(rect2, key);
		DevGUI.Label(rect3, value);
		y += 20f;
	}
}
