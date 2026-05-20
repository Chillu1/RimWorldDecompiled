using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Pawn_FoodRestrictionTracker : IExposable
{
	public Pawn pawn;

	private FoodPolicy curPolicy;

	private Dictionary<ThingDef, bool> allowedBabyFoodTypes;

	public FoodPolicy CurrentFoodPolicy
	{
		get
		{
			if (pawn.IsMutant && pawn.mutant.Def.disablePolicies)
			{
				return null;
			}
			if (curPolicy == null)
			{
				curPolicy = Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
			}
			return curPolicy;
		}
		set
		{
			curPolicy = value;
		}
	}

	public bool Configurable
	{
		get
		{
			if (pawn.RaceProps.Humanlike && !pawn.Destroyed)
			{
				if (pawn.Faction != Faction.OfPlayer)
				{
					return pawn.HostFaction == Faction.OfPlayer;
				}
				return true;
			}
			return false;
		}
	}

	public Pawn_FoodRestrictionTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public Pawn_FoodRestrictionTracker()
	{
	}

	public FoodPolicy GetCurrentRespectedRestriction(Pawn getter = null)
	{
		if (!Configurable)
		{
			return null;
		}
		if (pawn.Faction != Faction.OfPlayer && (getter == null || getter.Faction != Faction.OfPlayer))
		{
			return null;
		}
		if (pawn.InMentalState)
		{
			return null;
		}
		return CurrentFoodPolicy;
	}

	public bool BabyFoodAllowed(ThingDef food)
	{
		TrySetupAllowedBabyFoodTypes();
		if (!ITab_Pawn_Feeding.BabyConsumableFoods.Contains(food))
		{
			return false;
		}
		if (!allowedBabyFoodTypes.ContainsKey(food))
		{
			allowedBabyFoodTypes.Add(food, value: true);
		}
		return allowedBabyFoodTypes[food];
	}

	public void SetBabyFoodAllowed(ThingDef food, bool allowed)
	{
		TrySetupAllowedBabyFoodTypes();
		if (ITab_Pawn_Feeding.BabyConsumableFoods.Contains(food))
		{
			allowedBabyFoodTypes.SetOrAdd(food, allowed);
		}
	}

	private void TrySetupAllowedBabyFoodTypes()
	{
		if (allowedBabyFoodTypes != null)
		{
			return;
		}
		allowedBabyFoodTypes = new Dictionary<ThingDef, bool>();
		foreach (ThingDef babyConsumableFood in ITab_Pawn_Feeding.BabyConsumableFoods)
		{
			allowedBabyFoodTypes.Add(babyConsumableFood, value: true);
		}
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref curPolicy, "curRestriction");
		Scribe_Collections.Look(ref allowedBabyFoodTypes, "allowedBabyFoodTypes", LookMode.Def, LookMode.Value);
	}
}
