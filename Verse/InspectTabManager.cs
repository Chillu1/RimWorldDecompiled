using System;
using System.Collections.Generic;

namespace Verse;

public static class InspectTabManager
{
	private static Dictionary<Type, InspectTabBase> sharedInstances = new Dictionary<Type, InspectTabBase>();

	public static InspectTabBase GetSharedInstance(Type tabType)
	{
		if (sharedInstances.TryGetValue(tabType, out var value))
		{
			return value;
		}
		value = (InspectTabBase)Activator.CreateInstance(tabType);
		sharedInstances.Add(tabType, value);
		return value;
	}
}
