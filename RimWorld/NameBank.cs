using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class NameBank
{
	public PawnNameCategory nameType;

	private List<string>[,] names;

	private static readonly int numGenders = Enum.GetValues(typeof(Gender)).Length;

	private static readonly int numSlots = Enum.GetValues(typeof(PawnNameSlot)).Length;

	private IEnumerable<List<string>> AllNameLists
	{
		get
		{
			for (int i = 0; i < numGenders; i++)
			{
				for (int j = 0; j < numSlots; j++)
				{
					yield return names[i, j];
				}
			}
		}
	}

	public NameBank(PawnNameCategory ID)
	{
		nameType = ID;
		names = new List<string>[numGenders, numSlots];
		for (int i = 0; i < numGenders; i++)
		{
			for (int j = 0; j < numSlots; j++)
			{
				names[i, j] = new List<string>();
			}
		}
	}

	public void ErrorCheck()
	{
		foreach (List<string> allNameList in AllNameLists)
		{
			foreach (string item in (from x in allNameList
				group x by x into g
				where g.Count() > 1
				select g.Key).ToList())
			{
				Log.Error("Duplicated name: " + item);
			}
			foreach (string item2 in allNameList)
			{
				if (item2.Trim() != item2)
				{
					Log.Error("Trimmable whitespace on name: [" + item2 + "]");
				}
			}
		}
	}

	private List<string> NamesFor(PawnNameSlot slot, Gender gender)
	{
		return names[(uint)gender, (uint)slot];
	}

	public void AddNames(PawnNameSlot slot, Gender gender, IEnumerable<string> namesToAdd)
	{
		foreach (string item in namesToAdd)
		{
			NamesFor(slot, gender).Add(item);
		}
	}

	public void AddNamesFromFile(PawnNameSlot slot, Gender gender, string fileName)
	{
		AddNames(slot, gender, GenFile.LinesFromFile("Names/" + fileName));
	}

	public string GetName(PawnNameSlot slot, Gender gender = Gender.None, bool checkIfAlreadyUsed = true)
	{
		List<string> list = NamesFor(slot, gender);
		int num = 0;
		if (list.Count == 0)
		{
			Log.Error("Name list for gender=" + gender.ToString() + " slot=" + slot.ToString() + " is empty.");
			return "Errorname";
		}
		string text;
		do
		{
			text = list.RandomElement();
			if (checkIfAlreadyUsed && !NameUseChecker.NameWordIsUsed(text))
			{
				return text;
			}
			num++;
		}
		while (num <= 50);
		return text;
	}
}
