using System;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ArchitectCategoryTab
{
	public readonly DesignationCategoryDef def;

	private readonly QuickSearchFilter quickSearchFilter;

	private bool anySearchMatches;

	private Designator uniqueSearchMatch;

	private readonly Func<Gizmo, bool> shouldLowLightGizmoFunc;

	private readonly Func<Gizmo, bool> shouldHighLightGizmoFunc;

	public const float InfoRectHeight = 270f;

	public bool AnySearchMatches => anySearchMatches;

	public Designator UniqueSearchMatch => uniqueSearchMatch;

	public bool Visible => def.Visible;

	public int? PreferredColumn => def.preferredColumn;

	public static Rect InfoRect => new Rect(0f, (float)(UI.screenHeight - 35) - ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).WinHeight - 270f, 200f, 270f);

	public ArchitectCategoryTab(DesignationCategoryDef def, QuickSearchFilter quickSearchFilter)
	{
		this.def = def;
		this.quickSearchFilter = quickSearchFilter;
		shouldLowLightGizmoFunc = ShouldLowLightGizmo;
		shouldHighLightGizmoFunc = ShouldHighLightGizmo;
	}

	public void DesignationTabOnGUI(Designator forceActivatedCommand)
	{
		if (Find.DesignatorManager.SelectedDesignator != null)
		{
			Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, (float)(UI.screenHeight - 35) - ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).WinHeight - 270f);
		}
		Func<Gizmo, bool> customActivatorFunc = ((forceActivatedCommand == null) ? null : ((Func<Gizmo, bool>)((Gizmo cmd) => cmd == forceActivatedCommand)));
		float startX = 210f;
		GizmoGridDrawer.DrawGizmoGrid(def.ResolvedAllowedDesignators, startX, out var mouseoverGizmo, customActivatorFunc, shouldHighLightGizmoFunc, shouldLowLightGizmoFunc);
		if (mouseoverGizmo == null && Find.DesignatorManager.SelectedDesignator != null)
		{
			mouseoverGizmo = Find.DesignatorManager.SelectedDesignator;
		}
		DoInfoBox(InfoRect, (Designator)mouseoverGizmo);
	}

	private bool ShouldLowLightGizmo(Gizmo gizmo)
	{
		if (!(gizmo is Command c))
		{
			return false;
		}
		if (quickSearchFilter.Active && !Matches(c))
		{
			return true;
		}
		return false;
	}

	private bool ShouldHighLightGizmo(Gizmo gizmo)
	{
		if (!(gizmo is Command c))
		{
			return false;
		}
		if (quickSearchFilter.Active && Matches(c))
		{
			return true;
		}
		return false;
	}

	private bool Matches(Command c)
	{
		return quickSearchFilter.Matches(c.Label);
	}

	protected void DoInfoBox(Rect infoRect, Designator designator)
	{
		Find.WindowStack.ImmediateWindow(32520, infoRect, WindowLayer.GameUI, delegate
		{
			if (designator != null)
			{
				Rect rect = infoRect.AtZero().ContractedBy(7f);
				Widgets.BeginGroup(rect);
				Rect rect2 = new Rect(0f, 0f, rect.width - designator.PanelReadoutTitleExtraRightMargin, 999f);
				Text.Font = GameFont.Small;
				Widgets.Label(rect2, designator.LabelCap);
				float curY = Mathf.Max(24f, Text.CalcHeight(designator.LabelCap, rect2.width));
				designator.DrawPanelReadout(ref curY, rect.width);
				Rect rect3 = new Rect(0f, curY, rect.width, rect.height - curY);
				string desc = designator.Desc;
				GenText.SetTextSizeToFit(desc, rect3);
				desc = desc.TruncateHeight(rect3.width, rect3.height);
				Widgets.Label(rect3, desc);
				Widgets.EndGroup();
			}
		});
	}

	public void CacheSearchState()
	{
		anySearchMatches = true;
		uniqueSearchMatch = null;
		if (!quickSearchFilter.Active)
		{
			return;
		}
		int num = 0;
		Designator designator = null;
		foreach (Designator resolvedAllowedDesignator in def.ResolvedAllowedDesignators)
		{
			if (Matches(resolvedAllowedDesignator))
			{
				num++;
				designator = resolvedAllowedDesignator;
				if (num > 1)
				{
					return;
				}
			}
		}
		switch (num)
		{
		case 0:
			anySearchMatches = false;
			break;
		case 1:
			uniqueSearchMatch = designator;
			break;
		}
	}
}
