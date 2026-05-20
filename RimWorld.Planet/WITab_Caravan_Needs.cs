using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

public class WITab_Caravan_Needs : WITab
{
	private Vector2 scrollPosition;

	private float scrollViewHeight;

	private Pawn specificNeedsTabForPawn;

	private Vector2 thoughtScrollPosition;

	private bool doNeeds;

	private float SpecificNeedsTabWidth
	{
		get
		{
			if (specificNeedsTabForPawn.DestroyedOrNull())
			{
				return 0f;
			}
			return NeedsCardUtility.GetSize(specificNeedsTabForPawn).x;
		}
	}

	public WITab_Caravan_Needs()
	{
		labelKey = "TabCaravanNeeds";
	}

	protected override void FillTab()
	{
		EnsureSpecificNeedsTabForPawnValid();
		CaravanNeedsTabUtility.DoRows(size, base.SelCaravan.PawnsListForReading, base.SelCaravan, ref scrollPosition, ref scrollViewHeight, ref specificNeedsTabForPawn, doNeeds);
	}

	protected override void UpdateSize()
	{
		EnsureSpecificNeedsTabForPawnValid();
		base.UpdateSize();
		size = CaravanNeedsTabUtility.GetSize(base.SelCaravan.PawnsListForReading, PaneTopY);
		if (size.x + SpecificNeedsTabWidth > (float)UI.screenWidth)
		{
			doNeeds = false;
			size = CaravanNeedsTabUtility.GetSize(base.SelCaravan.PawnsListForReading, PaneTopY, doNeeds: false);
		}
		else
		{
			doNeeds = true;
		}
		size.y = Mathf.Max(size.y, NeedsCardUtility.FullSize.y);
	}

	protected override void ExtraOnGUI()
	{
		EnsureSpecificNeedsTabForPawnValid();
		base.ExtraOnGUI();
		Pawn localSpecificNeedsTabForPawn = specificNeedsTabForPawn;
		if (localSpecificNeedsTabForPawn == null)
		{
			return;
		}
		Rect tabRect = base.TabRect;
		float specificNeedsTabWidth = SpecificNeedsTabWidth;
		Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificNeedsTabWidth, tabRect.height);
		Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
		{
			if (!localSpecificNeedsTabForPawn.DestroyedOrNull())
			{
				NeedsCardUtility.DoNeedsMoodAndThoughts(rect.AtZero(), localSpecificNeedsTabForPawn, ref thoughtScrollPosition);
				if (Widgets.CloseButtonFor(rect.AtZero()))
				{
					specificNeedsTabForPawn = null;
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
			}
		});
	}

	public override void Notify_ClearingAllMapsMemory()
	{
		base.Notify_ClearingAllMapsMemory();
		specificNeedsTabForPawn = null;
	}

	private void EnsureSpecificNeedsTabForPawnValid()
	{
		if (specificNeedsTabForPawn != null && (specificNeedsTabForPawn.Destroyed || !base.SelCaravan.ContainsPawn(specificNeedsTabForPawn)))
		{
			specificNeedsTabForPawn = null;
		}
	}
}
