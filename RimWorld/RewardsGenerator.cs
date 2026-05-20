using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class RewardsGenerator
{
	private const float ThingRewardOnlyChance = 0.3f;

	private const float SocialRewardOnlyChance = 0.3f;

	private const float SocialRewardOnlyChance_RoyalFavorPossible = 0.6f;

	private const float ThingRewardMinFractionOfTotal = 0.3f;

	private const float SocialRewardMinFractionOfTotal = 0.3f;

	public const float ItemsValueFractionMaxVariance = 0.3f;

	private const float ThingRewardGenerateFirstChance = 0.65f;

	private const float ThingRewardSelectionWeight_Items = 3f;

	private const float ThingRewardSelectionWeight_Pawn = 1f;

	private const float SocialRewardSelectionWeight_RoyalFavor = 9f;

	private const float SocialRewardSelectionWeight_Goodwill = 1f;

	private const float MinValueForExtraSilverReward = 200f;

	private const float MinValueToGenerateSecondRewardType = 600f;

	private static readonly List<ThingDef> MarketValueFillers = new List<ThingDef>();

	public static readonly SimpleCurve RewardValueToRoyalFavorCurve = new SimpleCurve
	{
		new CurvePoint(100f, 2f),
		new CurvePoint(500f, 4f),
		new CurvePoint(2000f, 10f),
		new CurvePoint(5000f, 18f)
	};

	public static readonly SimpleCurve RewardValueToGoodwillCurve = new SimpleCurve
	{
		new CurvePoint(100f, 10f),
		new CurvePoint(500f, 15f),
		new CurvePoint(1000f, 20f),
		new CurvePoint(2000f, 35f),
		new CurvePoint(5000f, 50f)
	};

	public static void ResetStaticData()
	{
		MarketValueFillers.Clear();
		MarketValueFillers.Add(ThingDefOf.Silver);
		MarketValueFillers.Add(ThingDefOf.Gold);
		MarketValueFillers.Add(ThingDefOf.Uranium);
		MarketValueFillers.Add(ThingDefOf.Jade);
		MarketValueFillers.Add(ThingDefOf.Plasteel);
	}

	public static List<Reward> Generate(RewardsGeneratorParams parms)
	{
		float generatedRewardValue;
		return Generate(parms, out generatedRewardValue);
	}

	public static List<Reward> Generate(RewardsGeneratorParams parms, out float generatedRewardValue)
	{
		try
		{
			return DoGenerate(parms, out generatedRewardValue);
		}
		finally
		{
		}
	}

	private static List<Reward> DoGenerate(RewardsGeneratorParams parms, out float generatedRewardValue)
	{
		List<Reward> list = new List<Reward>();
		string text = parms.ConfigError();
		if (text != null)
		{
			Log.Error("Invalid reward generation params: " + text);
			generatedRewardValue = 0f;
			return list;
		}
		parms.rewardValue = Mathf.Max(parms.rewardValue, parms.minGeneratedRewardValue);
		bool flag = parms.allowGoodwill && parms.giverFaction != null && parms.giverFaction != Faction.OfPlayer && parms.giverFaction.CanEverGiveGoodwillRewards && parms.giverFaction.allowGoodwillRewards && parms.giverFaction.PlayerGoodwill <= 92;
		bool flag2 = parms.allowRoyalFavor && parms.giverFaction != null && parms.giverFaction.allowRoyalFavorRewards && parms.giverFaction.def.HasRoyalTitles;
		bool flag3 = flag2 || flag;
		bool flag4 = parms.giverFaction != null && parms.giverFaction.HostileTo(Faction.OfPlayer);
		bool flag5 = flag2 && Faction.OfEmpire != null && parms.giverFaction == Faction.OfEmpire && !parms.thingRewardItemsOnly;
		bool flag6;
		bool flag7;
		if (!parms.thingRewardDisallowed && !flag3)
		{
			flag6 = true;
			flag7 = false;
		}
		else if (parms.thingRewardDisallowed && flag3)
		{
			flag6 = false;
			flag7 = true;
		}
		else if (parms.thingRewardDisallowed && !flag3)
		{
			flag6 = false;
			flag7 = false;
		}
		else
		{
			float num = (flag2 ? 0.6f : 0.3f);
			float value = Rand.Value;
			if (value < 0.3f && !flag5 && !flag4)
			{
				flag6 = true;
				flag7 = false;
			}
			else if (parms.thingRewardRequired)
			{
				flag6 = true;
				flag7 = true;
			}
			else if (value < 0.3f + num)
			{
				flag6 = false;
				flag7 = true;
			}
			else
			{
				flag6 = !flag4;
				flag7 = true;
			}
		}
		float num2;
		float num3;
		if (flag6 && !flag7)
		{
			num2 = parms.rewardValue;
			num3 = 0f;
		}
		else if (!flag6 && flag7)
		{
			num2 = 0f;
			num3 = parms.rewardValue;
		}
		else
		{
			if (!(flag6 && flag7))
			{
				generatedRewardValue = 0f;
				return list;
			}
			float num4 = Rand.Range(0.3f, 0.7f);
			float num5 = 1f - num4;
			num2 = parms.rewardValue * num4;
			num3 = parms.rewardValue * num5;
		}
		float valueActuallyUsed = 0f;
		float valueActuallyUsed2 = 0f;
		Reward reward = null;
		Reward reward2 = null;
		if (Rand.Value < 0.65f && !flag5)
		{
			if (num2 > 0f)
			{
				reward = GenerateThingReward(num2, parms, out valueActuallyUsed);
				if (flag7 || (flag3 && num2 - valueActuallyUsed >= 600f))
				{
					num3 += num2 - valueActuallyUsed;
				}
			}
			if (num3 > 0f)
			{
				reward2 = GenerateSocialReward(num3, parms, flag, flag2, out valueActuallyUsed2);
			}
		}
		else
		{
			if (num3 > 0f)
			{
				reward2 = GenerateSocialReward(num3, parms, flag, flag2, out valueActuallyUsed2);
				if (flag6 || (!parms.thingRewardDisallowed && num3 - valueActuallyUsed2 >= 600f && !flag4))
				{
					num2 += num3 - valueActuallyUsed2;
				}
			}
			if (num2 > 0f)
			{
				reward = GenerateThingReward(num2, parms, out valueActuallyUsed);
			}
		}
		generatedRewardValue = valueActuallyUsed + valueActuallyUsed2;
		Reward_Items reward_Items = null;
		float num6 = parms.rewardValue - valueActuallyUsed - valueActuallyUsed2;
		if ((num6 >= 200f || valueActuallyUsed + valueActuallyUsed2 < parms.minGeneratedRewardValue) && !parms.thingRewardDisallowed)
		{
			reward_Items = AddMarketValueFillers(num6, ref generatedRewardValue, reward);
		}
		if (reward != null)
		{
			list.Add(reward);
		}
		if (reward2 != null)
		{
			list.Add(reward2);
		}
		if (reward_Items != null)
		{
			list.Add(reward_Items);
		}
		return list;
	}

	private static Reward GenerateSocialReward(float rewardValue, RewardsGeneratorParams parms, bool allowGoodwill, bool allowRoyalFavor, out float valueActuallyUsed)
	{
		if (!allowGoodwill && !allowRoyalFavor)
		{
			RewardsGeneratorParams rewardsGeneratorParams = parms;
			Log.Error("GenerateSocialReward could not generate any reward for parms=" + rewardsGeneratorParams.ToString());
			allowGoodwill = true;
		}
		float valueActuallyUsedLocal = 0f;
		Func<Reward> func = () => GenerateReward<Reward_Goodwill>(rewardValue, parms, out valueActuallyUsedLocal);
		Func<Reward> b = () => GenerateReward<Reward_RoyalFavor>(rewardValue, parms, out valueActuallyUsedLocal);
		if (allowGoodwill && parms.giverFaction != null && parms.giverFaction.HostileTo(Faction.OfPlayer))
		{
			Reward result = func();
			valueActuallyUsed = valueActuallyUsedLocal;
			return result;
		}
		float weightA = (allowGoodwill ? 1f : 0f);
		float weightB = (allowRoyalFavor ? 9f : 0f);
		Reward result2 = Rand.ElementByWeight(func, weightA, b, weightB)();
		valueActuallyUsed = valueActuallyUsedLocal;
		return result2;
	}

	private static Reward GenerateThingReward(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		float valueActuallyUsedLocal = 0f;
		Func<Reward> func = () => GenerateReward<Reward_Items>(rewardValue, parms, out valueActuallyUsedLocal);
		Func<Reward> b = () => GenerateReward<Reward_Pawn>(rewardValue, parms, out valueActuallyUsedLocal);
		if (parms.thingRewardItemsOnly)
		{
			Reward result = func();
			valueActuallyUsed = valueActuallyUsedLocal;
			return result;
		}
		float weightB = Mathf.Max(0f, 1f * parms.populationIntent);
		Reward result2 = Rand.ElementByWeight(func, 3f, b, weightB)();
		valueActuallyUsed = valueActuallyUsedLocal;
		return result2;
	}

	private static T GenerateReward<T>(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed) where T : Reward, new()
	{
		try
		{
			T val = new T();
			val.InitFromValue(rewardValue, parms, out valueActuallyUsed);
			return val;
		}
		finally
		{
		}
	}

	private static Reward_Items AddMarketValueFillers(float remainingValue, ref float generatedRewardValue, Reward thingReward)
	{
		IEnumerable<ThingDef> source = MarketValueFillers.Where((ThingDef x) => remainingValue / x.BaseMarketValue >= 15f);
		if (!source.Any())
		{
			return null;
		}
		ThingDef thingDef = null;
		Reward_Items existingItemsReward = thingReward as Reward_Items;
		if (existingItemsReward != null)
		{
			thingDef = source.FirstOrDefault((ThingDef x) => existingItemsReward.items.Any((Thing y) => y.def == x));
		}
		if (thingDef == null)
		{
			thingDef = source.RandomElement();
		}
		int num = GenMath.RoundRandom(remainingValue / thingDef.BaseMarketValue);
		if (num >= 1)
		{
			Thing thing = ThingMaker.MakeThing(thingDef);
			thing.stackCount = num;
			generatedRewardValue += thing.MarketValue * (float)thing.stackCount;
			if (!(thingReward is Reward_Items reward_Items))
			{
				return new Reward_Items
				{
					items = { thing }
				};
			}
			reward_Items.items.Add(thing);
		}
		return null;
	}
}
