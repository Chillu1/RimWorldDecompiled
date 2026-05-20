using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class ResearchPrerequisitesUtility
{
	public class UnlockedHeader : IEquatable<UnlockedHeader>
	{
		public List<ResearchProjectDef> unlockedBy;

		public UnlockedHeader(List<ResearchProjectDef> unlockedBy)
		{
			this.unlockedBy = unlockedBy;
		}

		public bool Equals(UnlockedHeader other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			return unlockedBy.SequenceEqual(other.unlockedBy);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((UnlockedHeader)obj);
		}

		public override int GetHashCode()
		{
			if (!unlockedBy.Any())
			{
				return 23;
			}
			return unlockedBy.First().GetHashCode();
		}
	}

	private static Dictionary<Def, List<ResearchProjectDef>> ComputeResearchPrerequisites()
	{
		Dictionary<Def, List<ResearchProjectDef>> dictionary = new Dictionary<Def, List<ResearchProjectDef>>();
		foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
		{
			foreach (Def unlockedDef in allDef.UnlockedDefs)
			{
				if (!dictionary.TryGetValue(unlockedDef, out var value))
				{
					value = new List<ResearchProjectDef>();
					dictionary.Add(unlockedDef, value);
				}
				value.Add(allDef);
			}
		}
		return dictionary;
	}

	public static List<Pair<UnlockedHeader, List<Def>>> UnlockedDefsGroupedByPrerequisites(ResearchProjectDef rd)
	{
		Dictionary<Def, List<ResearchProjectDef>> dictionary = ComputeResearchPrerequisites();
		List<Pair<Def, UnlockedHeader>> list = new List<Pair<Def, UnlockedHeader>>();
		foreach (Def unlockedDef in rd.UnlockedDefs)
		{
			list.Add(new Pair<Def, UnlockedHeader>(unlockedDef, new UnlockedHeader(dictionary[unlockedDef].Except(rd).ToList())));
		}
		return (from pair in list
			orderby pair.Second.unlockedBy.Count
			group pair.First by pair.Second).Select(GenCollection.ConvertIGroupingToPair).ToList();
	}
}
