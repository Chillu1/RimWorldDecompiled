using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Listing_ResourceReadout : Listing_Tree
{
	private Map map;

	private static Texture2D SolidCategoryBG = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.6f));

	protected override float LabelWidth => base.ColumnWidth;

	public Listing_ResourceReadout(Map map)
	{
		this.map = map;
	}

	public void DoCategory(TreeNode_ThingCategory node, int nestLevel, int openMask)
	{
		int countIn = map.resourceCounter.GetCountIn(node.catDef);
		if (countIn != 0)
		{
			OpenCloseWidget(node, nestLevel, openMask);
			Rect rect = new Rect(0f, curY, LabelWidth, lineHeight);
			rect.xMin = XAtIndentLevel(nestLevel) + 18f;
			Rect position = rect;
			position.width = 80f;
			position.yMax -= 3f;
			position.yMin += 3f;
			GUI.DrawTexture(position, SolidCategoryBG);
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, new TipSignal(node.catDef.LabelCap, node.catDef.GetHashCode()));
			}
			Rect position2 = new Rect(rect);
			float width = (position2.height = 28f);
			position2.width = width;
			position2.y = rect.y + rect.height / 2f - position2.height / 2f;
			GUI.DrawTexture(position2, node.catDef.icon);
			Rect rect2 = new Rect(rect);
			rect2.xMin = position2.xMax + 6f;
			Widgets.Label(rect2, countIn.ToStringCached());
			EndLine();
			if (IsOpen(node, openMask))
			{
				DoCategoryChildren(node, nestLevel + 1, openMask);
			}
		}
	}

	public void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask)
	{
		foreach (TreeNode_ThingCategory childCategoryNode in node.ChildCategoryNodes)
		{
			if (!childCategoryNode.catDef.resourceReadoutRoot)
			{
				DoCategory(childCategoryNode, indentLevel, openMask);
			}
		}
		foreach (ThingDef childThingDef in node.catDef.childThingDefs)
		{
			if (childThingDef.PlayerAcquirable)
			{
				DoThingDef(childThingDef, indentLevel + 1);
			}
		}
	}

	private void DoThingDef(ThingDef thingDef, int nestLevel)
	{
		int count = map.resourceCounter.GetCount(thingDef);
		if (count == 0)
		{
			return;
		}
		Rect rect = new Rect(0f, curY, LabelWidth, lineHeight);
		rect.xMin = XAtIndentLevel(nestLevel) + 18f;
		if (Mouse.IsOver(rect))
		{
			GUI.DrawTexture(rect, TexUI.HighlightTex);
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, new TipSignal(() => thingDef.LabelCap + ": " + thingDef.description.CapitalizeFirst(), thingDef.shortHash));
		}
		Rect rect2 = new Rect(rect);
		float width = (rect2.height = 28f);
		rect2.width = width;
		rect2.y = rect.y + rect.height / 2f - rect2.height / 2f;
		Widgets.ThingIcon(rect2, thingDef);
		Rect rect3 = new Rect(rect);
		rect3.xMin = rect2.xMax + 6f;
		Widgets.Label(rect3, count.ToStringCached());
		EndLine();
	}
}
