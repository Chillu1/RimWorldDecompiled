using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class PawnNamingUtility
{
	public static Dialog_NamePawn NamePawnDialog(this Pawn pawn, string initialFirstNameOverride = null)
	{
		Dictionary<NameFilter, List<string>> suggestedNames = null;
		NameFilter editableNames;
		NameFilter visibleNames;
		if (pawn.babyNamingDeadline >= Find.TickManager.TicksGame || DebugSettings.ShowDevGizmos)
		{
			editableNames = NameFilter.First | NameFilter.Nick | NameFilter.Last;
			visibleNames = NameFilter.First | NameFilter.Nick | NameFilter.Last;
			List<string> list = new List<string>();
			Pawn mother = pawn.GetMother();
			if (mother != null)
			{
				list.Add(GetLastName(mother));
			}
			Pawn father = pawn.GetFather();
			if (father != null)
			{
				list.Add(GetLastName(father));
			}
			Pawn birthParent = pawn.GetBirthParent();
			if (birthParent != null)
			{
				list.Add(GetLastName(birthParent));
			}
			list.RemoveDuplicates();
			suggestedNames = new Dictionary<NameFilter, List<string>> { 
			{
				NameFilter.Last,
				list
			} };
		}
		else
		{
			visibleNames = NameFilter.First | NameFilter.Nick | NameFilter.Last | NameFilter.Title;
			editableNames = NameFilter.Nick | NameFilter.Title;
		}
		return new Dialog_NamePawn(pawn, visibleNames, editableNames, suggestedNames, initialFirstNameOverride);
	}

	public static string GetLastName(Pawn pawn)
	{
		if (pawn.Name is NameTriple nameTriple)
		{
			return nameTriple.Last;
		}
		if (pawn.Name is NameSingle nameSingle)
		{
			return nameSingle.Name;
		}
		throw new NotImplementedException(pawn.Name.GetType().ToString());
	}
}
