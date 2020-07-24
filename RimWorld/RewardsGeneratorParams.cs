using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct RewardsGeneratorParams
	{
		public float rewardValue;

		public Faction giverFaction;

		public string chosenPawnSignal;

		public bool giveToCaravan;

		public float minGeneratedRewardValue;

		public bool thingRewardDisallowed;

		public bool thingRewardRequired;

		public bool thingRewardItemsOnly;

		public List<ThingDef> disallowedThingDefs;

		public bool allowRoyalFavor;

		public bool allowGoodwill;

		public float populationIntent;

		public string ConfigError()
		{
			if (rewardValue <= 0f)
			{
				return "rewardValue is " + rewardValue;
			}
			if (thingRewardDisallowed && thingRewardRequired)
			{
				return "thing reward is both disallowed and required";
			}
			if (thingRewardDisallowed && !allowRoyalFavor && !allowGoodwill)
			{
				return "no reward types are allowed";
			}
			return null;
		}

		public override string ToString()
		{
			return GenText.FieldsToString(this);
		}
	}
}
