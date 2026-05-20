using System;
using UnityEngine;

namespace RimWorld;

public static class PriceTypeUtlity
{
	public static float PriceMultiplier(this PriceType pType)
	{
		return pType switch
		{
			PriceType.VeryCheap => 0.4f, 
			PriceType.Cheap => 0.7f, 
			PriceType.Normal => 1f, 
			PriceType.Expensive => 2f, 
			PriceType.Exorbitant => 5f, 
			_ => -1f, 
		};
	}

	public static PriceType ClosestPriceType(float priceFactor)
	{
		float num = 99999f;
		PriceType priceType = PriceType.Undefined;
		foreach (PriceType value in Enum.GetValues(typeof(PriceType)))
		{
			float num2 = Mathf.Abs(priceFactor - value.PriceMultiplier());
			if (num2 < num)
			{
				num = num2;
				priceType = value;
			}
		}
		if (priceType == PriceType.Undefined)
		{
			priceType = PriceType.VeryCheap;
		}
		return priceType;
	}
}
