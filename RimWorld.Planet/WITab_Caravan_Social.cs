using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

public class WITab_Caravan_Social : WITab
{
	private Vector2 scrollPosition;

	private float scrollViewHeight;

	private Pawn specificSocialTabForPawn;

	private const float RowHeight = 34f;

	private const float ScrollViewTopMargin = 15f;

	private const float PawnLabelHeight = 18f;

	private const float PawnLabelColumnWidth = 100f;

	private const float SpaceAroundIcon = 4f;

	private List<Pawn> Pawns => base.SelCaravan.PawnsListForReading;

	private float SpecificSocialTabWidth
	{
		get
		{
			EnsureSpecificSocialTabForPawnValid();
			if (specificSocialTabForPawn.DestroyedOrNull())
			{
				return 0f;
			}
			return 540f;
		}
	}

	public WITab_Caravan_Social()
	{
		labelKey = "TabCaravanSocial";
	}

	protected override void FillTab()
	{
		EnsureSpecificSocialTabForPawnValid();
		Text.Font = GameFont.Small;
		Rect rect = new Rect(0f, 15f, size.x, size.y - 15f).ContractedBy(10f);
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
		float curY = 0f;
		Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
		DoRows(ref curY, rect2, rect);
		if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = curY + 30f;
		}
		Widgets.EndScrollView();
	}

	protected override void UpdateSize()
	{
		EnsureSpecificSocialTabForPawnValid();
		base.UpdateSize();
		size.x = 243f;
		size.y = Mathf.Min(550f, PaneTopY - 30f);
	}

	protected override void ExtraOnGUI()
	{
		EnsureSpecificSocialTabForPawnValid();
		base.ExtraOnGUI();
		Pawn localSpecificSocialTabForPawn = specificSocialTabForPawn;
		if (localSpecificSocialTabForPawn == null)
		{
			return;
		}
		Rect tabRect = base.TabRect;
		float specificSocialTabWidth = SpecificSocialTabWidth;
		Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificSocialTabWidth, tabRect.height);
		Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
		{
			if (!localSpecificSocialTabForPawn.DestroyedOrNull())
			{
				SocialCardUtility.DrawSocialCard(rect.AtZero(), localSpecificSocialTabForPawn);
				if (Widgets.CloseButtonFor(rect.AtZero()))
				{
					specificSocialTabForPawn = null;
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
			}
		});
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if ((specificSocialTabForPawn == null || !Pawns.Contains(specificSocialTabForPawn)) && Pawns.Any())
		{
			specificSocialTabForPawn = Pawns[0];
		}
	}

	private void DoRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
	{
		List<Pawn> pawns = Pawns;
		Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(base.SelCaravan);
		GUI.color = new Color(0.8f, 0.8f, 0.8f, 1f);
		Widgets.Label(new Rect(0f, curY, scrollViewRect.width, 24f), string.Format("{0}: {1}", "Negotiator".TranslateSimple(), (pawn != null) ? pawn.LabelShort : "NoneCapable".Translate().ToString()));
		curY += 24f;
		if (specificSocialTabForPawn != null && !pawns.Contains(specificSocialTabForPawn))
		{
			specificSocialTabForPawn = null;
		}
		bool flag = false;
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn2 = pawns[i];
			if (pawn2.RaceProps.IsFlesh && pawn2.IsColonist)
			{
				if (!flag)
				{
					Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
					flag = true;
				}
				DoRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
			}
		}
		bool flag2 = false;
		for (int j = 0; j < pawns.Count; j++)
		{
			Pawn pawn3 = pawns[j];
			if (pawn3.RaceProps.IsFlesh && !pawn3.IsColonist)
			{
				if (!flag2)
				{
					Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanPrisonersAndAnimals".Translate());
					flag2 = true;
				}
				DoRow(ref curY, scrollViewRect, scrollOutRect, pawn3);
			}
		}
	}

	private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
	{
		float num = scrollPosition.y - 34f;
		float num2 = scrollPosition.y + scrollOutRect.height;
		if (curY > num && curY < num2)
		{
			DoRow(new Rect(0f, curY, viewRect.width, 34f), p);
		}
		curY += 34f;
	}

	private void DoRow(Rect rect, Pawn p)
	{
		Widgets.BeginGroup(rect);
		Rect rect2 = rect.AtZero();
		CaravanThingsTabUtility.DoAbandonButton(rect2, p, base.SelCaravan);
		rect2.width -= 24f;
		Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
		rect2.width -= 24f;
		CaravanThingsTabUtility.DoOpenSpecificTabButton(rect2, p, ref specificSocialTabForPawn);
		rect2.width -= 24f;
		CaravanThingsTabUtility.DoOpenSpecificTabButtonInvisible(rect2, p, ref specificSocialTabForPawn);
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
		}
		Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
		Widgets.ThingIcon(rect3, p);
		Rect bgRect = new Rect(rect3.xMax + 4f, 8f, 100f, 18f);
		GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, alwaysDrawBg: false, alignCenter: false);
		if (p.Downed && !p.ageTracker.CurLifeStage.alwaysDowned)
		{
			GUI.color = new Color(1f, 0f, 0f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
			GUI.color = Color.white;
		}
		Widgets.EndGroup();
	}

	public override void Notify_ClearingAllMapsMemory()
	{
		base.Notify_ClearingAllMapsMemory();
		specificSocialTabForPawn = null;
	}

	private void EnsureSpecificSocialTabForPawnValid()
	{
		if (specificSocialTabForPawn != null && (specificSocialTabForPawn.Destroyed || !base.SelCaravan.ContainsPawn(specificSocialTabForPawn)))
		{
			specificSocialTabForPawn = null;
		}
	}
}
