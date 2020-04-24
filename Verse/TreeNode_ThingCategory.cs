using System.Collections.Generic;

namespace Verse
{
	public class TreeNode_ThingCategory : TreeNode
	{
		public ThingCategoryDef catDef;

		public string Label => catDef.label;

		public string LabelCap => Label.CapitalizeFirst();

		public IEnumerable<TreeNode_ThingCategory> ChildCategoryNodesAndThis
		{
			get
			{
				foreach (ThingCategoryDef thisAndChildCategoryDef in catDef.ThisAndChildCategoryDefs)
				{
					yield return thisAndChildCategoryDef.treeNode;
				}
			}
		}

		public IEnumerable<TreeNode_ThingCategory> ChildCategoryNodes
		{
			get
			{
				foreach (ThingCategoryDef childCategory in catDef.childCategories)
				{
					yield return childCategory.treeNode;
				}
			}
		}

		public TreeNode_ThingCategory(ThingCategoryDef def)
		{
			catDef = def;
		}

		public override string ToString()
		{
			return catDef.defName;
		}
	}
}
