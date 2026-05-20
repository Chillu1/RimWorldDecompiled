using System;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Dialog_NodeTree : Window
{
	private Vector2 scrollPosition;

	private Vector2 optsScrollPosition;

	protected string title;

	protected DiaNode curNode;

	public Action closeAction;

	private float makeInteractiveAtTime;

	public Color screenFillColor = Color.clear;

	protected float minOptionsAreaHeight;

	private const float InteractivityDelay = 1f;

	private const float TitleHeight = 36f;

	protected const float OptHorMargin = 15f;

	protected const float OptVerticalSpace = 7f;

	private const int ResizeIfMoreOptionsThan = 5;

	private const float MinSpaceLeftForTextAfterOptionsResizing = 100f;

	private float optTotalHeight;

	public override Vector2 InitialSize
	{
		get
		{
			int num = 480;
			if (curNode.options.Count > 5)
			{
				Text.Font = GameFont.Small;
				num += (curNode.options.Count - 5) * (int)(Text.LineHeight + 7f);
			}
			return new Vector2(620f, Mathf.Min(num, UI.screenHeight));
		}
	}

	private bool InteractiveNow => Time.realtimeSinceStartup >= makeInteractiveAtTime;

	public Dialog_NodeTree(DiaNode nodeRoot, bool delayInteractivity = false, bool radioMode = false, string title = null)
	{
		this.title = title;
		GotoNode(nodeRoot);
		forcePause = true;
		absorbInputAroundWindow = true;
		closeOnAccept = false;
		closeOnCancel = false;
		if (delayInteractivity)
		{
			makeInteractiveAtTime = RealTime.LastRealTime + 1f;
		}
		soundAppear = SoundDefOf.CommsWindow_Open;
		soundClose = SoundDefOf.CommsWindow_Close;
		if (radioMode)
		{
			soundAmbient = SoundDefOf.RadioComms_Ambience;
		}
	}

	public override void PreClose()
	{
		base.PreClose();
		curNode.PreClose();
	}

	public override void PostClose()
	{
		base.PostClose();
		if (closeAction != null)
		{
			closeAction();
		}
	}

	public override void WindowOnGUI()
	{
		if (screenFillColor != Color.clear)
		{
			GUI.color = screenFillColor;
			GUI.DrawTexture(new Rect(0f, 0f, UI.screenWidth, UI.screenHeight), BaseContent.WhiteTex);
			GUI.color = Color.white;
		}
		base.WindowOnGUI();
	}

	public override void DoWindowContents(Rect inRect)
	{
		Rect rect = inRect.AtZero();
		if (title != null)
		{
			Text.Font = GameFont.Small;
			Rect rect2 = rect;
			rect2.height = 36f;
			rect.yMin += 53f;
			Widgets.DrawTitleBG(rect2);
			rect2.xMin += 9f;
			rect2.yMin += 5f;
			Widgets.Label(rect2, title);
		}
		DrawNode(rect);
	}

	protected void DrawNode(Rect rect)
	{
		Widgets.BeginGroup(rect);
		Text.Font = GameFont.Small;
		float num = Mathf.Min(optTotalHeight, rect.height - 100f - Margin * 2f);
		Rect outRect = new Rect(0f, 0f, rect.width, rect.height - Mathf.Max(num, minOptionsAreaHeight));
		float width = rect.width - 16f;
		Rect rect2 = new Rect(0f, 0f, width, Text.CalcHeight(curNode.text, width));
		Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
		Widgets.Label(rect2, curNode.text.Resolve());
		Widgets.EndScrollView();
		Widgets.BeginScrollView(new Rect(0f, rect.height - num, rect.width, num), ref optsScrollPosition, new Rect(0f, 0f, rect.width - 16f, optTotalHeight));
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < curNode.options.Count; i++)
		{
			Rect rect3 = new Rect(15f, num2, rect.width - 30f, 999f);
			float num4 = curNode.options[i].OptOnGUI(rect3, InteractiveNow);
			num2 += num4 + 7f;
			num3 += num4 + 7f;
		}
		if (Event.current.type == EventType.Layout)
		{
			optTotalHeight = num3;
		}
		Widgets.EndScrollView();
		Widgets.EndGroup();
	}

	public void GotoNode(DiaNode node)
	{
		foreach (DiaOption option in node.options)
		{
			option.dialog = this;
		}
		curNode = node;
	}
}
