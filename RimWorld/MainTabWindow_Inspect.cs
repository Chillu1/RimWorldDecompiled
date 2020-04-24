using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Inspect : MainTabWindow, IInspectPane
	{
		private Type openTabType;

		private float recentHeight;

		private static IntVec3 lastSelectCell;

		private Gizmo mouseoverGizmo;

		public Type OpenTabType
		{
			get
			{
				return openTabType;
			}
			set
			{
				openTabType = value;
			}
		}

		public float RecentHeight
		{
			get
			{
				return recentHeight;
			}
			set
			{
				recentHeight = value;
			}
		}

		protected override float Margin => 0f;

		public override Vector2 RequestedTabSize => InspectPaneUtility.PaneSizeFor(this);

		private List<object> Selected => Find.Selector.SelectedObjects;

		private Thing SelThing => Find.Selector.SingleSelectedThing;

		private Zone SelZone => Find.Selector.SelectedZone;

		private int NumSelected => Find.Selector.NumSelected;

		public float PaneTopY => (float)UI.screenHeight - 165f - 35f;

		public bool AnythingSelected => NumSelected > 0;

		public bool ShouldShowSelectNextInCellButton
		{
			get
			{
				if (NumSelected == 1)
				{
					if (Find.Selector.SelectedZone != null)
					{
						return Find.Selector.SelectedZone.ContainsCell(lastSelectCell);
					}
					return true;
				}
				return false;
			}
		}

		public bool ShouldShowPaneContents => NumSelected == 1;

		public IEnumerable<InspectTabBase> CurTabs
		{
			get
			{
				if (NumSelected == 1)
				{
					if (SelThing != null && SelThing.def.inspectorTabsResolved != null)
					{
						return SelThing.GetInspectTabs();
					}
					if (SelZone != null)
					{
						return SelZone.GetInspectTabs();
					}
				}
				return null;
			}
		}

		public MainTabWindow_Inspect()
		{
			closeOnAccept = false;
			closeOnCancel = false;
			soundClose = SoundDefOf.TabClose;
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			InspectPaneUtility.ExtraOnGUI(this);
			if (AnythingSelected && Find.DesignatorManager.SelectedDesignator != null)
			{
				Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, PaneTopY);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			InspectPaneUtility.InspectPaneOnGUI(inRect, this);
		}

		public string GetLabel(Rect rect)
		{
			return InspectPaneUtility.AdjustedLabelFor(Selected, rect);
		}

		public void DrawInspectGizmos()
		{
			InspectGizmoGrid.DrawInspectGizmoGridFor(Selected, out mouseoverGizmo);
		}

		public void DoPaneContents(Rect rect)
		{
			InspectPaneFiller.DoPaneContentsFor((ISelectable)Find.Selector.FirstSelectedObject, rect);
		}

		public void DoInspectPaneButtons(Rect rect, ref float lineEndWidth)
		{
			if (NumSelected != 1)
			{
				return;
			}
			Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
			if (singleSelectedThing != null)
			{
				Widgets.InfoCardButton(rect.width - 48f, 0f, Find.Selector.SingleSelectedThing);
				lineEndWidth += 24f;
				Pawn pawn = singleSelectedThing as Pawn;
				if (pawn != null && pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse)
				{
					HostilityResponseModeUtility.DrawResponseButton(new Rect(rect.width - 72f, 0f, 24f, 24f), pawn, paintable: false);
					lineEndWidth += 24f;
				}
			}
		}

		public void SelectNextInCell()
		{
			if (NumSelected != 1)
			{
				return;
			}
			Selector selector = Find.Selector;
			if (selector.SelectedZone == null || selector.SelectedZone.ContainsCell(lastSelectCell))
			{
				if (selector.SelectedZone == null)
				{
					lastSelectCell = selector.SingleSelectedThing.Position;
				}
				selector.SelectNextAt(map: (selector.SingleSelectedThing == null) ? selector.SelectedZone.Map : selector.SingleSelectedThing.Map, c: lastSelectCell);
			}
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			InspectPaneUtility.UpdateTabs(this);
			if (mouseoverGizmo != null)
			{
				mouseoverGizmo.GizmoUpdateOnMouseover();
			}
		}

		public void CloseOpenTab()
		{
			openTabType = null;
		}

		public void Reset()
		{
			openTabType = null;
		}
	}
}
