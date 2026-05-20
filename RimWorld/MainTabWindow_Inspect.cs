using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class MainTabWindow_Inspect : MainTabWindow, IInspectPane
{
	private Type openTabType;

	private float recentHeight;

	private static IntVec3 lastSelectCell;

	private Thing lastSelectedThing;

	private Vector2 lastTabSize;

	private const float GuiltyTexSize = 26f;

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
			if (NumSelected == 1 && (Find.Selector.SelectedZone == null || Find.Selector.SelectedZone.ContainsCell(lastSelectCell)))
			{
				if (Find.Selector.SelectedPlan != null)
				{
					return Find.Selector.SelectedPlan.ContainsCell(lastSelectCell);
				}
				return true;
			}
			return false;
		}
	}

	public bool ShouldShowPaneContents
	{
		get
		{
			if (SelThing != null && SelThing.def.hideInspect && !DebugSettings.showHiddenInfo)
			{
				return false;
			}
			if (TryGetSelectedStorageGroup(out var _))
			{
				return true;
			}
			return NumSelected == 1;
		}
	}

	public IEnumerable<InspectTabBase> CurTabs
	{
		get
		{
			if (Find.ScreenshotModeHandler.Active)
			{
				return null;
			}
			if (NumSelected == 1)
			{
				if (SelThing != null && (SelThing.def.inspectorTabsResolved != null || SelThing is IStorageGroupMember { DrawStorageTab: not false }))
				{
					return SelThing.GetInspectTabs();
				}
				if (SelZone != null)
				{
					return SelZone.GetInspectTabs();
				}
			}
			else if (Selected.Count > 1 && Selected.All((object s) => s is IStorageGroupMember))
			{
				return (Selected.First() as Thing)?.GetInspectTabs();
			}
			return null;
		}
	}

	public MainTabWindow_Inspect()
	{
		closeOnAccept = false;
		closeOnCancel = false;
		drawInScreenshotMode = false;
		soundClose = SoundDefOf.TabClose;
	}

	public override void ExtraOnGUI()
	{
		base.ExtraOnGUI();
		InspectPaneUtility.ExtraOnGUI(this);
		Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator;
		if (selectedDesignator != null && (AnythingSelected || selectedDesignator.AlwaysDoGuiControls))
		{
			Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, PaneTopY);
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		InspectPaneUtility.InspectPaneOnGUI(inRect, this);
		if (lastSelectedThing != SelThing)
		{
			SetInitialSizeAndPosition();
			lastSelectedThing = SelThing;
		}
		else if (RequestedTabSize != lastTabSize)
		{
			SetInitialSizeAndPosition();
			lastTabSize = RequestedTabSize;
		}
	}

	public string GetLabel(Rect rect)
	{
		return InspectPaneUtility.AdjustedLabelFor(Selected, rect);
	}

	public void DoPaneContents(Rect rect)
	{
		if (TryGetSelectedStorageGroup(out var group))
		{
			InspectPaneFiller.DoPaneContentsForStorageGroup(group, rect);
		}
		else
		{
			InspectPaneFiller.DoPaneContentsFor((ISelectable)Find.Selector.FirstSelectedObject, rect);
		}
	}

	public void DoInspectPaneButtons(Rect rect, ref float lineEndWidth)
	{
		StorageGroup group;
		if (NumSelected == 1)
		{
			Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
			if (singleSelectedThing != null)
			{
				float num = rect.width - 48f;
				Widgets.InfoCardButton(num, 0f, Find.Selector.SingleSelectedThing);
				lineEndWidth += 24f;
				Pawn p = singleSelectedThing as Pawn;
				CompAnimalPenMarker comp;
				CompPilotConsole comp2;
				if (p != null)
				{
					if (p.playerSettings != null && p.playerSettings.UsesConfigurableHostilityResponse)
					{
						num -= 24f;
						HostilityResponseModeUtility.DrawResponseButton(new Rect(num, 0f, 24f, 24f), p, paintable: false);
						lineEndWidth += 24f;
					}
					if ((p.Faction == Faction.OfPlayer && p.RaceProps.Animal && p.RaceProps.hideTrainingTab) || (ModsConfig.BiotechActive && p.IsColonyMech))
					{
						num -= 30f;
						RenameUIUtility.DrawRenameButton(new Rect(num, 0f, 30f, 30f), p);
						lineEndWidth += 30f;
					}
					if (p.guilt != null && p.guilt.IsGuilty)
					{
						num -= 26f;
						Rect rect2 = new Rect(num, 0f, 26f, 26f);
						GUI.DrawTexture(rect2, TexUI.GuiltyTex);
						TooltipHandler.TipRegion(rect2, () => p.guilt.Tip, 6321223);
						lineEndWidth += 26f;
					}
				}
				else if (singleSelectedThing is IStorageGroupMember { ShowRenameButton: not false } storageGroupMember)
				{
					num -= 30f;
					Rect rect3 = new Rect(num, 0f, 30f, 30f);
					if (storageGroupMember.Group != null)
					{
						RenameUIUtility.DrawRenameButton(rect3, storageGroupMember.Group);
					}
					else if (singleSelectedThing.Spawned)
					{
						RenameUIUtility.DrawRenameButton(rect3, storageGroupMember);
					}
					lineEndWidth += 30f;
				}
				else if (singleSelectedThing.Spawned && singleSelectedThing.Faction == Faction.OfPlayer && singleSelectedThing.TryGetComp(out comp))
				{
					num -= 30f;
					RenameUIUtility.DrawRenameButton(new Rect(num, 0f, 30f, 30f), comp);
					lineEndWidth += 30f;
				}
				else if (singleSelectedThing.Spawned && singleSelectedThing.Faction == Faction.OfPlayer && singleSelectedThing.TryGetComp(out comp2) && comp2.engine != null)
				{
					num -= 30f;
					RenameUIUtility.DrawRenameButton(new Rect(num, 0f, 30f, 30f), comp2.engine);
					lineEndWidth += 30f;
				}
				else if (singleSelectedThing is IRenameable renamable)
				{
					num -= 30f;
					RenameUIUtility.DrawRenameButton(new Rect(num, 0f, 30f, 30f), renamable);
					lineEndWidth += 30f;
				}
			}
			else if (Find.Selector.SelectedZone != null)
			{
				Rect rect4 = new Rect(rect.width - 30f, 0f, 30f, 30f);
				if (ShouldShowSelectNextInCellButton)
				{
					rect4.x -= 24f;
				}
				RenameUIUtility.DrawRenameButton(rect4, Find.Selector.SelectedZone);
				lineEndWidth += 30f;
			}
			else if (Find.Selector.SelectedPlan != null)
			{
				Rect rect5 = new Rect(rect.width - 30f, 0f, 30f, 30f);
				if (ShouldShowSelectNextInCellButton)
				{
					rect5.x -= 24f;
				}
				RenameUIUtility.DrawRenameButton(rect5, Find.Selector.SelectedPlan);
				lineEndWidth += 30f;
			}
		}
		else if (TryGetSelectedStorageGroup(out group))
		{
			float x = rect.width - 30f;
			RenameUIUtility.DrawRenameButton(new Rect(x, 0f, 30f, 30f), group);
			lineEndWidth += 30f;
		}
	}

	private bool TryGetSelectedStorageGroup(out StorageGroup group)
	{
		bool flag = true;
		group = null;
		if (Find.Selector.SelectedObjects.Count <= 1)
		{
			group = null;
			return false;
		}
		foreach (object selectedObject in Find.Selector.SelectedObjects)
		{
			if (selectedObject is IStorageGroupMember storageGroupMember)
			{
				if (group == null)
				{
					group = storageGroupMember.Group;
				}
				if (storageGroupMember.Group != group || storageGroupMember.Group == null)
				{
					flag = false;
					break;
				}
				continue;
			}
			flag = false;
			break;
		}
		if (flag)
		{
			return group != null;
		}
		return false;
	}

	public void SelectNextInCell()
	{
		if (NumSelected != 1)
		{
			return;
		}
		Selector selector = Find.Selector;
		if ((selector.SelectedZone != null && !selector.SelectedZone.ContainsCell(lastSelectCell)) || (selector.SelectedPlan != null && !selector.SelectedPlan.ContainsCell(lastSelectCell)))
		{
			return;
		}
		if (selector.SelectedZone == null && selector.SelectedPlan == null)
		{
			lastSelectCell = selector.SingleSelectedThing.PositionHeld;
		}
		Map map;
		if (selector.SingleSelectedThing != null)
		{
			map = selector.SingleSelectedThing.MapHeld;
		}
		else if (selector.SelectedZone != null)
		{
			map = selector.SelectedZone.Map;
		}
		else
		{
			if (selector.SelectedPlan == null)
			{
				Log.Error("Attempted to selected next thing in cell but case was not handled.");
				return;
			}
			map = selector.SelectedPlan.Map;
		}
		selector.SelectNextAt(lastSelectCell, map);
	}

	public override void WindowUpdate()
	{
		base.WindowUpdate();
		InspectPaneUtility.UpdateTabs(this);
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
