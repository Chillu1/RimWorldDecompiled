using System;
using Verse;

namespace RimWorld;

public static class TechLevelUtility
{
	public static string ToStringHuman(this TechLevel tl)
	{
		return tl switch
		{
			TechLevel.Undefined => "Undefined".Translate(), 
			TechLevel.Animal => "TechLevel_Animal".Translate(), 
			TechLevel.Neolithic => "TechLevel_Neolithic".Translate(), 
			TechLevel.Medieval => "TechLevel_Medieval".Translate(), 
			TechLevel.Industrial => "TechLevel_Industrial".Translate(), 
			TechLevel.Spacer => "TechLevel_Spacer".Translate(), 
			TechLevel.Ultra => "TechLevel_Ultra".Translate(), 
			TechLevel.Archotech => "TechLevel_Archotech".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static bool CanSpawnWithEquipmentFrom(this TechLevel pawnLevel, TechLevel gearLevel)
	{
		if (gearLevel == TechLevel.Undefined)
		{
			return false;
		}
		switch (pawnLevel)
		{
		case TechLevel.Undefined:
			return false;
		case TechLevel.Neolithic:
			return (int)gearLevel <= 2;
		case TechLevel.Medieval:
			return (int)gearLevel <= 3;
		case TechLevel.Industrial:
			return gearLevel == TechLevel.Industrial;
		case TechLevel.Spacer:
			if (gearLevel != TechLevel.Spacer)
			{
				return gearLevel == TechLevel.Industrial;
			}
			return true;
		case TechLevel.Ultra:
			if (gearLevel != TechLevel.Ultra)
			{
				return gearLevel == TechLevel.Spacer;
			}
			return true;
		case TechLevel.Archotech:
			return gearLevel == TechLevel.Archotech;
		default:
			Log.Error("Unknown tech levels " + pawnLevel.ToString() + ", " + gearLevel);
			return true;
		}
	}

	public static bool IsNeolithicOrWorse(this TechLevel techLevel)
	{
		if (techLevel == TechLevel.Undefined)
		{
			return false;
		}
		return (int)techLevel <= 2;
	}
}
