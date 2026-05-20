using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_ChangeDryadCaste : Window
{
	private CompTreeConnection treeConnection;

	private Pawn connectedPawn;

	private Vector2 scrollPosition;

	private GauranlenTreeModeDef selectedMode;

	private GauranlenTreeModeDef currentMode;

	private float rightViewWidth;

	private List<GauranlenTreeModeDef> allDryadModes;

	private const float HeaderHeight = 35f;

	private const float LeftRectWidth = 400f;

	private const float OptionSpacing = 52f;

	private const float ChangeFormButtonHeight = 55f;

	private static readonly Vector2 OptionSize = new Vector2(190f, 46f);

	private static readonly Vector2 ButSize = new Vector2(200f, 40f);

	public override Vector2 InitialSize => new Vector2(Mathf.Min(900, UI.screenWidth), 650f);

	private PawnKindDef SelectedKind => selectedMode.pawnKindDef;

	public Dialog_ChangeDryadCaste(Thing tree)
	{
		treeConnection = tree.TryGetComp<CompTreeConnection>();
		currentMode = treeConnection.desiredMode;
		selectedMode = currentMode;
		connectedPawn = treeConnection.ConnectedPawn;
		forcePause = true;
		closeOnAccept = false;
		doCloseX = true;
		doCloseButton = true;
		allDryadModes = DefDatabase<GauranlenTreeModeDef>.AllDefs.ToList();
	}

	public override void PreOpen()
	{
		if (!ModLister.CheckIdeology("Dryad upgrades"))
		{
			Close();
		}
		base.PreOpen();
		SetupView();
	}

	private void SetupView()
	{
		foreach (GauranlenTreeModeDef allDryadMode in allDryadModes)
		{
			rightViewWidth = Mathf.Max(rightViewWidth, GetPosition(allDryadMode, InitialSize.y).x + OptionSize.x);
		}
		rightViewWidth += 20f;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Medium;
		string label = ((selectedMode != null) ? selectedMode.LabelCap : "ChangeMode".Translate());
		Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 35f), label);
		Text.Font = GameFont.Small;
		float num = inRect.y + 35f + 10f;
		float curY = num;
		float num2 = inRect.height - num;
		num2 -= ButSize.y + 10f;
		DrawLeftRect(new Rect(inRect.xMin, num, 400f, num2), ref curY);
		DrawRightRect(new Rect(inRect.x + 400f + 17f, num, inRect.width - 400f - 17f, num2));
	}

	private void DrawLeftRect(Rect rect, ref float curY)
	{
		Rect rect2 = new Rect(rect.x, curY, rect.width, rect.height);
		rect2.yMax = rect.yMax;
		Rect rect3 = rect2.ContractedBy(4f);
		if (selectedMode == null)
		{
			Widgets.Label(rect3, "ChooseProductionModeInitialDesc".Translate(connectedPawn.Named("PAWN"), treeConnection.parent.Named("TREE"), ThingDefOf.DryadCocoon.GetCompProperties<CompProperties_DryadCocoon>().daysToComplete.Named("UPGRADEDURATION")));
			return;
		}
		Widgets.Label(rect3.x, ref curY, rect3.width, selectedMode.Description);
		curY += 10f;
		if (!Find.IdeoManager.classicMode && !selectedMode.requiredMemes.NullOrEmpty())
		{
			Widgets.Label(rect3.x, ref curY, rect3.width, "RequiredMemes".Translate() + ":");
			string text = "";
			for (int i = 0; i < selectedMode.requiredMemes.Count; i++)
			{
				MemeDef memeDef = selectedMode.requiredMemes[i];
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text = text + "  - " + memeDef.LabelCap.ToString().Colorize(connectedPawn.Ideo.HasMeme(memeDef) ? Color.white : ColorLibrary.RedReadable);
			}
			Widgets.Label(rect3.x, ref curY, rect3.width, text);
			curY += 10f;
		}
		if (selectedMode.previousStage != null)
		{
			Widgets.Label(rect3.x, ref curY, rect3.width, string.Concat("RequiredStage".Translate(), ": ", selectedMode.previousStage.pawnKindDef.LabelCap.ToString().Colorize(Color.white)));
			curY += 10f;
		}
		if (selectedMode.displayedStats != null)
		{
			for (int j = 0; j < selectedMode.displayedStats.Count; j++)
			{
				StatDef statDef = selectedMode.displayedStats[j];
				Widgets.Label(rect3.x, ref curY, rect3.width, statDef.LabelCap + ": " + statDef.ValueToString(SelectedKind.race.GetStatValueAbstract(statDef), statDef.toStringNumberSense));
			}
			curY += 10f;
		}
		if (selectedMode.hyperlinks != null)
		{
			foreach (Dialog_InfoCard.Hyperlink item in Dialog_InfoCard.DefsToHyperlinks(selectedMode.hyperlinks))
			{
				Widgets.HyperlinkWithIcon(new Rect(rect3.x, curY, rect3.width, Text.LineHeight), item);
				curY += Text.LineHeight;
			}
			curY += 10f;
		}
		Rect rect4 = new Rect(rect3.x, rect3.yMax - 55f, rect3.width, 55f);
		if (MeetsRequirements(selectedMode) && selectedMode != currentMode)
		{
			if (Widgets.ButtonText(rect4, "Accept".Translate()))
			{
				Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("GauranlenModeChangeDescFull".Translate(treeConnection.parent.Named("TREE"), connectedPawn.Named("CONNECTEDPAWN"), ThingDefOf.DryadCocoon.GetCompProperties<CompProperties_DryadCocoon>().daysToComplete.Named("DURATION")), delegate
				{
					StartChange();
				});
				Find.WindowStack.Add(window);
			}
		}
		else
		{
			string label = ((selectedMode == currentMode) ? ((string)"AlreadySelected".Translate()) : ((!MeetsMemeRequirements(selectedMode)) ? ((string)"MissingRequiredMemes".Translate()) : ((selectedMode.previousStage == null || currentMode == selectedMode.previousStage) ? ((string)"Locked".Translate()) : ((string)("Locked".Translate() + ": " + "MissingRequiredCaste".Translate())))));
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.DrawHighlight(rect4);
			Widgets.Label(rect4.ContractedBy(5f), label);
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}

	private void StartChange()
	{
		treeConnection.desiredMode = selectedMode;
		SoundDefOf.GauranlenProductionModeSet.PlayOneShotOnCamera();
		Close(doCloseSound: false);
	}

	private void DrawRightRect(Rect rect)
	{
		Widgets.DrawMenuSection(rect);
		Rect rect2 = new Rect(0f, 0f, rightViewWidth, rect.height - 16f);
		Rect rect3 = rect2.ContractedBy(10f);
		Widgets.ScrollHorizontal(rect, ref scrollPosition, rect2);
		Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
		Widgets.BeginGroup(rect3);
		DrawDependencyLines(rect3);
		foreach (GauranlenTreeModeDef allDryadMode in allDryadModes)
		{
			DrawDryadStage(rect3, allDryadMode);
		}
		Widgets.EndGroup();
		Widgets.EndScrollView();
	}

	private bool MeetsMemeRequirements(GauranlenTreeModeDef stage)
	{
		if (!Find.IdeoManager.classicMode && !stage.requiredMemes.NullOrEmpty())
		{
			foreach (MemeDef requiredMeme in stage.requiredMemes)
			{
				if (!connectedPawn.Ideo.HasMeme(requiredMeme))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool MeetsRequirements(GauranlenTreeModeDef mode)
	{
		if (mode.previousStage != null && currentMode != mode.previousStage)
		{
			return false;
		}
		return MeetsMemeRequirements(mode);
	}

	private Color GetBoxColor(GauranlenTreeModeDef mode)
	{
		Color result = TexUI.AvailResearchColor;
		if (mode == currentMode)
		{
			result = TexUI.OldActiveResearchColor;
		}
		else if (!MeetsRequirements(mode))
		{
			result = TexUI.LockedResearchColor;
		}
		if (selectedMode == mode)
		{
			result += TexUI.HighlightBgResearchColor;
		}
		return result;
	}

	private Color GetBoxOutlineColor(GauranlenTreeModeDef mode)
	{
		if (selectedMode != null && selectedMode == mode)
		{
			return TexUI.HighlightBorderResearchColor;
		}
		return TexUI.DefaultBorderResearchColor;
	}

	private Color GetTextColor(GauranlenTreeModeDef mode)
	{
		if (!MeetsRequirements(mode))
		{
			return ColorLibrary.RedReadable;
		}
		return Color.white;
	}

	private void DrawDependencyLines(Rect fullRect)
	{
		foreach (GauranlenTreeModeDef allDryadMode in allDryadModes)
		{
			if (allDryadMode.previousStage != null)
			{
				DrawLineBetween(allDryadMode, allDryadMode.previousStage, fullRect.height, TexUI.DefaultLineResearchColor);
			}
		}
		foreach (GauranlenTreeModeDef allDryadMode2 in allDryadModes)
		{
			if (allDryadMode2.previousStage != null && (allDryadMode2.previousStage == selectedMode || selectedMode == allDryadMode2))
			{
				DrawLineBetween(allDryadMode2, allDryadMode2.previousStage, fullRect.height, TexUI.HighlightLineResearchColor, 3f);
			}
		}
	}

	private void DrawDryadStage(Rect rect, GauranlenTreeModeDef stage)
	{
		Vector2 position = GetPosition(stage, rect.height);
		Rect rect2 = new Rect(position.x, position.y, OptionSize.x, OptionSize.y);
		Widgets.DrawBoxSolidWithOutline(rect2, GetBoxColor(stage), GetBoxOutlineColor(stage));
		Rect rect3 = new Rect(rect2.x, rect2.y, rect2.height, rect2.height);
		Widgets.DefIcon(rect3.ContractedBy(4f), stage.pawnKindDef);
		GUI.color = GetTextColor(stage);
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(new Rect(rect3.xMax, rect2.y, rect2.width - rect3.width, rect2.height).ContractedBy(4f), stage.LabelCap);
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = Color.white;
		if (Widgets.ButtonInvisible(rect2))
		{
			selectedMode = stage;
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
	}

	private void DrawLineBetween(GauranlenTreeModeDef left, GauranlenTreeModeDef right, float height, Color color, float width = 2f)
	{
		Vector2 start = GetPosition(left, height) + new Vector2(5f, OptionSize.y / 2f);
		Vector2 end = GetPosition(right, height) + OptionSize / 2f;
		Widgets.DrawLine(start, end, color, width);
	}

	private Vector2 GetPosition(GauranlenTreeModeDef stage, float height)
	{
		return new Vector2(stage.drawPosition.x * OptionSize.x + stage.drawPosition.x * 52f, (height - OptionSize.y) * stage.drawPosition.y);
	}
}
