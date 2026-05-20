using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Dialog_DebugSetHediffRemaining : Window
{
	private Hediff hediff;

	private const float HeaderLabelHeight = 40f;

	private const float ButtonHeight = 30f;

	private const float ButtonWidth = 40f;

	private const float Padding = 10f;

	private static readonly Vector2 InitialPositionShift = new Vector2(4f, 0f);

	private HediffComp_Disappears Comp => hediff.TryGetComp<HediffComp_Disappears>();

	public override Vector2 InitialSize => new Vector2(300f, 125f);

	public Dialog_DebugSetHediffRemaining(Hediff hediff, IWindowDrawing customWindowDrawing = null)
		: base(customWindowDrawing)
	{
		this.hediff = hediff;
		layer = WindowLayer.Super;
		closeOnClickedOutside = true;
		doWindowBackground = false;
		drawShadow = false;
		preventCameraMotion = false;
		SoundDefOf.FloatMenu_Open.PlayOneShotOnCamera();
	}

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

	public override void DoWindowContents(Rect inRect)
	{
		if (hediff == null || Comp == null || !hediff.pawn.health.hediffSet.hediffs.Contains(hediff))
		{
			Close();
		}
		Widgets.DrawWindowBackground(inRect);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), hediff.def.defName + " time remaining");
		Text.Anchor = TextAnchor.UpperLeft;
		float num = 190f;
		float num2 = inRect.width / 2f - num / 2f;
		if (Widgets.ButtonText(new Rect(num2, 50f, 40f, 30f), "1d"))
		{
			SetTicks(60000);
		}
		float num3 = num2 + 50f;
		if (Widgets.ButtonText(new Rect(num3, 50f, 40f, 30f), "12h"))
		{
			SetTicks(30000);
		}
		float num4 = num3 + 50f;
		if (Widgets.ButtonText(new Rect(num4, 50f, 40f, 30f), "6h"))
		{
			SetTicks(15000);
		}
		if (Widgets.ButtonText(new Rect(num4 + 50f, 50f, 40f, 30f), "1h"))
		{
			SetTicks(2500);
		}
	}

	private void SetTicks(int ticks)
	{
		Comp.ticksToDisappear = ticks;
		SoundDefOf.Click.PlayOneShotOnCamera();
		Close();
	}
}
