using System.Collections.Generic;

namespace Verse
{
	public static class EditTreeNodeDatabase
	{
		private static List<TreeNode_Editor> roots = new List<TreeNode_Editor>();

		public static TreeNode_Editor RootOf(object obj)
		{
			for (int i = 0; i < roots.Count; i++)
			{
				if (roots[i].obj == obj)
				{
					return roots[i];
				}
			}
			TreeNode_Editor treeNode_Editor = TreeNode_Editor.NewRootNode(obj);
			roots.Add(treeNode_Editor);
			return treeNode_Editor;
		}
	}
}
