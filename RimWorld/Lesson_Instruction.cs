using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class Lesson_Instruction : Lesson
{
	public InstructionDef def;

	private const float RectWidth = 310f;

	private const float BarHeight = 30f;

	protected Map Map => Find.AnyPlayerHomeMap;

	protected virtual float ProgressPercent => -1f;

	protected virtual bool ShowProgressBar => ProgressPercent >= 0f;

	public override string DefaultRejectInputMessage => def.rejectInputMessage;

	public override InstructionDef Instruction => def;

	public override void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		base.ExposeData();
	}

	public override void OnActivated()
	{
		base.OnActivated();
		if (def.giveOnActivateCount > 0)
		{
			Thing thing = ThingMaker.MakeThing(def.giveOnActivateDef);
			thing.stackCount = def.giveOnActivateCount;
			GenSpawn.Spawn(thing, TutorUtility.FindUsableRect(2, 2, Map).CenterCell, Map);
		}
		if (!def.resetBuildDesignatorStuffs)
		{
			return;
		}
		foreach (DesignationCategoryDef allDef in DefDatabase<DesignationCategoryDef>.AllDefs)
		{
			foreach (Designator resolvedAllowedDesignator in allDef.ResolvedAllowedDesignators)
			{
				if (resolvedAllowedDesignator is Designator_Build designator_Build)
				{
					designator_Build.ResetStuffToDefault();
				}
			}
		}
	}

	public override void LessonOnGUI()
	{
		Text.Font = GameFont.Small;
		string textAdj = def.Text.AdjustedForKeys();
		float num = Text.CalcHeight(textAdj, 290f) + 20f;
		if (ShowProgressBar)
		{
			num += 47f;
		}
		Vector2 b = new Vector2((float)UI.screenWidth - 17f - 155f, 17f + num / 2f);
		if (!Find.TutorialState.introDone)
		{
			float screenOverlayAlpha = 0f;
			if (def.startCentered)
			{
				Vector2 vector = new Vector2(UI.screenWidth / 2, UI.screenHeight / 2);
				if (base.AgeSeconds < 4f)
				{
					b = vector;
					screenOverlayAlpha = 0.9f;
				}
				else if (base.AgeSeconds < 5f)
				{
					float t = (base.AgeSeconds - 4f) / 1f;
					b = Vector2.Lerp(vector, b, t);
					screenOverlayAlpha = Mathf.Lerp(0.9f, 0f, t);
				}
			}
			if (screenOverlayAlpha > 0f)
			{
				Rect fullScreenRect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
				Find.WindowStack.ImmediateWindow(972651, fullScreenRect, WindowLayer.SubSuper, delegate
				{
					GUI.color = new Color(1f, 1f, 1f, screenOverlayAlpha);
					GUI.DrawTexture(fullScreenRect, BaseContent.BlackTex);
					GUI.color = Color.white;
				}, doBackground: false, absorbInputAroundWindow: true, 0f);
			}
			else
			{
				Find.TutorialState.introDone = true;
			}
		}
		Rect mainRect = new Rect(b.x - 155f, b.y - num / 2f - 10f, 310f, num);
		if (Find.TutorialState.introDone && Find.WindowStack.IsOpen<Page_ConfigureStartingPawns>())
		{
			Rect rect = mainRect;
			rect.x = 17f;
			if ((mainRect.Contains(Event.current.mousePosition) || (def == InstructionDefOf.RandomizeCharacter && UI.screenHeight <= 768)) && !rect.Contains(Event.current.mousePosition))
			{
				mainRect.x = 17f;
			}
		}
		Find.WindowStack.ImmediateWindow(177706, mainRect, WindowLayer.Super, delegate
		{
			Rect rect2 = mainRect.AtZero();
			Widgets.DrawWindowBackgroundTutor(rect2);
			Rect rect3 = rect2.ContractedBy(10f);
			Text.Font = GameFont.Small;
			Rect rect4 = rect3;
			if (ShowProgressBar)
			{
				rect4.height -= 47f;
			}
			Widgets.Label(rect4, textAdj);
			if (ShowProgressBar)
			{
				Widgets.FillableBar(new Rect(rect3.x, rect3.yMax - 30f, rect3.width, 30f), ProgressPercent, LearningReadout.ProgressBarFillTex);
			}
			if (base.AgeSeconds < 0.5f)
			{
				GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, base.AgeSeconds / 0.5f));
				GUI.DrawTexture(rect2, BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
		}, doBackground: false);
		if (def.highlightTags != null)
		{
			for (int num2 = 0; num2 < def.highlightTags.Count; num2++)
			{
				UIHighlighter.HighlightTag(def.highlightTags[num2]);
			}
		}
	}

	public override void Notify_Event(EventPack ep)
	{
		if (def.eventTagsEnd != null && def.eventTagsEnd.Contains(ep.Tag))
		{
			Find.ActiveLesson.Deactivate();
		}
	}

	public override AcceptanceReport AllowAction(EventPack ep)
	{
		return def.actionTagsAllowed != null && def.actionTagsAllowed.Contains(ep.Tag);
	}

	public override void PostDeactivated()
	{
		SoundDefOf.CommsWindow_Close.PlayOneShotOnCamera();
		TutorSystem.Notify_Event("InstructionDeactivated-" + def.defName);
		if (def.endTutorial)
		{
			Find.ActiveLesson.Deactivate();
			Find.TutorialState.Notify_TutorialEnding();
			LessonAutoActivator.Notify_TutorialEnding();
		}
	}
}
