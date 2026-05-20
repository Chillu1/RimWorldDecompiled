using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class SelectorUtility
{
	private static readonly List<Thing> tmp_thingsToSort = new List<Thing>();

	public static void SortInColonistBarOrder(List<Thing> things)
	{
		tmp_thingsToSort.Clear();
		tmp_thingsToSort.AddRange(things);
		things.Clear();
		foreach (Pawn item in Find.ColonistBar.GetColonistsInOrder())
		{
			int num = tmp_thingsToSort.IndexOf(item);
			if (num != -1)
			{
				things.Add(item);
				tmp_thingsToSort.RemoveAt(num);
			}
		}
		things.AddRange(tmp_thingsToSort);
		tmp_thingsToSort.Clear();
	}

	public static bool IsEquivalentRace(Pawn a, Pawn b)
	{
		if (ModsConfig.AnomalyActive && ((a.def == ThingDefOf.Human && b.def == ThingDefOf.CreepJoiner) || (b.def == ThingDefOf.Human && a.def == ThingDefOf.CreepJoiner)))
		{
			return true;
		}
		return a.RaceProps == b.RaceProps;
	}
}
