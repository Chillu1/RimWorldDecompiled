using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class PawnNameDatabaseSolid
	{
		private static Dictionary<GenderPossibility, List<NameTriple>> solidNames;

		private const float PreferredNameChance = 0.5f;

		static PawnNameDatabaseSolid()
		{
			solidNames = new Dictionary<GenderPossibility, List<NameTriple>>();
			foreach (GenderPossibility value in Enum.GetValues(typeof(GenderPossibility)))
			{
				solidNames.Add(value, new List<NameTriple>());
			}
		}

		public static void AddPlayerContentName(NameTriple newName, GenderPossibility genderPos)
		{
			solidNames[genderPos].Add(newName);
		}

		public static List<NameTriple> GetListForGender(GenderPossibility gp)
		{
			return solidNames[gp];
		}

		public static IEnumerable<NameTriple> AllNames()
		{
			foreach (KeyValuePair<GenderPossibility, List<NameTriple>> solidName in solidNames)
			{
				foreach (NameTriple item in solidName.Value)
				{
					yield return item;
				}
			}
		}
	}
}
