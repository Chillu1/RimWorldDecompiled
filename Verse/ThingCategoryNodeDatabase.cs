using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public static class ThingCategoryNodeDatabase
{
	public static bool initialized;

	private static TreeNode_ThingCategory rootNode;

	public static List<TreeNode_ThingCategory> allThingCategoryNodes;

	public static TreeNode_ThingCategory RootNode => rootNode;

	public static void Clear()
	{
		rootNode = null;
		initialized = false;
	}

	public static void FinalizeInit()
	{
		rootNode = ThingCategoryDefOf.Root.treeNode;
		foreach (ThingCategoryDef allDef in DefDatabase<ThingCategoryDef>.AllDefs)
		{
			if (allDef.parent != null)
			{
				allDef.parent.childCategories.Add(allDef);
			}
		}
		SetNestLevelRecursive(rootNode, 0);
		foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef2.thingCategories == null)
			{
				continue;
			}
			foreach (ThingCategoryDef thingCategory in allDef2.thingCategories)
			{
				thingCategory.childThingDefs.Add(allDef2);
			}
		}
		foreach (SpecialThingFilterDef allDef3 in DefDatabase<SpecialThingFilterDef>.AllDefs)
		{
			allDef3.parentCategory.childSpecialFilters.Add(allDef3);
		}
		if (rootNode.catDef.childCategories.Any())
		{
			rootNode.catDef.childCategories[0].treeNode.SetOpen(-1, val: true);
		}
		allThingCategoryNodes = rootNode.ChildCategoryNodesAndThis.ToList();
		initialized = true;
	}

	private static void SetNestLevelRecursive(TreeNode_ThingCategory node, int nestDepth)
	{
		foreach (ThingCategoryDef childCategory in node.catDef.childCategories)
		{
			childCategory.treeNode.nestDepth = nestDepth;
			SetNestLevelRecursive(childCategory.treeNode, nestDepth + 1);
		}
	}
}
