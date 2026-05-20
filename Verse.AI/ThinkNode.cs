using System;
using System.Collections.Generic;

namespace Verse.AI;

public abstract class ThinkNode
{
	public List<ThinkNode> subNodes = new List<ThinkNode>();

	public bool leaveJoinableLordIfIssuesJob;

	protected float priority = -1f;

	[Unsaved(false)]
	private int uniqueSaveKeyInt = -2;

	[NoTranslate]
	public string tag;

	[Unsaved(false)]
	public ThinkNode parent;

	[Unsaved(false)]
	private bool hasPriority;

	public const int InvalidSaveKey = -1;

	protected const int UnresolvedSaveKey = -2;

	public int UniqueSaveKey => uniqueSaveKeyInt;

	public IEnumerable<ThinkNode> ThisAndChildrenRecursive
	{
		get
		{
			yield return this;
			foreach (ThinkNode item in ChildrenRecursive)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<ThinkNode> ChildrenRecursive
	{
		get
		{
			if (subNodes.Count == 0)
			{
				yield break;
			}
			Stack<ThinkNode> stack = new Stack<ThinkNode>();
			for (int num = subNodes.Count - 1; num >= 0; num--)
			{
				stack.Push(subNodes[num]);
			}
			while (stack.Count != 0)
			{
				ThinkNode elem = stack.Pop();
				yield return elem;
				for (int num2 = elem.subNodes.Count - 1; num2 >= 0; num2--)
				{
					stack.Push(elem.subNodes[num2]);
				}
			}
		}
	}

	protected ThinkNode()
	{
		hasPriority = priority >= 0f || GetType().IsMethodOverriden("GetPriority");
	}

	public T FirstNodeOfType<T>(bool includeThis = false) where T : ThinkNode
	{
		if (includeThis && this is T result)
		{
			return result;
		}
		foreach (ThinkNode subNode in subNodes)
		{
			T val = subNode.FirstNodeOfType<T>(includeThis: true);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	public bool TryGetPriority(Pawn pawn, out float value)
	{
		if (!hasPriority)
		{
			value = 0f;
			return false;
		}
		value = GetPriority(pawn);
		return true;
	}

	public virtual float GetPriority(Pawn pawn)
	{
		if (priority < 0f)
		{
			Log.ErrorOnce("ThinkNode_PrioritySorter has child node which didn't give a priority: " + this, GetHashCode());
			return 0f;
		}
		return priority;
	}

	public abstract ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams);

	protected virtual void ResolveSubnodes()
	{
	}

	public void ResolveSubnodesAndRecur()
	{
		if (uniqueSaveKeyInt == -2)
		{
			ResolveSubnodes();
			for (int i = 0; i < subNodes.Count; i++)
			{
				subNodes[i].ResolveSubnodesAndRecur();
			}
		}
	}

	public virtual void ResolveReferences()
	{
	}

	public virtual ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode thinkNode = (ThinkNode)Activator.CreateInstance(GetType());
		for (int i = 0; i < subNodes.Count; i++)
		{
			thinkNode.subNodes.Add(subNodes[i].DeepCopy(resolve));
		}
		thinkNode.priority = priority;
		thinkNode.leaveJoinableLordIfIssuesJob = leaveJoinableLordIfIssuesJob;
		thinkNode.uniqueSaveKeyInt = uniqueSaveKeyInt;
		thinkNode.tag = tag;
		if (resolve)
		{
			thinkNode.ResolveSubnodesAndRecur();
		}
		ThinkTreeKeyAssigner.AssignSingleKey(thinkNode, 0);
		return thinkNode;
	}

	internal void SetUniqueSaveKey(int key)
	{
		uniqueSaveKeyInt = key;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineInt(uniqueSaveKeyInt, 1157295731);
	}
}
