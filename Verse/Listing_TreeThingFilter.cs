using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Listing_TreeThingFilter : Listing_Tree
{
	private static readonly Color NoMatchColor = Color.grey;

	private static readonly LRUCache<(TreeNode_ThingCategory, ThingFilter), List<SpecialThingFilterDef>> cachedHiddenSpecialFilters = new LRUCache<(TreeNode_ThingCategory, ThingFilter), List<SpecialThingFilterDef>>(500);

	private ThingFilter filter;

	private ThingFilter parentFilter;

	private const float IconSize = 20f;

	private const float IconOffset = 6f;

	private List<SpecialThingFilterDef> hiddenSpecialFilters;

	private List<ThingDef> forceHiddenDefs;

	private List<SpecialThingFilterDef> tempForceHiddenSpecialFilters;

	private List<ThingDef> suppressSmallVolumeTags;

	protected QuickSearchFilter searchFilter;

	public int matchCount;

	private Rect visibleRect;

	public Listing_TreeThingFilter(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, IEnumerable<SpecialThingFilterDef> forceHiddenFilters, List<ThingDef> suppressSmallVolumeTags, QuickSearchFilter searchFilter)
	{
		this.filter = filter;
		this.parentFilter = parentFilter;
		if (forceHiddenDefs != null)
		{
			this.forceHiddenDefs = forceHiddenDefs.ToList();
		}
		if (forceHiddenFilters != null)
		{
			tempForceHiddenSpecialFilters = forceHiddenFilters.ToList();
		}
		this.suppressSmallVolumeTags = suppressSmallVolumeTags;
		this.searchFilter = searchFilter;
	}

	public void ListCategoryChildren(TreeNode_ThingCategory node, int openMask, Map map, Rect visibleRect)
	{
		this.visibleRect = visibleRect;
		int num = 0;
		foreach (SpecialThingFilterDef parentsSpecialThingFilterDef in node.catDef.ParentsSpecialThingFilterDefs)
		{
			if (Visible(parentsSpecialThingFilterDef, node))
			{
				DoSpecialFilter(parentsSpecialThingFilterDef, num);
			}
		}
		DoCategoryChildren(node, num, openMask, map, subtreeMatchedSearch: false);
	}

	private void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool subtreeMatchedSearch)
	{
		List<SpecialThingFilterDef> childSpecialFilters = node.catDef.childSpecialFilters;
		for (int i = 0; i < childSpecialFilters.Count; i++)
		{
			if (Visible(childSpecialFilters[i], node))
			{
				DoSpecialFilter(childSpecialFilters[i], indentLevel);
			}
		}
		foreach (TreeNode_ThingCategory childCategoryNode in node.ChildCategoryNodes)
		{
			if (Visible(childCategoryNode) && !HideCategoryDueToSearch(childCategoryNode))
			{
				DoCategory(childCategoryNode, indentLevel, openMask, map, subtreeMatchedSearch);
			}
		}
		List<ThingDef> list = new List<ThingDef>();
		foreach (ThingDef sortedChildThingDef in node.catDef.SortedChildThingDefs)
		{
			if (Find.HiddenItemsManager.Hidden(sortedChildThingDef))
			{
				list.Add(sortedChildThingDef);
			}
			else if (Visible(sortedChildThingDef) && !HideThingDueToSearch(sortedChildThingDef))
			{
				DoThingDef(sortedChildThingDef, indentLevel, map);
			}
		}
		if (!searchFilter.Active && list.Count > 0)
		{
			DoUndiscoveredEntry(indentLevel, node.catDef.parent != ThingCategoryDefOf.Corpses, list);
		}
		bool HideCategoryDueToSearch(TreeNode_ThingCategory subCat)
		{
			if (!searchFilter.Active || subtreeMatchedSearch)
			{
				return false;
			}
			if (CategoryMatches(subCat))
			{
				return false;
			}
			if (ThisOrDescendantsVisibleAndMatchesSearch(subCat))
			{
				return false;
			}
			return true;
		}
		bool HideThingDueToSearch(ThingDef tDef)
		{
			if (!searchFilter.Active || subtreeMatchedSearch)
			{
				return false;
			}
			return !searchFilter.Matches(tDef);
		}
	}

	private void DoSpecialFilter(SpecialThingFilterDef sfDef, int nestLevel)
	{
		if (!sfDef.configurable)
		{
			return;
		}
		Color? textColor = null;
		if (searchFilter.Matches(sfDef))
		{
			matchCount++;
		}
		else
		{
			textColor = NoMatchColor;
		}
		if (CurrentRowVisibleOnScreen())
		{
			LabelLeft("*" + sfDef.LabelCap, sfDef.description, nestLevel, 0f, textColor);
			bool checkOn = filter.Allows(sfDef);
			bool flag = checkOn;
			Widgets.Checkbox(new Vector2(LabelWidth, curY), ref checkOn, lineHeight, disabled: false, paintable: true);
			if (checkOn != flag)
			{
				filter.SetAllow(sfDef, checkOn);
			}
		}
		EndLine();
	}

	private void DoCategory(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool subtreeMatchedSearch)
	{
		Color? textColor = null;
		if (searchFilter.Active)
		{
			if (CategoryMatches(node))
			{
				subtreeMatchedSearch = true;
				matchCount++;
			}
			else
			{
				textColor = NoMatchColor;
			}
		}
		if (CurrentRowVisibleOnScreen())
		{
			OpenCloseWidget(node, indentLevel, openMask);
			LabelLeft(node.LabelCap, node.catDef.description, indentLevel, 0f, textColor);
			MultiCheckboxState multiCheckboxState = AllowanceStateOf(node);
			MultiCheckboxState multiCheckboxState2 = Widgets.CheckboxMulti(new Rect(LabelWidth, curY, lineHeight, lineHeight), multiCheckboxState, paintable: true);
			if (multiCheckboxState != multiCheckboxState2)
			{
				filter.SetAllow(node.catDef, multiCheckboxState2 == MultiCheckboxState.On, forceHiddenDefs, hiddenSpecialFilters);
			}
		}
		EndLine();
		if (IsOpen(node, openMask))
		{
			DoCategoryChildren(node, indentLevel + 1, openMask, map, subtreeMatchedSearch);
		}
	}

	private void DoThingDef(ThingDef tDef, int nestLevel, Map map)
	{
		Color? color = null;
		if (searchFilter.Matches(tDef))
		{
			matchCount++;
		}
		else
		{
			color = NoMatchColor;
		}
		if (tDef.uiIcon != null && tDef.uiIcon != BaseContent.BadTex)
		{
			nestLevel++;
			Widgets.DefIcon(new Rect(XAtIndentLevel(nestLevel) - 6f, curY, 20f, 20f), tDef, null, 1f, null, drawPlaceholder: true, color);
		}
		if (CurrentRowVisibleOnScreen())
		{
			bool num = (suppressSmallVolumeTags == null || !suppressSmallVolumeTags.Contains(tDef)) && tDef.IsStuff && tDef.smallVolume;
			string text = tDef.DescriptionDetailed;
			if (num)
			{
				text += "\n\n" + "ThisIsSmallVolume".Translate(10.ToStringCached());
			}
			float num2 = -4f;
			if (num)
			{
				Rect rect = new Rect(LabelWidth - 19f, curY, 19f, 20f);
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperRight;
				GUI.color = Color.gray;
				Widgets.Label(rect, "/" + 10.ToStringCached());
				Text.Font = GameFont.Small;
				GenUI.ResetLabelAlign();
				GUI.color = Color.white;
			}
			num2 -= 19f;
			if (map != null)
			{
				int count = map.resourceCounter.GetCount(tDef);
				if (count > 0)
				{
					string text2 = count.ToStringCached();
					Rect rect2 = new Rect(0f, curY, LabelWidth + num2, 40f);
					Text.Font = GameFont.Small;
					Text.Anchor = TextAnchor.UpperRight;
					GUI.color = new Color(0.5f, 0.5f, 0.1f);
					Widgets.Label(rect2, text2);
					num2 -= Text.CalcSize(text2).x;
					GenUI.ResetLabelAlign();
					Text.Font = GameFont.Small;
					GUI.color = Color.white;
				}
			}
			LabelLeft(tDef.LabelCap, text, nestLevel, num2, color);
			bool checkOn = filter.Allows(tDef);
			bool flag = checkOn;
			Widgets.Checkbox(new Vector2(LabelWidth, curY), ref checkOn, lineHeight, disabled: false, paintable: true);
			if (checkOn != flag)
			{
				filter.SetAllow(tDef, checkOn);
			}
		}
		EndLine();
	}

	private void DoUndiscoveredEntry(int nestLevel, bool useIconOffset, List<ThingDef> toggledThingDefs)
	{
		if (CurrentRowVisibleOnScreen())
		{
			TaggedString taggedString = "UndiscoveredItemLabel".Translate();
			string tipText = "UndiscoveredItemDesc".Translate().Resolve();
			LabelLeft(taggedString, tipText, nestLevel, 0f, null, useIconOffset ? 10f : 0f);
			bool checkOn = filter.Allows(toggledThingDefs[0]);
			bool flag = checkOn;
			Widgets.Checkbox(new Vector2(LabelWidth, curY), ref checkOn, lineHeight, disabled: false, paintable: true);
			if (checkOn != flag)
			{
				foreach (ThingDef toggledThingDef in toggledThingDefs)
				{
					filter.SetAllow(toggledThingDef, checkOn);
				}
			}
		}
		EndLine();
	}

	public MultiCheckboxState AllowanceStateOf(TreeNode_ThingCategory cat)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (ThingDef descendantThingDef in cat.catDef.DescendantThingDefs)
		{
			if (Visible(descendantThingDef))
			{
				num++;
				if (filter.Allows(descendantThingDef))
				{
					num2++;
				}
			}
		}
		foreach (SpecialThingFilterDef descendantSpecialThingFilterDef in cat.catDef.DescendantSpecialThingFilterDefs)
		{
			if (Visible(descendantSpecialThingFilterDef, cat))
			{
				num3++;
				if (filter.Allows(descendantSpecialThingFilterDef))
				{
					num4++;
				}
			}
		}
		if (filter.OnlySpecialFilters)
		{
			if (num4 == 0)
			{
				return MultiCheckboxState.Off;
			}
			if (num4 < num3)
			{
				return MultiCheckboxState.Partial;
			}
			return MultiCheckboxState.On;
		}
		if (num2 == 0)
		{
			return MultiCheckboxState.Off;
		}
		if (num == num2 && num3 == num4)
		{
			return MultiCheckboxState.On;
		}
		return MultiCheckboxState.Partial;
	}

	private bool Visible(ThingDef td)
	{
		if (!td.PlayerAcquirable)
		{
			return false;
		}
		if (td.virtualDefParent != null)
		{
			return false;
		}
		if (forceHiddenDefs != null && forceHiddenDefs.Contains(td))
		{
			return false;
		}
		if (parentFilter != null)
		{
			if (!parentFilter.Allows(td))
			{
				return false;
			}
			if (parentFilter.IsAlwaysDisallowedDueToSpecialFilters(td))
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsOpen(TreeNode node, int openMask)
	{
		if (base.IsOpen(node, openMask))
		{
			return true;
		}
		if (node is TreeNode_ThingCategory node2 && searchFilter.Active && ThisOrDescendantsVisibleAndMatchesSearch(node2))
		{
			return true;
		}
		return false;
	}

	private bool ThisOrDescendantsVisibleAndMatchesSearch(TreeNode_ThingCategory node)
	{
		if (Visible(node) && CategoryMatches(node))
		{
			return true;
		}
		foreach (ThingDef childThingDef in node.catDef.childThingDefs)
		{
			if (ThingDefVisibleAndMatches(childThingDef))
			{
				return true;
			}
		}
		foreach (SpecialThingFilterDef childSpecialFilter in node.catDef.childSpecialFilters)
		{
			if (SpecialFilterVisibleAndMatches(childSpecialFilter, node))
			{
				return true;
			}
		}
		foreach (ThingCategoryDef childCategory in node.catDef.childCategories)
		{
			if (ThisOrDescendantsVisibleAndMatchesSearch(childCategory.treeNode))
			{
				return true;
			}
		}
		return false;
		bool SpecialFilterVisibleAndMatches(SpecialThingFilterDef sf, TreeNode_ThingCategory subCat)
		{
			if (Visible(sf, subCat))
			{
				return searchFilter.Matches(sf);
			}
			return false;
		}
		bool ThingDefVisibleAndMatches(ThingDef td)
		{
			if (Visible(td))
			{
				return searchFilter.Matches(td);
			}
			return false;
		}
	}

	private bool CategoryMatches(TreeNode_ThingCategory node)
	{
		return searchFilter.Matches(node.catDef.label);
	}

	private bool Visible(TreeNode_ThingCategory node)
	{
		if (filter.OnlySpecialFilters)
		{
			return node.catDef.DescendantSpecialThingFilterDefs.Any(Visible);
		}
		return node.catDef.DescendantThingDefs.Any(Visible);
	}

	private bool Visible(SpecialThingFilterDef f)
	{
		if (parentFilter != null && !parentFilter.Allows(f))
		{
			return false;
		}
		return true;
	}

	private bool Visible(SpecialThingFilterDef filterDef, TreeNode_ThingCategory node)
	{
		if (parentFilter != null && !parentFilter.Allows(filterDef))
		{
			return false;
		}
		if (filter.OnlySpecialFilters)
		{
			return true;
		}
		if (hiddenSpecialFilters == null)
		{
			CalculateHiddenSpecialFilters(node);
		}
		for (int i = 0; i < hiddenSpecialFilters.Count; i++)
		{
			if (hiddenSpecialFilters[i] == filterDef)
			{
				return false;
			}
		}
		return true;
	}

	private bool CurrentRowVisibleOnScreen()
	{
		Rect other = new Rect(0f, curY, base.ColumnWidth, lineHeight);
		return visibleRect.Overlaps(other);
	}

	private void CalculateHiddenSpecialFilters(TreeNode_ThingCategory node)
	{
		hiddenSpecialFilters = GetCachedHiddenSpecialFilters(node, parentFilter);
		if (tempForceHiddenSpecialFilters != null)
		{
			hiddenSpecialFilters = new List<SpecialThingFilterDef>(hiddenSpecialFilters);
			hiddenSpecialFilters.AddRange(tempForceHiddenSpecialFilters);
		}
	}

	private static List<SpecialThingFilterDef> GetCachedHiddenSpecialFilters(TreeNode_ThingCategory node, ThingFilter parentFilter)
	{
		(TreeNode_ThingCategory, ThingFilter) key = (node, parentFilter);
		if (cachedHiddenSpecialFilters.TryGetValue(key, out var result))
		{
			return result;
		}
		result = CalculateHiddenSpecialFilters(node, parentFilter);
		cachedHiddenSpecialFilters.Add(key, result);
		return result;
	}

	private static List<SpecialThingFilterDef> CalculateHiddenSpecialFilters(TreeNode_ThingCategory node, ThingFilter parentFilter)
	{
		List<SpecialThingFilterDef> list = new List<SpecialThingFilterDef>();
		IEnumerable<SpecialThingFilterDef> enumerable = node.catDef.ParentsSpecialThingFilterDefs.Concat(node.catDef.DescendantSpecialThingFilterDefs);
		IEnumerable<ThingDef> enumerable2 = node.catDef.DescendantThingDefs;
		if (parentFilter != null)
		{
			enumerable2 = enumerable2.Where(parentFilter.Allows);
		}
		foreach (SpecialThingFilterDef item in enumerable)
		{
			bool flag = false;
			foreach (ThingDef item2 in enumerable2)
			{
				if (item.Worker.CanEverMatch(item2))
				{
					flag = true;
					break;
				}
			}
			if (parentFilter != null && parentFilter.hiddenSpecialFilters.Contains(item))
			{
				flag = false;
			}
			if (!flag)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void ResetStaticData()
	{
		cachedHiddenSpecialFilters.Clear();
	}
}
