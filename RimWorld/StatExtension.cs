using Verse;

namespace RimWorld;

public static class StatExtension
{
	public static float GetStatValue(this Thing thing, StatDef stat, bool applyPostProcess = true, int cacheStaleAfterTicks = -1)
	{
		return stat.Worker.GetValue(thing, applyPostProcess, cacheStaleAfterTicks);
	}

	public static float GetStatValueForPawn(this Thing thing, StatDef stat, Pawn pawn, bool applyPostProcess = true)
	{
		return stat.Worker.GetValue(thing, pawn, applyPostProcess);
	}

	public static float GetStatValueAbstract(this BuildableDef def, StatDef stat, ThingDef stuff = null)
	{
		return stat.Worker.GetValueAbstract(def, stuff);
	}

	public static float GetStatValueAbstract(this AbilityDef def, StatDef stat, Pawn forPawn = null)
	{
		return stat.Worker.GetValueAbstract(def, forPawn);
	}

	public static bool StatBaseDefined(this BuildableDef def, StatDef stat)
	{
		if (def.statBases == null)
		{
			return false;
		}
		for (int i = 0; i < def.statBases.Count; i++)
		{
			if (def.statBases[i].stat == stat)
			{
				return true;
			}
		}
		return false;
	}

	public static void SetStatBaseValue(this BuildableDef def, StatDef stat, float newBaseValue)
	{
		StatUtility.SetStatValueInList(ref def.statBases, stat, newBaseValue);
	}
}
