using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

public class Screen_ArchonexusSettlementCinematics : Window
{
	private Action cameraJumpAction;

	private Action nextStepAction;

	private bool FadeInLatch;

	private float ScreenStartTime;

	public const float FadeSecs = 2f;

	private const float MessageDisplaySecs = 7f;

	private const float FadeBuffer = 0.2f;

	private const int MessageWidth = 800;

	public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

	protected override float Margin => 0f;

	private float FadeToBlackEndTime => ScreenStartTime + 2f + 0.2f;

	private float MessageDisplayEndTime => FadeToBlackEndTime + 7f;

	public Screen_ArchonexusSettlementCinematics(Action cameraJumpAction, Action nextStepAction)
	{
		doWindowBackground = false;
		doCloseButton = false;
		doCloseX = false;
		forcePause = true;
		this.cameraJumpAction = cameraJumpAction;
		this.nextStepAction = nextStepAction;
	}

	public override void PreOpen()
	{
		base.PreOpen();
		Find.MusicManagerPlay.ForceFadeoutAndSilenceFor(11.2f, 1.5f);
		SoundDefOf.ArchonexusNewColonyAccept.PlayOneShotOnCamera();
		ScreenFader.StartFade(Color.black, 2f);
		ScreenStartTime = Time.realtimeSinceStartup;
	}

	public override void PostClose()
	{
		base.PostOpen();
		ScreenFader.SetColor(Color.black);
		nextStepAction();
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (IsFinishedFadingIn())
		{
			if (!FadeInLatch)
			{
				FadeInLatch = true;
				cameraJumpAction();
				ScreenFader.SetColor(Color.clear);
			}
			if (IsFinishedDisplayMessage())
			{
				Close(doCloseSound: false);
				return;
			}
			Rect rect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
			GUI.DrawTexture(rect, BaseContent.BlackTex);
			Rect rect2 = new Rect(rect);
			rect2.xMin = rect.center.x - 400f;
			rect2.width = 800f;
			rect2.yMin = rect.center.y;
			GameFont font = Text.Font;
			TextAnchor anchor = Text.Anchor;
			Color color = GUI.color;
			Text.Font = GameFont.Medium;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(new Rect(inRect), "SoldColonyDescription".Translate());
			Text.Font = font;
			GUI.color = color;
			Text.Anchor = anchor;
		}
	}

	public bool IsFinishedFadingIn()
	{
		return Time.realtimeSinceStartup > FadeToBlackEndTime;
	}

	public bool IsFinishedDisplayMessage()
	{
		return Time.realtimeSinceStartup > MessageDisplayEndTime;
	}
}
