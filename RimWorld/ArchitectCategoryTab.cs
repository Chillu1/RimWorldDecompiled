using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ArchitectCategoryTab
	{
		public DesignationCategoryDef def;

		public const float InfoRectHeight = 270f;

		public static Rect InfoRect => new Rect(0f, (float)(UI.screenHeight - 35) - ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).WinHeight - 270f, 200f, 270f);

		public ArchitectCategoryTab(DesignationCategoryDef def)
		{
			this.def = def;
		}

		public void DesignationTabOnGUI()
		{
			if (Find.DesignatorManager.SelectedDesignator != null)
			{
				Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, (float)(UI.screenHeight - 35) - ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).WinHeight - 270f);
			}
			float startX = 210f;
			GizmoGridDrawer.DrawGizmoGrid(def.ResolvedAllowedDesignators.Cast<Gizmo>(), startX, out Gizmo mouseoverGizmo);
			if (mouseoverGizmo == null && Find.DesignatorManager.SelectedDesignator != null)
			{
				mouseoverGizmo = Find.DesignatorManager.SelectedDesignator;
			}
			DoInfoBox(InfoRect, (Designator)mouseoverGizmo);
		}

		protected void DoInfoBox(Rect infoRect, Designator designator)
		{
			Find.WindowStack.ImmediateWindow(32520, infoRect, WindowLayer.GameUI, delegate
			{
				if (designator != null)
				{
					Rect position = infoRect.AtZero().ContractedBy(7f);
					GUI.BeginGroup(position);
					Rect rect = new Rect(0f, 0f, position.width - designator.PanelReadoutTitleExtraRightMargin, 999f);
					Text.Font = GameFont.Small;
					Widgets.Label(rect, designator.LabelCap);
					float curY = Mathf.Max(24f, Text.CalcHeight(designator.LabelCap, rect.width));
					designator.DrawPanelReadout(ref curY, position.width);
					Rect rect2 = new Rect(0f, curY, position.width, position.height - curY);
					string desc = designator.Desc;
					GenText.SetTextSizeToFit(desc, rect2);
					Widgets.Label(rect2, desc);
					GUI.EndGroup();
				}
			});
		}
	}
}
