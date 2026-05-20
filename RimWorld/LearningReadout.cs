using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class LearningReadout : IExposable
{
	private List<ConceptDef> activeConcepts = new List<ConceptDef>();

	private ConceptDef selectedConcept;

	private bool showAllMode;

	private float contentHeight;

	private Vector2 scrollPosition = Vector2.zero;

	private string searchString = "";

	private float lastConceptActivateRealTime = -999f;

	private ConceptDef mouseoverConcept;

	private Rect windowRect;

	private Action windowOnGUICached;

	private const float OuterMargin = 8f;

	private const float InnerMargin = 7f;

	private const float ReadoutWidth = 200f;

	private const float InfoPaneWidth = 310f;

	private const float OpenButtonSize = 24f;

	public static readonly Texture2D ProgressBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(38f / 51f, 0.6039216f, 0.2f));

	public static readonly Texture2D ProgressBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(26f / 51f, 0.40784314f, 2f / 15f));

	private static List<ConceptDef> tmpConceptsToShow = new List<ConceptDef>();

	public int ActiveConceptsCount => activeConcepts.Count;

	public bool ShowAllMode => showAllMode;

	public LearningReadout()
	{
		windowOnGUICached = WindowOnGUI;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref activeConcepts, "activeConcepts", LookMode.Undefined);
		Scribe_Defs.Look(ref selectedConcept, "selectedConcept");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			activeConcepts.RemoveAll((ConceptDef c) => PlayerKnowledgeDatabase.IsComplete(c));
		}
	}

	public bool TryActivateConcept(ConceptDef conc)
	{
		if (activeConcepts.Contains(conc))
		{
			return false;
		}
		activeConcepts.Add(conc);
		SoundDefOf.Lesson_Activated.PlayOneShotOnCamera();
		lastConceptActivateRealTime = RealTime.LastRealTime;
		return true;
	}

	public bool IsActive(ConceptDef conc)
	{
		return activeConcepts.Contains(conc);
	}

	public void LearningReadoutUpdate()
	{
	}

	public void Notify_ConceptNewlyLearned(ConceptDef conc)
	{
		if (activeConcepts.Contains(conc) || selectedConcept == conc)
		{
			SoundDefOf.Lesson_Deactivated.PlayOneShotOnCamera();
			SoundDefOf.CommsWindow_Close.PlayOneShotOnCamera();
		}
		if (activeConcepts.Contains(conc))
		{
			activeConcepts.Remove(conc);
		}
		if (selectedConcept == conc)
		{
			selectedConcept = null;
		}
	}

	private string FilterSearchStringInput(string input)
	{
		if (input == searchString)
		{
			return input;
		}
		if (input.Length > 20)
		{
			input = input.Substring(0, 20);
		}
		return input;
	}

	public void LearningReadoutOnGUI()
	{
		if (!TutorSystem.TutorialMode && TutorSystem.AdaptiveTrainingEnabled && (Find.PlaySettings.showLearningHelper || activeConcepts.Count != 0) && !Find.WindowStack.IsOpen<Screen_Credits>())
		{
			float b = (float)UI.screenHeight / 2f;
			float a = contentHeight + 14f;
			windowRect = new Rect((float)UI.screenWidth - 8f - 200f, 8f, 200f, Mathf.Min(a, b));
			Rect rect = windowRect;
			Find.WindowStack.ImmediateWindow(76136312, windowRect, WindowLayer.Super, windowOnGUICached, doBackground: false);
			float num = Time.realtimeSinceStartup - lastConceptActivateRealTime;
			if (num < 1f && num > 0f)
			{
				GenUI.DrawFlash(rect.x, rect.center.y, (float)UI.screenWidth * 0.6f, Pulser.PulseBrightness(1f, 1f, num) * 0.85f, new Color(0.8f, 0.77f, 0.53f));
			}
			ConceptDef conceptDef = ((selectedConcept != null) ? selectedConcept : mouseoverConcept);
			if (conceptDef != null)
			{
				DrawInfoPane(conceptDef);
				conceptDef.HighlightAllTags();
			}
			mouseoverConcept = null;
		}
	}

	private void WindowOnGUI()
	{
		Rect rect = windowRect.AtZero().ContractedBy(7f);
		bool flag = contentHeight > rect.height;
		Widgets.DrawWindowBackgroundTutor(windowRect.AtZero());
		float y = rect.y;
		Text.Font = GameFont.Small;
		Rect rect2 = new Rect(rect.x, y, rect.width - 24f, 24f);
		Widgets.Label(rect2, "LearningHelper".Translate());
		y += rect2.height;
		if (Widgets.ButtonImage(new Rect(rect2.xMax, rect2.y, 24f, 24f), (!showAllMode) ? TexButton.Plus : TexButton.Minus))
		{
			showAllMode = !showAllMode;
			if (showAllMode)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
		}
		if (showAllMode)
		{
			Rect rect3 = new Rect(rect.x, y, rect.width - 20f - 2f, 28f);
			searchString = FilterSearchStringInput(Widgets.TextField(rect3, searchString));
			if (searchString == "")
			{
				GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect4 = rect3;
				rect4.xMin += 7f;
				Widgets.Label(rect4, "Filter".Translate() + "...");
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			if (Widgets.ButtonImage(new Rect(rect3.xMax + 4f, y + 14f - 10f, 20f, 20f), TexButton.CloseXSmall))
			{
				searchString = "";
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			}
			y += 32f;
		}
		tmpConceptsToShow.Clear();
		if (showAllMode)
		{
			tmpConceptsToShow.AddRange(DefDatabase<ConceptDef>.AllDefsListForReading);
		}
		else
		{
			tmpConceptsToShow.AddRange(activeConcepts);
		}
		if (tmpConceptsToShow.Any())
		{
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(rect.x, y, rect.width);
			GUI.color = Color.white;
			y += 4f;
		}
		float num = y - rect.y;
		rect.yMin = y;
		Rect viewRect = rect.AtZero();
		if (flag)
		{
			viewRect.height = contentHeight - num;
			viewRect.width -= 20f;
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
		}
		else
		{
			Widgets.BeginGroup(rect);
		}
		y = 0f;
		if (showAllMode)
		{
			tmpConceptsToShow.SortBy((ConceptDef x) => -DisplayPriority(x), (ConceptDef x) => x.label);
		}
		for (int num2 = 0; num2 < tmpConceptsToShow.Count; num2++)
		{
			if (!tmpConceptsToShow[num2].TriggeredDirect)
			{
				y = DrawConceptListRow(0f, y, viewRect.width, tmpConceptsToShow[num2]).yMax;
			}
		}
		tmpConceptsToShow.Clear();
		contentHeight = num + y;
		if (flag)
		{
			Widgets.EndScrollView();
		}
		else
		{
			Widgets.EndGroup();
		}
	}

	private int DisplayPriority(ConceptDef conc)
	{
		int num = 1;
		if (MatchesSearchString(conc))
		{
			num += 10000;
		}
		return num;
	}

	private bool MatchesSearchString(ConceptDef conc)
	{
		if (searchString != "")
		{
			return conc.label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return false;
	}

	private Rect DrawConceptListRow(float x, float y, float width, ConceptDef conc)
	{
		float knowledge = PlayerKnowledgeDatabase.GetKnowledge(conc);
		bool num = PlayerKnowledgeDatabase.IsComplete(conc);
		bool num2 = !num && knowledge > 0f;
		float num3 = Text.CalcHeight(conc.LabelCap, width);
		if (num2)
		{
			num3 += 0f;
		}
		Rect rect = new Rect(x, y, width, num3);
		if (num2)
		{
			Rect rect2 = new Rect(rect);
			rect2.yMin += 1f;
			rect2.yMax -= 1f;
			Widgets.FillableBar(rect2, PlayerKnowledgeDatabase.GetKnowledge(conc), ProgressBarFillTex, ProgressBarBGTex, doBorder: false);
		}
		if (num)
		{
			GUI.DrawTexture(rect, BaseContent.GreyTex);
		}
		if (selectedConcept == conc)
		{
			GUI.DrawTexture(rect, TexUI.HighlightSelectedTex);
		}
		Widgets.DrawHighlightIfMouseover(rect);
		if (MatchesSearchString(conc))
		{
			Widgets.DrawHighlight(rect);
		}
		Widgets.Label(rect, conc.LabelCap);
		if (Mouse.IsOver(rect) && selectedConcept == null)
		{
			mouseoverConcept = conc;
		}
		if (Widgets.ButtonInvisible(rect))
		{
			if (selectedConcept == conc)
			{
				selectedConcept = null;
			}
			else
			{
				selectedConcept = conc;
			}
			SoundDefOf.PageChange.PlayOneShotOnCamera();
		}
		return rect;
	}

	private Rect DrawInfoPane(ConceptDef conc)
	{
		float knowledge = PlayerKnowledgeDatabase.GetKnowledge(conc);
		bool complete = PlayerKnowledgeDatabase.IsComplete(conc);
		bool drawProgressBar = !complete && knowledge > 0f;
		Text.Font = GameFont.Medium;
		float titleHeight = Text.CalcHeight(conc.LabelCap, 276f);
		Text.Font = GameFont.Small;
		float textHeight = Text.CalcHeight(conc.HelpTextAdjusted, 296f);
		float num = titleHeight + textHeight + 14f + 5f;
		if (selectedConcept == conc)
		{
			num += 40f;
		}
		if (drawProgressBar)
		{
			num += 30f;
		}
		Rect outRect = new Rect((float)UI.screenWidth - 8f - 200f - 8f - 310f, 8f, 310f, num);
		Rect result = outRect;
		Find.WindowStack.ImmediateWindow(987612111, outRect, WindowLayer.Super, delegate
		{
			outRect = outRect.AtZero();
			Rect rect = outRect.ContractedBy(7f);
			Widgets.DrawShadowAround(outRect);
			Widgets.DrawWindowBackgroundTutor(outRect);
			Rect rect2 = rect;
			rect2.width -= 20f;
			rect2.height = titleHeight + 5f;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect2, conc.LabelCap);
			Text.Font = GameFont.Small;
			Rect rect3 = rect;
			rect3.yMin = rect2.yMax;
			rect3.height = textHeight;
			Widgets.Label(rect3, conc.HelpTextAdjusted);
			if (drawProgressBar)
			{
				Rect rect4 = rect;
				rect4.yMin = rect3.yMax;
				rect4.height = 30f;
				Widgets.FillableBar(rect4, PlayerKnowledgeDatabase.GetKnowledge(conc), ProgressBarFillTex);
			}
			if (selectedConcept == conc)
			{
				if (Widgets.CloseButtonFor(outRect))
				{
					selectedConcept = null;
					SoundDefOf.PageChange.PlayOneShotOnCamera();
				}
				Rect rect5 = new Rect(rect.center.x - 70f, rect.yMax - 30f, 140f, 30f);
				if (!complete)
				{
					if (Widgets.ButtonText(rect5, "MarkLearned".Translate()))
					{
						selectedConcept = null;
						SoundDefOf.PageChange.PlayOneShotOnCamera();
						PlayerKnowledgeDatabase.SetKnowledge(conc, 1f);
					}
				}
				else
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect5, "AlreadyLearned".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = Color.white;
				}
			}
		}, doBackground: false);
		return result;
	}
}
