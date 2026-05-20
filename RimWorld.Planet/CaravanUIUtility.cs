using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class CaravanUIUtility
{
	private enum TransferableCategory
	{
		Pawn,
		Item,
		TravelSupplies
	}

	public struct CaravanInfo
	{
		public float massUsage;

		public float massCapacity;

		public string massCapacityExplanation;

		public float tilesPerDay;

		public string tilesPerDayExplanation;

		public (float days, float tillRot) daysWorthOfFood;

		public (ThingDef food, float perDay) foragedFoodPerDay;

		public string foragedFoodPerDayExplanation;

		public float visibility;

		public string visibilityExplanation;

		public float extraMassUsage;

		public float extraMassCapacity;

		public string extraMassCapacityExplanation;

		public CaravanInfo(float massUsage, float massCapacity, string massCapacityExplanation, float tilesPerDay, string tilesPerDayExplanation, (float days, float tillRot) daysWorthOfFood, (ThingDef food, float perDay) foragedFoodPerDay, string foragedFoodPerDayExplanation, float visibility, string visibilityExplanation, float extraMassUsage = -1f, float extraMassCapacity = -1f, string extraMassCapacityExplanation = null)
		{
			this.massUsage = massUsage;
			this.massCapacity = massCapacity;
			this.massCapacityExplanation = massCapacityExplanation;
			this.tilesPerDay = tilesPerDay;
			this.tilesPerDayExplanation = tilesPerDayExplanation;
			this.daysWorthOfFood = daysWorthOfFood;
			this.foragedFoodPerDay = foragedFoodPerDay;
			this.foragedFoodPerDayExplanation = foragedFoodPerDayExplanation;
			this.visibility = visibility;
			this.visibilityExplanation = visibilityExplanation;
			this.extraMassUsage = extraMassUsage;
			this.extraMassCapacity = extraMassCapacity;
			this.extraMassCapacityExplanation = extraMassCapacityExplanation;
		}
	}

	private static readonly List<Pair<float, Color>> MassColor = new List<Pair<float, Color>>
	{
		new Pair<float, Color>(0.37f, Color.green),
		new Pair<float, Color>(0.82f, Color.yellow),
		new Pair<float, Color>(1f, new Color(1f, 0.6f, 0f))
	};

	private static readonly List<Pair<float, Color>> TilesPerDayColor = new List<Pair<float, Color>>
	{
		new Pair<float, Color>(0f, Color.white),
		new Pair<float, Color>(0.001f, ColorLibrary.RedReadable),
		new Pair<float, Color>(1f, Color.yellow),
		new Pair<float, Color>(2f, Color.white)
	};

	private static readonly List<Pair<float, Color>> DaysWorthOfFoodColor = new List<Pair<float, Color>>
	{
		new Pair<float, Color>(1f, Color.red),
		new Pair<float, Color>(2f, Color.white)
	};

	private static readonly List<Pair<float, Color>> DaysWorthOfFoodKnownRouteColor = new List<Pair<float, Color>>
	{
		new Pair<float, Color>(0.3f, ColorLibrary.RedReadable),
		new Pair<float, Color>(0.9f, Color.yellow),
		new Pair<float, Color>(1.02f, Color.green)
	};

	private static readonly List<Pair<float, Color>> VisibilityColor = new List<Pair<float, Color>>
	{
		new Pair<float, Color>(0f, Color.white),
		new Pair<float, Color>(0.01f, Color.green),
		new Pair<float, Color>(0.2f, Color.green),
		new Pair<float, Color>(1f, Color.white),
		new Pair<float, Color>(1.2f, ColorLibrary.RedReadable)
	};

	private static readonly List<TransferableUIUtility.ExtraInfo> tmpInfo = new List<TransferableUIUtility.ExtraInfo>();

	public static void CreateCaravanTransferableWidgets(List<TransferableOneWay> transferables, out TransferableOneWayWidget pawnsTransfer, out TransferableOneWayWidget itemsTransfer, out TransferableOneWayWidget travelSuppliesTransfer, string thingCountTip, IgnorePawnsInventoryMode ignorePawnInventoryMass, Func<float> availableMassGetter, bool ignoreSpawnedCorpsesGearAndInventoryMass, PlanetTile tile, bool playerPawnsReadOnly = false)
	{
		pawnsTransfer = new TransferableOneWayWidget(null, null, null, thingCountTip, drawMass: true, ignorePawnInventoryMass, includePawnsMassInMassUsage: false, availableMassGetter, 0f, ignoreSpawnedCorpsesGearAndInventoryMass, tile, drawMarketValue: true, drawEquippedWeapon: true, drawNutritionEatenPerDay: true, drawMechEnergy: true, drawItemNutrition: false, drawForagedFoodPerDay: true, drawDaysUntilRot: false, playerPawnsReadOnly);
		AddPawnsSections(pawnsTransfer, transferables);
		itemsTransfer = new TransferableOneWayWidget(transferables.Where((TransferableOneWay x) => GetTransferableCategory(x) == TransferableCategory.Item), null, null, thingCountTip, drawMass: true, ignorePawnInventoryMass, includePawnsMassInMassUsage: false, availableMassGetter, 0f, ignoreSpawnedCorpsesGearAndInventoryMass, tile, drawMarketValue: true, drawEquippedWeapon: false, drawNutritionEatenPerDay: false, drawMechEnergy: false, drawItemNutrition: true, drawForagedFoodPerDay: false, drawDaysUntilRot: true);
		travelSuppliesTransfer = new TransferableOneWayWidget(transferables.Where((TransferableOneWay x) => GetTransferableCategory(x) == TransferableCategory.TravelSupplies), null, null, thingCountTip, drawMass: true, ignorePawnInventoryMass, includePawnsMassInMassUsage: false, availableMassGetter, 0f, ignoreSpawnedCorpsesGearAndInventoryMass, tile, drawMarketValue: true, drawEquippedWeapon: false, drawNutritionEatenPerDay: false, drawMechEnergy: false, drawItemNutrition: true, drawForagedFoodPerDay: false, drawDaysUntilRot: true);
	}

	private static TransferableCategory GetTransferableCategory(Transferable t)
	{
		if (t.ThingDef.category == ThingCategory.Pawn)
		{
			return TransferableCategory.Pawn;
		}
		if ((!t.ThingDef.thingCategories.NullOrEmpty() && t.ThingDef.thingCategories.Contains(ThingCategoryDefOf.Medicine)) || (t.ThingDef.IsIngestible && !t.ThingDef.IsDrug && !t.ThingDef.IsCorpse && (t.ThingDef.plant == null || !t.ThingDef.plant.IsTree)) || (t.AnyThing.GetInnerIfMinified().def.IsBed && t.AnyThing.GetInnerIfMinified().def.building.bed_caravansCanUse))
		{
			return TransferableCategory.TravelSupplies;
		}
		return TransferableCategory.Item;
	}

	public static void AddPawnsSections(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
	{
		IEnumerable<TransferableOneWay> source = transferables.Where((TransferableOneWay x) => x.ThingDef.category == ThingCategory.Pawn);
		widget.AddSection("ColonistsSection".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).IsFreeNonSlaveColonist));
		if (ModsConfig.IdeologyActive)
		{
			widget.AddSection("SlavesSection".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).IsSlave));
		}
		widget.AddSection("PrisonersSection".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).IsPrisoner));
		widget.AddSection("CaptureSection".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).Downed && CaravanUtility.ShouldAutoCapture((Pawn)x.AnyThing, Faction.OfPlayer)));
		widget.AddSection("AnimalsSection".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).IsAnimal));
		if (ModsConfig.BiotechActive)
		{
			widget.AddSection("MechsSection".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).IsColonyMech && ((Pawn)x.AnyThing).OverseerSubject != null && ((Pawn)x.AnyThing).OverseerSubject.State == OverseerSubjectState.Overseen));
		}
		if (ModsConfig.AnomalyActive)
		{
			widget.AddSection("EntitiesSection".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).IsColonySubhuman && ((Pawn)x.AnyThing).mutant.Def.canTravelInCaravan));
		}
	}

	private static string GetDaysWorthOfFoodLabel((float days, float tillRot) daysWorthOfFood, bool multiline)
	{
		if (daysWorthOfFood.days >= 600f)
		{
			return "InfiniteDaysWorthOfFoodInfo".Translate();
		}
		string text = daysWorthOfFood.days.ToString("0.#");
		string text2 = (multiline ? "\n" : " ");
		if (daysWorthOfFood.tillRot < 600f && daysWorthOfFood.tillRot < daysWorthOfFood.days)
		{
			text += text2 + "(" + "DaysWorthOfFoodInfoRot".Translate($"{daysWorthOfFood.tillRot:0.#})");
		}
		return text;
	}

	private static Color GetDaysWorthOfFoodColor((float days, float tillRot) daysWorthOfFood, int? ticksToArrive)
	{
		if (daysWorthOfFood.days >= 600f)
		{
			return Color.white;
		}
		float num = Mathf.Min(daysWorthOfFood.days, daysWorthOfFood.tillRot);
		if (ticksToArrive.HasValue)
		{
			return GenUI.LerpColor(DaysWorthOfFoodKnownRouteColor, num / ((float)ticksToArrive.Value / 60000f));
		}
		return GenUI.LerpColor(DaysWorthOfFoodColor, num);
	}

	private static string GetMovementSpeedLabel(float tilesPerDay, bool immobile, bool shortVersion)
	{
		if (immobile)
		{
			return shortVersion ? "TilesPerDayImmobileShort".Translate() : "TilesPerDayImmobile".Translate();
		}
		return $"{tilesPerDay:0.#} " + "TilesPerDay".Translate();
	}

	private static Color GetMovementSpeedColor(float tilesPerDay, bool immobile)
	{
		if (!immobile)
		{
			return GenUI.LerpColor(TilesPerDayColor, tilesPerDay);
		}
		return Color.red;
	}

	public static void DrawCaravanInfo(CaravanInfo info, CaravanInfo? info2, PlanetTile currentTile, int? ticksToArrive, float lastMassFlashTime, Rect rect, bool lerpMassColor = true, string extraDaysWorthOfFoodTipInfo = null, bool multiline = false, bool isCaravan = true)
	{
		tmpInfo.Clear();
		isCaravan = isCaravan && currentTile.Valid && currentTile.LayerDef.SurfaceTiles;
		TaggedString taggedString = $"{info.massUsage.ToStringEnsureThreshold(info.massCapacity, 0)} / {info.massCapacity:F0} " + "kg".Translate();
		TaggedString taggedString2 = (info2.HasValue ? ($"{info2.Value.massUsage.ToStringEnsureThreshold(info2.Value.massCapacity, 0)} / {info2.Value.massCapacity:F0} " + "kg".Translate()) : ((TaggedString)null));
		tmpInfo.Add(new TransferableUIUtility.ExtraInfo("Mass".Translate(), taggedString, GetMassColor(info.massUsage, info.massCapacity, lerpMassColor), GetMassTip(info.massUsage, info.massCapacity, info.massCapacityExplanation, info2?.massUsage, info2?.massCapacity, info2?.massCapacityExplanation, isCaravan), taggedString2, info2.HasValue ? GetMassColor(info2.Value.massUsage, info2.Value.massCapacity, lerpMassColor) : Color.white, lastMassFlashTime));
		if (!Mathf.Approximately(info.extraMassUsage, -1f) && isCaravan)
		{
			TaggedString taggedString3 = $"{info.extraMassUsage.ToStringEnsureThreshold(info.extraMassCapacity, 0)} / {info.extraMassCapacity:F0} " + "kg".Translate();
			TaggedString taggedString4 = (info2.HasValue ? ($"{info2.Value.extraMassUsage.ToStringEnsureThreshold(info2.Value.extraMassCapacity, 0)} / {info2.Value.extraMassCapacity:F0} " + "kg".Translate()) : ((TaggedString)null));
			tmpInfo.Add(new TransferableUIUtility.ExtraInfo("CaravanMass".Translate(), taggedString3, GetMassColor(info.extraMassUsage, info.extraMassCapacity, lerpMassColor: true), GetMassTip(info.extraMassUsage, info.extraMassCapacity, info.extraMassCapacityExplanation, info2?.extraMassUsage, info2?.extraMassCapacity, info2?.extraMassCapacityExplanation, isCaravan), taggedString4, info2.HasValue ? GetMassColor(info2.Value.extraMassUsage, info2.Value.extraMassCapacity, lerpMassColor: true) : Color.white));
		}
		if (isCaravan)
		{
			string text = "CaravanMovementSpeedTip".Translate();
			if (!info.tilesPerDayExplanation.NullOrEmpty())
			{
				text = text + "\n\n" + info.tilesPerDayExplanation;
			}
			if (info2.HasValue && !info2.Value.tilesPerDayExplanation.NullOrEmpty())
			{
				text = text + "\n\n-----\n\n" + info2.Value.tilesPerDayExplanation;
			}
			tmpInfo.Add(new TransferableUIUtility.ExtraInfo("CaravanMovementSpeed".Translate(), GetMovementSpeedLabel(info.tilesPerDay, info.massUsage > info.massCapacity, info2.HasValue), GetMovementSpeedColor(info.tilesPerDay, info.massUsage > info.massCapacity), text, info2.HasValue ? GetMovementSpeedLabel(info2.Value.tilesPerDay, info2.Value.massUsage > info2.Value.massCapacity, shortVersion: true) : null, info2.HasValue ? GetMovementSpeedColor(info2.Value.tilesPerDay, info2.Value.massUsage > info2.Value.massCapacity) : Color.white));
		}
		tmpInfo.Add(new TransferableUIUtility.ExtraInfo("DaysWorthOfFood".Translate(), GetDaysWorthOfFoodLabel(info.daysWorthOfFood, multiline), GetDaysWorthOfFoodColor(info.daysWorthOfFood, ticksToArrive), "DaysWorthOfFoodTooltip".Translate() + extraDaysWorthOfFoodTipInfo + "\n\n" + VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(currentTile, Find.TickManager.TicksAbs), info2.HasValue ? GetDaysWorthOfFoodLabel(info2.Value.daysWorthOfFood, multiline) : null, info2.HasValue ? GetDaysWorthOfFoodColor(info2.Value.daysWorthOfFood, ticksToArrive) : Color.white));
		string text2 = info.foragedFoodPerDay.perDay.ToString("0.#");
		string text3 = info2?.foragedFoodPerDay.perDay.ToString("0.#");
		TaggedString taggedString5 = "ForagedFoodPerDayTip".Translate();
		taggedString5 += "\n\n" + info.foragedFoodPerDayExplanation;
		if (info2.HasValue)
		{
			taggedString5 += "\n\n-----\n\n" + info2.Value.foragedFoodPerDayExplanation;
		}
		if (info.foragedFoodPerDay.perDay > 0f || (info2.HasValue && info2.Value.foragedFoodPerDay.perDay > 0f))
		{
			string text4 = (multiline ? "\n" : " ");
			if (!info2.HasValue)
			{
				text2 = text2 + text4 + "(" + info.foragedFoodPerDay.food.label + ")";
			}
			else
			{
				text3 = text3 + text4 + "(" + info2.Value.foragedFoodPerDay.food.label.Truncate(50f) + ")";
			}
		}
		if (isCaravan)
		{
			tmpInfo.Add(new TransferableUIUtility.ExtraInfo("ForagedFoodPerDay".Translate(), text2, Color.white, taggedString5, text3, Color.white));
			string text5 = "CaravanVisibilityTip".Translate();
			if (!info.visibilityExplanation.NullOrEmpty())
			{
				text5 = text5 + "\n\n" + info.visibilityExplanation;
			}
			if (info2.HasValue && !info2.Value.visibilityExplanation.NullOrEmpty())
			{
				text5 = text5 + "\n\n-----\n\n" + info2.Value.visibilityExplanation;
			}
			tmpInfo.Add(new TransferableUIUtility.ExtraInfo("Visibility".Translate(), info.visibility.ToStringPercent(), GenUI.LerpColor(VisibilityColor, info.visibility), text5, info2?.visibility.ToStringPercent(), info2.HasValue ? GenUI.LerpColor(VisibilityColor, info2.Value.visibility) : Color.white));
		}
		TransferableUIUtility.DrawExtraInfo(tmpInfo, rect);
	}

	private static Color GetMassColor(float massUsage, float massCapacity, bool lerpMassColor)
	{
		if (massCapacity == 0f)
		{
			return Color.white;
		}
		if (massUsage > massCapacity)
		{
			return Color.red;
		}
		if (lerpMassColor)
		{
			return GenUI.LerpColor(MassColor, massUsage / massCapacity);
		}
		return Color.white;
	}

	private static string GetMassTip(float massUsage, float massCapacity, string massCapacityExplanation, float? massUsage2, float? massCapacity2, string massCapacity2Explanation, bool isCaravan)
	{
		TaggedString taggedString = "MassCarriedSimple".Translate() + ": " + massUsage.ToStringEnsureThreshold(massCapacity, 2) + " " + "kg".Translate() + "\n" + "MassCapacity".Translate() + ": " + massCapacity.ToString("F2") + " " + "kg".Translate();
		if (massUsage2.HasValue)
		{
			taggedString += "\n <-> \n" + "MassCarriedSimple".Translate() + ": " + massUsage2.Value.ToStringEnsureThreshold(massCapacity2.Value, 2) + " " + "kg".Translate() + "\n" + "MassCapacity".Translate() + ": " + massCapacity2.Value.ToString("F2") + " " + "kg".Translate();
		}
		if (isCaravan)
		{
			taggedString += "\n\n" + "CaravanMassUsageTooltip".Translate();
		}
		if (!massCapacityExplanation.NullOrEmpty())
		{
			taggedString += "\n\n" + "MassCapacity".Translate() + ":" + "\n" + massCapacityExplanation;
		}
		if (!massCapacity2Explanation.NullOrEmpty())
		{
			taggedString += "\n\n-----\n\n" + "MassCapacity".Translate() + ":" + "\n" + massCapacity2Explanation;
		}
		return taggedString;
	}
}
