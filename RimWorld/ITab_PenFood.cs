using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_PenFood : ITab_PenBase
{
	private static readonly Vector2 WinSize = new Vector2(500f, 500f);

	private const int StatLineIndent = 8;

	private const int StatLabelColumnWidth = 210;

	private const float AboveTableMargin = 10f;

	private Vector2 animalPaneScrollPos;

	private readonly List<PenFoodCalculator.PenAnimalInfo> tmpAnimalInfos = new List<PenFoodCalculator.PenAnimalInfo>();

	public ITab_PenFood()
	{
		size = WinSize;
		labelKey = "TabPenFood";
	}

	public override void OnOpen()
	{
		base.OnOpen();
		animalPaneScrollPos = Vector2.zero;
	}

	protected override void FillTab()
	{
		CompAnimalPenMarker selectedCompAnimalPenMarker = base.SelectedCompAnimalPenMarker;
		Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
		if (selectedCompAnimalPenMarker.PenState.Unenclosed)
		{
			Widgets.NoneLabelCenteredVertically(rect, "(" + "PenFoodTab_NotEnclosed".Translate() + ")");
			return;
		}
		PenFoodCalculator penFoodCalculator = selectedCompAnimalPenMarker.PenFoodCalculator;
		Widgets.BeginGroup(rect);
		float curY = 0f;
		DrawTopPane(ref curY, rect.width, penFoodCalculator);
		float height = rect.height - curY;
		DrawAnimalPane(ref curY, rect.width, height, penFoodCalculator, selectedCompAnimalPenMarker.parent.Map);
		Widgets.EndGroup();
	}

	private void DrawTopPane(ref float curY, float width, PenFoodCalculator calc)
	{
		float num = calc.SumNutritionConsumptionPerDay - calc.NutritionPerDayToday;
		bool flag = num > 0f;
		DrawStatLine("PenSizeLabel".Translate(), calc.PenSizeDescription(), ref curY, width);
		DrawStatLine("PenFoodTab_NaturalNutritionGrowthRate".Translate(), PenFoodCalculator.NutritionPerDayToString(calc.NutritionPerDayToday), ref curY, width, calc.NaturalGrowthRateTooltip, flag ? new Color?(Color.red) : ((Color?)null));
		DrawStatLine("PenFoodTab_TotalNutritionConsumptionRate".Translate(), PenFoodCalculator.NutritionPerDayToString(calc.SumNutritionConsumptionPerDay), ref curY, width, calc.TotalConsumedToolTop, flag ? new Color?(Color.red) : ((Color?)null));
		if (!(calc.sumStockpiledNutritionAvailableNow > 0f))
		{
			return;
		}
		DrawStatLine("PenFoodTab_StockpileTotal".Translate(), PenFoodCalculator.NutritionToString(calc.sumStockpiledNutritionAvailableNow, withUnits: false), ref curY, width, calc.StockpileToolTip);
		if (flag)
		{
			int num2 = Mathf.FloorToInt(calc.sumStockpiledNutritionAvailableNow / num);
			DrawStatLine("PenFoodTab_StockpileEmptyDays".Translate(), num2.ToString(), ref curY, width, () => "PenFoodTab_StockpileEmptyDaysDescription".Translate(), Color.red);
		}
	}

	private void DrawStatLine(string label, string value, ref float curY, float width, Func<string> toolipGetter = null, Color? valueColor = null)
	{
		float lineHeight = Text.LineHeight;
		Rect rect = new Rect(8f, curY, width, lineHeight);
		rect.SplitVertically(210f, out var left, out var right);
		Widgets.Label(left, label);
		GUI.color = valueColor ?? Color.white;
		Widgets.Label(right, value);
		GUI.color = Color.white;
		if (Mouse.IsOver(rect) && toolipGetter != null)
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, toolipGetter, Gen.HashCombineInt(10192384, label.GetHashCode()));
		}
		curY += lineHeight;
	}

	private void DrawAnimalPane(ref float curYOuter, float width, float height, PenFoodCalculator calc, Map map)
	{
		float cellWidth = width - 328f;
		float curY = curYOuter;
		float num = curY;
		float num2 = Mathf.Max(Text.LineHeight, 27f);
		float num3 = Text.LineHeightOf(GameFont.Small) + 10f;
		float num4 = num2;
		Rect rect = new Rect(0f, curY, width, height - (curY - num) - num4);
		rect.SplitHorizontally(num3, out var top, out var bottom);
		float x = top.x;
		curY = top.y;
		DrawIconCell(null, ref x, 53f, num3);
		DrawCell("PenFoodTab_AnimalType".Translate(), ref x, cellWidth, num3, TextAnchor.LowerLeft);
		DrawCell("PenFoodTab_Count".Translate(), ref x, 100f, num3, TextAnchor.LowerCenter);
		DrawCell("PenFoodTab_NutritionConsumedPerDay_ColumLabel".Translate(), ref x, 120f, num3, TextAnchor.LowerCenter, null, () => "PenFoodTab_NutritionConsumedPerDay_ColumnTooltip".Translate());
		GUI.color = Widgets.SeparatorLineColor;
		Widgets.DrawLineHorizontal(0f, top.yMax - 1f, width);
		GUI.color = Color.white;
		tmpAnimalInfos.Clear();
		tmpAnimalInfos.AddRange(calc.ActualAnimalInfos);
		tmpAnimalInfos.AddRange(calc.ComputeExampleAnimals(base.SelectedCompAnimalPenMarker.ForceDisplayedAnimalDefs));
		Rect viewRect = new Rect(bottom.x, bottom.y, bottom.width - 16f, (float)tmpAnimalInfos.Count * num2);
		Widgets.BeginScrollView(bottom, ref animalPaneScrollPos, viewRect);
		curY = viewRect.y;
		int num5 = 0;
		foreach (PenFoodCalculator.PenAnimalInfo info in tmpAnimalInfos)
		{
			float x2 = viewRect.x;
			Rect rect2 = new Rect(x2, curY, viewRect.width, num2);
			if (num5 % 2 == 1)
			{
				Widgets.DrawLightHighlight(rect2);
			}
			DrawIconCell(info.animalDef, ref x2, 53f, num2);
			DrawCell(info.animalDef.LabelCap, ref x2, cellWidth, num2, TextAnchor.MiddleLeft);
			if (!info.example)
			{
				DrawCell(info.TotalCount.ToString(), ref x2, 100f, num2, TextAnchor.MiddleCenter);
				DrawCell(PenFoodCalculator.NutritionPerDayToString(info.TotalNutritionConsumptionPerDay, withUnits: false), ref x2, 120f, num2, TextAnchor.MiddleCenter);
			}
			else
			{
				float num6 = SimplifiedPastureNutritionSimulator.NutritionConsumedPerDay(info.animalDef);
				int num7 = Mathf.FloorToInt(calc.NutritionPerDayToday / num6);
				DrawCell("max".Translate() + " " + num7.ToString(), ref x2, 100f, num2, TextAnchor.MiddleCenter, Color.grey);
				DrawCell("PenFoodTab_NutritionConsumedEachAnimalLabel".Translate(PenFoodCalculator.NutritionPerDayToString(num6, withUnits: false).Named("CONSUMEDAMOUNT")), ref x2, 120f, num2, TextAnchor.MiddleCenter, Color.grey);
				DrawExampleAnimalControls(info, ref x2, 27f, num2);
			}
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, () => info.ToolTip(calc), 9477435);
			}
			curY += rect2.height;
			num5++;
		}
		Widgets.EndScrollView();
		Rect rect3 = new Rect(rect.x, Mathf.Min(rect.yMax, curY), rect.width, num4);
		Widgets.Dropdown(rect3.LeftPart(0.35f), calc, (PenFoodCalculator calculator) => (ThingDef)null, MenuGenerator, "PenFoodTab_AddAnimal".Translate());
		curY = rect3.yMax;
		curYOuter = curY;
		void DrawCell(string label, ref float reference, float num8, float cellHeight, TextAnchor anchor = TextAnchor.UpperLeft, Color? color = null, Func<string> tooltip = null)
		{
			if (label != null)
			{
				Rect rect4 = new Rect(reference, curY, num8, cellHeight);
				Text.Anchor = anchor;
				if (color.HasValue)
				{
					GUI.color = color.Value;
				}
				Widgets.Label(rect4, label);
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				if (tooltip != null && Mouse.IsOver(rect4))
				{
					Widgets.DrawHighlight(rect4);
					TooltipHandler.TipRegion(rect4, tooltip, 7578334);
				}
			}
			reference += num8 + 4f;
		}
		void DrawExampleAnimalControls(PenFoodCalculator.PenAnimalInfo info2, ref float reference, float num8, float cellHeight)
		{
			if (Widgets.ButtonImage(new Rect(reference, curY, num8, cellHeight), TexButton.Delete, Color.white, GenUI.SubtleMouseoverColor))
			{
				RemoveAnimal(calc, info2);
			}
			reference += num8 + 4f;
		}
		void DrawIconCell(ThingDef thingDef, ref float reference, float num8, float cellHeight)
		{
			if (thingDef != null)
			{
				Rect rect4 = new Rect(reference, curY, 27f, 27f);
				Widgets.ThingIcon(rect4, thingDef);
				rect4.x += 29f;
				Widgets.InfoCardButton(rect4.x, rect4.y + 2f, thingDef);
			}
			reference += num8 + 4f;
		}
		IEnumerable<Widgets.DropdownMenuElement<ThingDef>> MenuGenerator(PenFoodCalculator calculator)
		{
			foreach (ThingDef animal in map.plantGrowthRateCalculator.GrazingAnimals)
			{
				if (!base.SelectedCompAnimalPenMarker.ForceDisplayedAnimalDefs.Contains(animal))
				{
					yield return new Widgets.DropdownMenuElement<ThingDef>
					{
						option = new FloatMenuOption(animal.LabelCap, delegate
						{
							AddExampleAnimal(calculator, animal);
						}, animal),
						payload = animal
					};
				}
			}
		}
	}

	private void RemoveAnimal(PenFoodCalculator calc, PenFoodCalculator.PenAnimalInfo info)
	{
		base.SelectedCompAnimalPenMarker.RemoveForceDisplayedAnimal(info.animalDef);
	}

	private void AddExampleAnimal(PenFoodCalculator calc, ThingDef animal)
	{
		base.SelectedCompAnimalPenMarker.AddForceDisplayedAnimal(animal);
	}
}
