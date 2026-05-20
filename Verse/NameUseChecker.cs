using System.Collections.Generic;
using RimWorld;

namespace Verse;

public static class NameUseChecker
{
	public static IEnumerable<Name> AllPawnsNamesEverUsed
	{
		get
		{
			foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				if (item.Name != null)
				{
					yield return item.Name;
				}
			}
		}
	}

	public static bool NameWordIsUsed(string singleName)
	{
		foreach (Name item in AllPawnsNamesEverUsed)
		{
			if (item is NameTriple nameTriple && (singleName == nameTriple.First || singleName == nameTriple.Nick || singleName == nameTriple.Last))
			{
				return true;
			}
			if (item is NameSingle nameSingle && nameSingle.Name == singleName)
			{
				return true;
			}
		}
		return false;
	}

	public static bool NameSingleIsUsed(string candidate)
	{
		foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
		{
			if (item.Name is NameSingle nameSingle && nameSingle.Name == candidate)
			{
				return true;
			}
		}
		return false;
	}

	public static bool XenotypeNameIsUsed(string candidate)
	{
		foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
		{
			if (item.genes != null && item.genes.UniqueXenotype)
			{
				string xenotypeName = item.genes.xenotypeName;
				if (candidate == xenotypeName)
				{
					return true;
				}
			}
		}
		return false;
	}
}
