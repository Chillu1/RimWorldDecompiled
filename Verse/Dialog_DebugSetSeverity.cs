using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Dialog_DebugSetSeverity : Window
{
	private Hediff hediff;

	private float maxSeverity;

	private float sliderValue;

	private static readonly Vector2 InitialPositionShift = new Vector2(4f, 0f);

	private readonly Vector2 BottomButtonSize = new Vector2(100f, 20f);

	private const float HeaderLabelHeight = 40f;

	private const float Padding = 20f;

	public override Vector2 InitialSize => new Vector2(300f, 125f);

	protected override void SetInitialSizeAndPosition()
	{
		Vector2 vector = UI.MousePositionOnUIInverted + InitialPositionShift;
		if (vector.x + InitialSize.x > (float)UI.screenWidth)
		{
			vector.x = (float)UI.screenWidth - InitialSize.x;
		}
		if (vector.y + InitialSize.y > (float)UI.screenHeight)
		{
			vector.y = (float)UI.screenHeight - InitialSize.y;
		}
		windowRect = new Rect(vector.x, vector.y, InitialSize.x, InitialSize.y);
	}

	public Dialog_DebugSetSeverity(Hediff hediff)
	{
		this.hediff = hediff;
		maxSeverity = ((hediff.def.maxSeverity >= float.MaxValue) ? 1f : hediff.def.maxSeverity);
		sliderValue = Mathf.InverseLerp(hediff.def.minSeverity, maxSeverity, hediff.Severity);
		layer = WindowLayer.Super;
		closeOnClickedOutside = true;
		doWindowBackground = false;
		drawShadow = false;
		preventCameraMotion = false;
		SoundDefOf.FloatMenu_Open.PlayOneShotOnCamera();
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (hediff == null || !hediff.pawn.health.hediffSet.hediffs.Contains(hediff))
		{
			Close();
		}
		Widgets.DrawWindowBackground(inRect);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), hediff.def.defName + " severity");
		Text.Anchor = TextAnchor.UpperLeft;
		Rect position = inRect;
		position.y = 40f;
		position.height = 10f;
		position.x += 20f;
		position.width -= 40f;
		sliderValue = GUI.HorizontalSlider(position, sliderValue, 0f, 1f);
		Rect rect = inRect;
		rect.y = inRect.height - 10f - BottomButtonSize.y;
		rect.height = BottomButtonSize.y;
		rect.x += 20f;
		rect.width -= 40f;
		float num = Mathf.Round(sliderValue * 100f);
		if (Widgets.ButtonText(rect, $"Set to {num}%"))
		{
			float severity = Mathf.Lerp(hediff.def.minSeverity, maxSeverity, Mathf.Clamp01(num / 100f));
			hediff.Severity = severity;
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
	}
}
