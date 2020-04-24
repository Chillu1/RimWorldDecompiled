using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GameRules : IExposable
	{
		private HashSet<Type> disallowedDesignatorTypes = new HashSet<Type>();

		private HashSet<BuildableDef> disallowedBuildings = new HashSet<BuildableDef>();

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

		public void SetAllowBuilding(BuildableDef building, bool allowed)
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
			Designator_Place designator_Place = d as Designator_Place;
			if (designator_Place != null)
			{
				return !disallowedBuildings.Contains(designator_Place.PlacingDef);
			}
			return !disallowedDesignatorTypes.Contains(d.GetType());
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref disallowedBuildings, "disallowedBuildings");
			Scribe_Collections.Look(ref disallowedDesignatorTypes, "disallowedDesignatorTypes");
		}
	}
}
