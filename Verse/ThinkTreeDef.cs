using System.Collections.Generic;
using Verse.AI;

namespace Verse;

public class ThinkTreeDef : Def
{
	public ThinkNode thinkRoot;

	[NoTranslate]
	public string insertTag;

	public float insertPriority;

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		thinkRoot.ResolveSubnodesAndRecur();
		foreach (ThinkNode item in thinkRoot.ThisAndChildrenRecursive)
		{
			item.ResolveReferences();
		}
		ThinkTreeKeyAssigner.AssignKeys(thinkRoot, GenText.StableStringHash(defName));
		ResolveParentNodes(thinkRoot);
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		HashSet<int> usedKeys = new HashSet<int>();
		HashSet<ThinkNode> instances = new HashSet<ThinkNode>();
		foreach (ThinkNode node in thinkRoot.ThisAndChildrenRecursive)
		{
			int key = node.UniqueSaveKey;
			if (key == -1)
			{
				yield return "Thinknode " + node.GetType()?.ToString() + " has invalid save key " + key;
			}
			else if (instances.Contains(node))
			{
				yield return "There are two same ThinkNode instances in one think tree (their type is " + node.GetType()?.ToString() + ")";
			}
			else if (usedKeys.Contains(key))
			{
				yield return "Two ThinkNodes have the same unique save key " + key + " (one of the nodes is " + node.GetType()?.ToString() + ")";
			}
			if (key != -1)
			{
				usedKeys.Add(key);
			}
			instances.Add(node);
		}
	}

	public bool TryGetThinkNodeWithSaveKey(int key, out ThinkNode outNode)
	{
		outNode = null;
		if (key == -1)
		{
			return false;
		}
		if (key == thinkRoot.UniqueSaveKey)
		{
			outNode = thinkRoot;
			return true;
		}
		foreach (ThinkNode item in thinkRoot.ChildrenRecursive)
		{
			if (item.UniqueSaveKey == key)
			{
				outNode = item;
				return true;
			}
		}
		return false;
	}

	private void ResolveParentNodes(ThinkNode node)
	{
		for (int i = 0; i < node.subNodes.Count; i++)
		{
			if (node.subNodes[i].parent != null)
			{
				Log.Warning("Think node " + node.subNodes[i]?.ToString() + " from think tree " + defName + " already has a parent node (" + node.subNodes[i].parent?.ToString() + "). This means that it's referenced by more than one think tree (should have been copied instead).");
			}
			else
			{
				node.subNodes[i].parent = node;
				ResolveParentNodes(node.subNodes[i]);
			}
		}
	}
}
