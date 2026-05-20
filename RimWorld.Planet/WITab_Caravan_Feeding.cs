using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public class WITab_Caravan_Feeding : WITab
	{
		private Vector2 scrollPosition;

		private Pawn selectedPawn;

		private Vector2 scrollPositionAutoBreastfeed;

		private Vector2 scrollPositionBabyConsumables;

		private const int contextHash = 1384745000;

		private const float RowHeight = 34f;

		private const float ScrollViewTopMargin = 15f;

		private const float PawnLabelHeight = 18f;

		private const float PawnLabelColumnWidth = 100f;

		private const float SpaceAroundIcon = 4f;

		private const float BabyPickWindowWidth = 250f;

		private List<Pawn> Pawns => base.SelCaravan.PawnsListForReading;

		public override bool IsVisible
		{
			get
			{
				ChildcareUtility.BreastfeedFailReason? reason;
				return Pawns.Any((Pawn pawn) => ChildcareUtility.CanSuckle(pawn, out reason));
			}
		}

		public WITab_Caravan_Feeding()
		{
			labelKey = "TabCaravanFeeding";
		}

		protected override void UpdateSize()
		{
			size = new Vector2(250f, Mathf.Min(550f, PaneTopY - 30f));
			base.UpdateSize();
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			RectDivider divider = new RectDivider(new Rect(0f, 15f, size.x, size.y - 15f).ContractedBy(10f), 1384745000);
			Widgets.ListSeparator(ref divider, "Babies".Translate().CapitalizeFirst());
			ChildcareUtility.BreastfeedFailReason? reason;
			RectDivider scrollViewRect = divider.CreateViewRect(Pawns.Count((Pawn pawn) => ChildcareUtility.CanSuckle(pawn, out reason)), 34f);
			Widgets.BeginScrollView(divider, ref scrollPosition, scrollViewRect);
			DoRows(ref scrollViewRect);
			Widgets.EndScrollView();
		}

		private void DoRows(ref RectDivider scrollViewRect)
		{
			if (selectedPawn != null && (selectedPawn.Destroyed || !base.SelCaravan.ContainsPawn(selectedPawn)))
			{
				selectedPawn = null;
			}
			foreach (Pawn pawn in Pawns)
			{
				if (ChildcareUtility.CanSuckle(pawn, out var _))
				{
					DoRow(scrollViewRect.NewRow(34f), pawn);
				}
			}
		}

		private void DoRow(Rect rect, Pawn pawn)
		{
			Widgets.BeginGroup(rect);
			Rect rect2 = rect.AtZero();
			CaravanThingsTabUtility.DoAbandonButton(rect2, pawn, base.SelCaravan);
			rect2.width -= 24f;
			Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, pawn);
			rect2.width -= 24f;
			CaravanThingsTabUtility.DoOpenSpecificTabButton(rect2, pawn, ref selectedPawn);
			rect2.width -= 24f;
			CaravanThingsTabUtility.DoOpenSpecificTabButtonInvisible(rect2, pawn, ref selectedPawn);
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
			}
			Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
			Widgets.ThingIcon(rect3, pawn);
			Rect bgRect = new Rect(rect3.xMax + 4f, 8f, 100f, 18f);
			GenMapUI.DrawPawnLabel(pawn, bgRect, 1f, 100f, null, GameFont.Small, alwaysDrawBg: false, alignCenter: false);
			if (pawn.Downed && !pawn.ageTracker.CurLifeStage.alwaysDowned)
			{
				GUI.color = new Color(1f, 0f, 0f, 0.5f);
				Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
				GUI.color = Color.white;
			}
			Widgets.EndGroup();
		}

		protected override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			if (selectedPawn != null && (selectedPawn.Destroyed || !base.SelCaravan.ContainsPawn(selectedPawn)))
			{
				selectedPawn = null;
			}
			Pawn captureSelectedPawn = selectedPawn;
			if (selectedPawn == null)
			{
				return;
			}
			Rect tabRect = base.TabRect;
			float width = 500f;
			Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, width, tabRect.height);
			Find.WindowStack.ImmediateWindow(323826479, rect, WindowLayer.GameUI, delegate
			{
				if (!captureSelectedPawn.DestroyedOrNull() && base.SelCaravan != null)
				{
					ITab_Pawn_Feeding.FillTab(captureSelectedPawn, rect.AtZero(), ref scrollPositionAutoBreastfeed, ref scrollPositionBabyConsumables, Pawns);
					if (Widgets.CloseButtonFor(rect.AtZero()))
					{
						captureSelectedPawn = null;
						SoundDefOf.TabClose.PlayOneShotOnCamera();
					}
				}
			});
		}

		public override void Notify_ClearingAllMapsMemory()
		{
			base.Notify_ClearingAllMapsMemory();
			selectedPawn = null;
		}

		public override void OnOpen()
		{
			base.OnOpen();
			if (selectedPawn == null || !Pawns.Contains(selectedPawn))
			{
				selectedPawn = Pawns.FirstOrFallback((Pawn pawn) => ChildcareUtility.CanSuckle(pawn, out var _));
			}
		}
	}
}
