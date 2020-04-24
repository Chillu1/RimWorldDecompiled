using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class ThinkTreeKeyAssigner
	{
		private static HashSet<int> assignedKeys = new HashSet<int>();

		public static void Reset()
		{
			assignedKeys.Clear();
		}

		public static void AssignKeys(ThinkNode rootNode, int startHash)
		{
			Rand.PushState(startHash);
			foreach (ThinkNode item in rootNode.ThisAndChildrenRecursive)
			{
				item.SetUniqueSaveKey(NextUnusedKeyFor(item));
			}
			Rand.PopState();
		}

		public static void AssignSingleKey(ThinkNode node, int startHash)
		{
			Rand.PushState(startHash);
			node.SetUniqueSaveKey(NextUnusedKeyFor(node));
			Rand.PopState();
		}

		private static int NextUnusedKeyFor(ThinkNode node)
		{
			int num = 0;
			while (node != null)
			{
				num = Gen.HashCombineInt(num, GenText.StableStringHash(node.GetType().Name));
				node = node.parent;
			}
			while (assignedKeys.Contains(num))
			{
				num ^= Rand.Int;
			}
			assignedKeys.Add(num);
			return num;
		}
	}
}
