using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI.Group;

public class StateGraph : IDisposable
{
	public List<LordToil> lordToils = new List<LordToil>();

	public List<Transition> transitions = new List<Transition>();

	private static HashSet<LordToil> checkedToils;

	public LordToil StartingToil
	{
		get
		{
			return lordToils[0];
		}
		set
		{
			if (lordToils.Contains(value))
			{
				lordToils.Remove(value);
			}
			lordToils.Insert(0, value);
		}
	}

	public void AddToil(LordToil toil)
	{
		lordToils.Add(toil);
	}

	public void AddTransition(Transition transition, bool highPriority = false)
	{
		if (highPriority)
		{
			transitions.Insert(0, transition);
		}
		else
		{
			transitions.Add(transition);
		}
	}

	public StateGraph AttachSubgraph(StateGraph subGraph)
	{
		for (int i = 0; i < subGraph.lordToils.Count; i++)
		{
			lordToils.Add(subGraph.lordToils[i]);
		}
		for (int j = 0; j < subGraph.transitions.Count; j++)
		{
			transitions.Add(subGraph.transitions[j]);
		}
		return subGraph;
	}

	public void ErrorCheck()
	{
		if (lordToils.Count == 0)
		{
			Log.Error("Graph has 0 lord toils.");
		}
		foreach (LordToil toil in lordToils.Distinct())
		{
			int num = lordToils.Count((LordToil s) => s == toil);
			if (num != 1)
			{
				Log.Error("Graph has lord toil " + toil?.ToString() + " registered " + num + " times.");
			}
		}
		foreach (Transition trans in transitions)
		{
			int num2 = transitions.Count((Transition t) => t == trans);
			if (num2 != 1)
			{
				Log.Error("Graph has transition " + trans?.ToString() + " registered " + num2 + " times.");
			}
		}
		checkedToils = new HashSet<LordToil>();
		CheckForUnregisteredLinkedToilsRecursive(StartingToil);
		checkedToils = null;
	}

	private void CheckForUnregisteredLinkedToilsRecursive(LordToil toil)
	{
		if (!lordToils.Contains(toil))
		{
			Log.Error("Unregistered linked lord toil: " + toil);
		}
		checkedToils.Add(toil);
		for (int i = 0; i < transitions.Count; i++)
		{
			Transition transition = transitions[i];
			if (transition.sources.Contains(toil) && !checkedToils.Contains(toil))
			{
				CheckForUnregisteredLinkedToilsRecursive(transition.target);
			}
		}
	}

	public void Dispose()
	{
		foreach (LordToil lordToil in lordToils)
		{
			lordToil.Dispose();
		}
	}
}
