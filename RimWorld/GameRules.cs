using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class GameRules : IExposable
{
	private HashSet<Type> disallowedDesignatorTypes = new HashSet<Type>();

	private HashSet<ThingDef> disallowedBuildings = new HashSet<ThingDef>();

	public void SetAllowDesignator(Type type, bool allowed)
	{
		if (allowed && disallowedDesignatorTypes.Contains(type))
		{
			disallowedDesignatorTypes.Remove(type);
		}
		if (!allowed && !disallowedDesignatorTypes.Contains(type))
		{
			disallowedDesignatorTypes.Add(type);
		}
		Find.ReverseDesignatorDatabase.Reinit();
	}

	public void SetAllowBuilding(ThingDef building, bool allowed)
	{
		if (allowed && disallowedBuildings.Contains(building))
		{
			disallowedBuildings.Remove(building);
		}
		if (!allowed && !disallowedBuildings.Contains(building))
		{
			disallowedBuildings.Add(building);
		}
	}

	public bool DesignatorAllowed(Designator d)
	{
		if (d is Designator_Place designator_Place)
		{
			return !disallowedBuildings.Contains(designator_Place.PlacingDef);
		}
		foreach (Type disallowedDesignatorType in disallowedDesignatorTypes)
		{
			if (disallowedDesignatorType.IsAssignableFrom(d.GetType()))
			{
				return false;
			}
		}
		return !disallowedDesignatorTypes.Contains(d.GetType());
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref disallowedBuildings, "disallowedBuildings");
		Scribe_Collections.Look(ref disallowedDesignatorTypes, "disallowedDesignatorTypes");
	}
}
