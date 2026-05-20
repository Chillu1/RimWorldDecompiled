using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public static class StatUtility
{
	public static void SetStatValueInList(ref List<StatModifier> statList, StatDef stat, float value)
	{
		if (statList == null)
		{
			statList = new List<StatModifier>();
		}
		for (int i = 0; i < statList.Count; i++)
		{
			if (statList[i].stat == stat)
			{
				statList[i].value = value;
				return;
			}
		}
		StatModifier statModifier = new StatModifier();
		statModifier.stat = stat;
		statModifier.value = value;
		statList.Add(statModifier);
	}

	public static float GetStatFactorFromList(this List<StatModifier> modList, StatDef stat)
	{
		return modList.GetStatValueFromList(stat, 1f);
	}

	public static float GetStatFactorFromList(this List<StatModifierQuality> modList, StatDef stat, QualityCategory qc)
	{
		return modList.GetStatValueFromList(stat, qc, 1f);
	}

	public static bool TryGetStatFactorRangeFromList(this List<StatModifierQuality> modList, StatDef stat, out FloatRange range)
	{
		return modList.TryGetStatValueRangeFromList(stat, out range);
	}

	public static float GetStatOffsetFromList(this List<StatModifier> modList, StatDef stat)
	{
		return modList.GetStatValueFromList(stat, 0f);
	}

	public static float GetStatOffsetFromList(this List<StatModifierQuality> modList, StatDef stat, QualityCategory qc)
	{
		return modList.GetStatValueFromList(stat, qc, 0f);
	}

	public static bool TryGetStatOffsetRangeFromList(this List<StatModifierQuality> modList, StatDef stat, out FloatRange range)
	{
		return modList.TryGetStatValueRangeFromList(stat, out range);
	}

	public static float GetStatValueFromList(this List<StatModifier> modList, StatDef stat, float defaultValue)
	{
		if (modList != null)
		{
			for (int i = 0; i < modList.Count; i++)
			{
				if (modList[i].stat == stat)
				{
					return modList[i].value;
				}
			}
		}
		return defaultValue;
	}

	public static float GetStatValueFromList(this List<StatModifierQuality> modList, StatDef stat, QualityCategory qc, float defaultValue)
	{
		if (modList != null)
		{
			foreach (StatModifierQuality mod in modList)
			{
				if (mod.stat == stat)
				{
					return mod.GetValue(qc);
				}
			}
		}
		return defaultValue;
	}

	public static bool TryGetStatValueRangeFromList(this List<StatModifierQuality> modList, StatDef stat, out FloatRange range)
	{
		if (modList != null)
		{
			foreach (StatModifierQuality mod in modList)
			{
				if (mod.stat == stat)
				{
					range = new FloatRange(mod.GetValue(QualityCategory.Awful), mod.GetValue(QualityCategory.Legendary));
					return true;
				}
			}
		}
		range = default(FloatRange);
		return false;
	}

	public static bool StatListContains(this List<StatModifier> modList, StatDef stat)
	{
		if (modList != null)
		{
			for (int i = 0; i < modList.Count; i++)
			{
				if (modList[i].stat == stat)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetOffsetsAndFactorsFor(StatDef stat, Thing thing)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StatRequest statRequest = StatRequest.For(thing);
		float baseValueFor = stat.Worker.GetBaseValueFor(statRequest);
		ToStringNumberSense toStringNumberSense = stat.toStringNumberSense;
		stat.Worker.GetOffsetsAndFactorsExplanation(statRequest, stringBuilder, baseValueFor, "    ");
		stat.Worker.GetAdditionalOffsetsAndFactorsExplanation(statRequest, toStringNumberSense, stringBuilder, "    ");
		return stringBuilder.ToString();
	}
}
