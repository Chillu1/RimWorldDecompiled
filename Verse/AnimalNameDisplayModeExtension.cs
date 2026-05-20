using System;

namespace Verse;

public static class AnimalNameDisplayModeExtension
{
	public static string ToStringHuman(this AnimalNameDisplayMode mode)
	{
		return mode switch
		{
			AnimalNameDisplayMode.None => "None".Translate(), 
			AnimalNameDisplayMode.TameNamed => "AnimalNameDisplayMode_TameNamed".Translate(), 
			AnimalNameDisplayMode.TameAll => "AnimalNameDisplayMode_TameAll".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static bool ShouldDisplayAnimalName(this AnimalNameDisplayMode mode, Pawn animal)
	{
		switch (mode)
		{
		case AnimalNameDisplayMode.None:
			return false;
		case AnimalNameDisplayMode.TameAll:
			return animal.Name != null;
		case AnimalNameDisplayMode.TameNamed:
			if (animal.Name != null)
			{
				return !animal.Name.Numerical;
			}
			return false;
		default:
			throw new NotImplementedException(Prefs.AnimalNameMode.ToStringSafe());
		}
	}
}
