using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet;

public static class ForagedFoodPerDayCalculator
{
	private static List<Pawn> tmpPawns = new List<Pawn>();

	private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

	private const float BaseProgressPerTick = 0.0001f;

	public const float NotMovingProgressFactor = 2f;

	public static (ThingDef food, float perDay) ForagedFoodPerDay(List<Pawn> pawns, BiomeDef biome, Faction faction, bool caravanMovingNow, bool caravanNightResting, StringBuilder explanation = null)
	{
		float foragedFoodCountPerInterval = GetForagedFoodCountPerInterval(pawns, biome, faction, explanation);
		float progressPerTick = GetProgressPerTick(caravanMovingNow, caravanNightResting, explanation);
		float num = foragedFoodCountPerInterval * progressPerTick * 60000f;
		float num2 = ((num == 0f) ? 0f : (num * biome.foragedFood.GetStatValueAbstract(StatDefOf.Nutrition)));
		if (explanation != null)
		{
			explanation.AppendLine();
			explanation.AppendLine();
			TaggedString taggedString = "TotalNutrition".Translate() + ": " + num2.ToString("0.##") + " / " + "day".Translate();
			if (num2 > 0f)
			{
				taggedString += "\n= " + biome.LabelCap + ": " + biome.foragedFood.LabelCap + " x" + num.ToString("0.##") + " / " + "day".Translate();
			}
			explanation.Append(taggedString);
		}
		return (food: biome.foragedFood, perDay: num);
	}

	public static float GetProgressPerTick(bool caravanMovingNow, bool caravanNightResting, StringBuilder explanation = null)
	{
		float num = 0.0001f;
		if (!caravanMovingNow && !caravanNightResting)
		{
			num *= 2f;
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.Append("CaravanNotMoving".Translate() + ": " + 2f.ToStringPercent());
			}
		}
		return num;
	}

	public static float GetForagedFoodCountPerInterval(List<Pawn> pawns, BiomeDef biome, Faction faction, StringBuilder explanation = null)
	{
		float num = ((biome.foragedFood != null) ? biome.forageability : 0f);
		explanation?.Append("ForagedNutritionPerDay".Translate() + ":");
		float num2 = 0f;
		bool flag = false;
		int i = 0;
		for (int count = pawns.Count; i < count; i++)
		{
			Pawn pawn = pawns[i];
			bool skip;
			float baseForagedNutritionPerDay = GetBaseForagedNutritionPerDay(pawn, out skip);
			if (!skip)
			{
				num2 += baseForagedNutritionPerDay;
				flag = true;
				if (explanation != null)
				{
					explanation.AppendLine();
					explanation.Append("  - " + pawn.LabelShortCap + ": +" + baseForagedNutritionPerDay.ToString("0.##"));
				}
			}
		}
		float num3 = num2;
		num2 /= 6f;
		if (explanation != null)
		{
			explanation.AppendLine();
			if (flag)
			{
				explanation.Append("  = " + num3.ToString("0.##"));
			}
			else
			{
				explanation.Append("  (" + "NoneCapable".Translate().ToLower() + ")");
			}
			explanation.AppendLine();
			explanation.AppendLine();
			explanation.Append("Biome".Translate() + ": x" + num.ToStringPercent() + " (" + biome.label + ")");
			if (faction.def.forageabilityFactor != 1f)
			{
				explanation.AppendLine();
				explanation.Append("  " + "FactionType".Translate() + ": " + faction.def.forageabilityFactor.ToStringPercent());
			}
		}
		num2 *= num;
		num2 *= faction.def.forageabilityFactor;
		if (biome.foragedFood != null)
		{
			return num2 / biome.foragedFood.ingestible.CachedNutrition;
		}
		return num2;
	}

	public static float GetBaseForagedNutritionPerDay(Pawn p, out bool skip)
	{
		if ((!p.IsAnimal && !p.IsFreeColonist) || p.InMentalState || p.Downed || p.CarriedByCaravan())
		{
			skip = true;
			return 0f;
		}
		skip = false;
		if (!StatDefOf.ForagedNutritionPerDay.Worker.IsDisabledFor(p))
		{
			return p.GetStatValue(StatDefOf.ForagedNutritionPerDay);
		}
		return 0f;
	}

	public static (ThingDef food, float perDay) ForagedFoodPerDay(Caravan caravan, StringBuilder explanation = null)
	{
		return ForagedFoodPerDay(caravan.PawnsListForReading, caravan.Biome, caravan.Faction, caravan.pather.MovingNow, caravan.NightResting, explanation);
	}

	public static float GetProgressPerTick(Caravan caravan, StringBuilder explanation = null)
	{
		return GetProgressPerTick(caravan.pather.MovingNow, caravan.NightResting, explanation);
	}

	public static float GetForagedFoodCountPerInterval(Caravan caravan, StringBuilder explanation = null)
	{
		return GetForagedFoodCountPerInterval(caravan.PawnsListForReading, caravan.Biome, caravan.Faction, explanation);
	}

	public static (ThingDef food, float perDay) ForagedFoodPerDay(List<TransferableOneWay> transferables, BiomeDef biome, Faction faction, StringBuilder explanation = null)
	{
		tmpPawns.Clear();
		for (int i = 0; i < transferables.Count; i++)
		{
			TransferableOneWay transferableOneWay = transferables[i];
			if (transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn)
			{
				for (int j = 0; j < transferableOneWay.CountToTransfer; j++)
				{
					tmpPawns.Add((Pawn)transferableOneWay.things[j]);
				}
			}
		}
		(ThingDef food, float perDay) result = ForagedFoodPerDay(tmpPawns, biome, faction, caravanMovingNow: true, caravanNightResting: false, explanation);
		tmpPawns.Clear();
		return result;
	}

	public static (ThingDef food, float perDay) ForagedFoodPerDayLeftAfterTransfer(List<TransferableOneWay> transferables, BiomeDef biome, Faction faction, StringBuilder explanation = null)
	{
		tmpPawns.Clear();
		for (int i = 0; i < transferables.Count; i++)
		{
			TransferableOneWay transferableOneWay = transferables[i];
			if (transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn)
			{
				for (int num = transferableOneWay.things.Count - 1; num >= transferableOneWay.CountToTransfer; num--)
				{
					tmpPawns.Add((Pawn)transferableOneWay.things[num]);
				}
			}
		}
		(ThingDef food, float perDay) result = ForagedFoodPerDay(tmpPawns, biome, faction, caravanMovingNow: true, caravanNightResting: false, explanation);
		tmpPawns.Clear();
		return result;
	}

	public static (ThingDef food, float perDay) ForagedFoodPerDayLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, BiomeDef biome, Faction faction, StringBuilder explanation = null)
	{
		tmpThingCounts.Clear();
		TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCounts);
		(ThingDef food, float perDay) result = ForagedFoodPerDay(tmpThingCounts, biome, faction, explanation);
		tmpThingCounts.Clear();
		return result;
	}

	public static (ThingDef food, float perDay) ForagedFoodPerDay(List<ThingCount> thingCounts, BiomeDef biome, Faction faction, StringBuilder explanation = null)
	{
		tmpPawns.Clear();
		for (int i = 0; i < thingCounts.Count; i++)
		{
			if (thingCounts[i].Count > 0 && thingCounts[i].Thing is Pawn item)
			{
				tmpPawns.Add(item);
			}
		}
		(ThingDef food, float perDay) result = ForagedFoodPerDay(tmpPawns, biome, faction, caravanMovingNow: true, caravanNightResting: false, explanation);
		tmpPawns.Clear();
		return result;
	}
}
