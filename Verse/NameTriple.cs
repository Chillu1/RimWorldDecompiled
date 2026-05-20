using System;

namespace Verse;

public class NameTriple : Name
{
	[LoadAlias("first")]
	private string firstInt;

	[LoadAlias("nick")]
	private string nickInt;

	[LoadAlias("last")]
	private string lastInt;

	private static NameTriple invalidInt = new NameTriple("Invalid", "Invalid", "Invalid");

	public string First => firstInt;

	public string Last => lastInt;

	public string Nick
	{
		get
		{
			if (!nickInt.NullOrEmpty())
			{
				return nickInt;
			}
			if (Last == "")
			{
				return First;
			}
			if (First == "")
			{
				return Last;
			}
			if ((Gen.HashCombine(First.GetHashCode(), Last.GetHashCode()) & 1) == 1)
			{
				return First;
			}
			return Last;
		}
	}

	public override string ToStringFull
	{
		get
		{
			if (First == Nick || Last == Nick)
			{
				return First + " " + Last;
			}
			return First + " '" + Nick + "' " + Last;
		}
	}

	public override string ToStringShort => Nick;

	public override bool IsValid
	{
		get
		{
			if (!First.NullOrEmpty())
			{
				return !Last.NullOrEmpty();
			}
			return false;
		}
	}

	public override bool Numerical => false;

	public bool NickSet => !nickInt.NullOrEmpty();

	public static NameTriple Invalid => invalidInt;

	public string this[int index] => index switch
	{
		0 => First, 
		1 => Nick, 
		2 => Last, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public NameTriple()
	{
	}

	public NameTriple(string first, string nick, string last)
	{
		firstInt = first.Trim();
		nickInt = nick?.Trim();
		lastInt = last.Trim();
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref firstInt, "first");
		Scribe_Values.Look(ref nickInt, "nick");
		Scribe_Values.Look(ref lastInt, "last");
	}

	public void PostLoad()
	{
		firstInt = firstInt?.Trim();
		nickInt = nickInt?.Trim();
		lastInt = lastInt?.Trim();
	}

	public void ResolveMissingPieces(string overrideLastName = null)
	{
		if (First.NullOrEmpty() && Nick.NullOrEmpty() && Last.NullOrEmpty())
		{
			Log.Error("Cannot resolve missing pieces in PawnName: No name data.");
			firstInt = (nickInt = (lastInt = "Empty"));
			return;
		}
		if (First == null)
		{
			firstInt = "";
		}
		if (Last == null)
		{
			lastInt = "";
		}
		if (overrideLastName != null)
		{
			lastInt = overrideLastName;
		}
	}

	public override bool ConfusinglySimilarTo(Name other)
	{
		if (other is NameTriple nameTriple)
		{
			if (Nick != null && Nick == nameTriple.Nick)
			{
				return true;
			}
			if (First == nameTriple.First && Last == nameTriple.Last)
			{
				return true;
			}
		}
		if (other is NameSingle nameSingle && nameSingle.Name == Nick)
		{
			return true;
		}
		return false;
	}

	public static NameTriple FromString(string rawName, bool forceNoNick = false)
	{
		if (rawName.Trim().Length == 0)
		{
			Log.Error("Tried to parse PawnName from empty or whitespace string.");
			return Invalid;
		}
		NameTriple nameTriple = new NameTriple();
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < rawName.Length - 1; i++)
		{
			if (rawName[i] == ' ' && rawName[i + 1] == '\'' && num == -1)
			{
				num = i;
			}
			if (rawName[i] == '\'' && rawName[i + 1] == ' ')
			{
				num2 = i;
			}
		}
		if (num == -1 || num2 == -1)
		{
			if (!rawName.Contains(' '))
			{
				nameTriple.nickInt = rawName.Trim();
			}
			else
			{
				string[] array = rawName.Split(' ');
				if (array.Length == 1)
				{
					nameTriple.nickInt = array[0].Trim();
				}
				else if (array.Length == 2)
				{
					nameTriple.firstInt = array[0].Trim();
					nameTriple.lastInt = array[1].Trim();
				}
				else
				{
					nameTriple.firstInt = array[0].Trim();
					nameTriple.lastInt = "";
					for (int j = 1; j < array.Length; j++)
					{
						nameTriple.lastInt += array[j];
						if (j < array.Length - 1)
						{
							nameTriple.lastInt += " ";
						}
					}
				}
			}
		}
		else
		{
			nameTriple.firstInt = rawName.Substring(0, num).Trim();
			if (!forceNoNick)
			{
				nameTriple.nickInt = rawName.Substring(num + 2, num2 - num - 2).Trim();
			}
			nameTriple.lastInt = ((num2 < rawName.Length - 2) ? rawName.Substring(num2 + 2).Trim() : "");
		}
		nameTriple.ResolveMissingPieces();
		return nameTriple;
	}

	public override string ToString()
	{
		return First + " '" + Nick + "' " + Last;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is NameTriple))
		{
			return false;
		}
		NameTriple nameTriple = (NameTriple)obj;
		if (First == nameTriple.First && Last == nameTriple.Last)
		{
			return Nick == nameTriple.Nick;
		}
		return false;
	}

	public NameTriple WithoutNick()
	{
		return new NameTriple(firstInt, null, lastInt);
	}

	public override int GetHashCode()
	{
		return Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(0, First), Last), Nick);
	}
}
