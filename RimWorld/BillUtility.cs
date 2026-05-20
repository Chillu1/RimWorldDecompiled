using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class BillUtility
{
	public static Bill Clipboard = null;

	private static readonly RecipeTooltipLayout recipeTooltipLayout = new RecipeTooltipLayout();

	private const int MaxIngredientIcons = 9;

	private const float Indent = 8f;

	private const float ElementSpacing = 4f;

	private const float Margin = 6f;

	private static HashSet<ThingDef> tmpSpecialProducts = new HashSet<ThingDef>();

	private static List<IngredientCount> tmpSortedIngredients = new List<IngredientCount>();

	public static void TryDrawIngredientSearchRadiusOnMap(this Bill bill, IntVec3 center)
	{
		if (bill.ingredientSearchRadius < GenRadial.MaxRadialPatternRadius)
		{
			GenDraw.DrawRadiusRing(center, bill.ingredientSearchRadius);
		}
	}

	public static Bill MakeNewBill(this RecipeDef recipe, Precept_ThingStyle precept = null)
	{
		if (recipe.UsesUnfinishedThing)
		{
			return new Bill_ProductionWithUft(recipe, precept);
		}
		if (recipe.mechResurrection)
		{
			return new Bill_ResurrectMech(recipe, precept);
		}
		if (recipe.gestationCycles > 0)
		{
			return new Bill_ProductionMech(recipe, precept);
		}
		if (recipe.formingTicks > 0)
		{
			return new Bill_Autonomous(recipe, precept);
		}
		return new Bill_Production(recipe, precept);
	}

	public static IEnumerable<IBillGiver> GlobalBillGivers()
	{
		foreach (Map map in Find.Maps)
		{
			foreach (IBillGiver item in MapBillGivers(map))
			{
				yield return item;
			}
		}
		foreach (Caravan caravan in Find.WorldObjects.Caravans)
		{
			foreach (Thing allThing in caravan.AllThings)
			{
				if (allThing.GetInnerIfMinified() is IBillGiver billGiver)
				{
					yield return billGiver;
				}
			}
		}
	}

	public static IEnumerable<IBillGiver> MapBillGivers(Map map)
	{
		foreach (Thing item in map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)))
		{
			if (!(item is IBillGiver billGiver))
			{
				Log.ErrorOnce("Found non-bill-giver tagged as PotentialBillGiver", 13389774);
			}
			else
			{
				yield return billGiver;
			}
		}
		foreach (Thing item2 in map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.MinifiedThing)))
		{
			if (item2.GetInnerIfMinified() is IBillGiver billGiver2)
			{
				yield return billGiver2;
			}
		}
	}

	public static IEnumerable<Bill> GlobalBills()
	{
		foreach (IBillGiver item in GlobalBillGivers())
		{
			foreach (Bill item2 in item.BillStack)
			{
				yield return item2;
			}
		}
		if (Clipboard != null)
		{
			yield return Clipboard;
		}
	}

	public static IEnumerable<Bill> MapBills(Map map)
	{
		foreach (IBillGiver item in MapBillGivers(map))
		{
			foreach (Bill item2 in item.BillStack)
			{
				yield return item2;
			}
		}
	}

	public static void Notify_ISlotGroupRemoved(ISlotGroup group)
	{
		if (GravshipUtility.generatingGravship)
		{
			return;
		}
		foreach (Bill item in GlobalBills())
		{
			item.ValidateSettings();
		}
	}

	public static void Notify_ColonistUnavailable(Pawn pawn)
	{
		try
		{
			foreach (Bill item in GlobalBills())
			{
				item.ValidateSettings();
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not notify bills: " + ex);
		}
	}

	public static WorkGiverDef GetWorkgiver(this IBillGiver billGiver)
	{
		if (!(billGiver is Thing thing))
		{
			Log.ErrorOnce($"Attempting to get the workgiver for a non-Thing IBillGiver {billGiver.ToString()}", 96810282);
			return null;
		}
		List<WorkGiverDef> allDefsListForReading = DefDatabase<WorkGiverDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			WorkGiverDef workGiverDef = allDefsListForReading[i];
			if (workGiverDef.Worker is WorkGiver_DoBill workGiver_DoBill && workGiver_DoBill.ThingIsUsableBillGiver(thing))
			{
				return workGiverDef;
			}
		}
		Log.ErrorOnce($"Can't find a WorkGiver for a BillGiver {thing.ToString()}", 57348705);
		return null;
	}

	public static bool IsSurgeryViolationOnExtraFactionMember(this Bill_Medical bill, Pawn billDoer)
	{
		if (bill.recipe.IsSurgery && bill.recipe.Worker != null)
		{
			RecipeWorker worker = bill.recipe.Worker;
			Faction sharedExtraFaction = billDoer.GetSharedExtraFaction(bill.GiverPawn, ExtraFactionType.HomeFaction);
			if (sharedExtraFaction != null && worker.IsViolationOnPawn(bill.GiverPawn, bill.Part, sharedExtraFaction))
			{
				return true;
			}
			Faction sharedExtraFaction2 = billDoer.GetSharedExtraFaction(bill.GiverPawn, ExtraFactionType.MiniFaction);
			if (sharedExtraFaction2 != null && worker.IsViolationOnPawn(bill.GiverPawn, bill.Part, sharedExtraFaction2))
			{
				return true;
			}
		}
		return false;
	}

	public static void MakeIngredientsListInProcessingOrder(this Bill bill, List<IngredientCount> ingredientsOrdered)
	{
		ingredientsOrdered.Clear();
		if (bill.recipe.productHasIngredientStuff)
		{
			ingredientsOrdered.Add(bill.recipe.ingredients[0]);
		}
		for (int i = 0; i < bill.recipe.ingredients.Count; i++)
		{
			if (!bill.recipe.productHasIngredientStuff || i != 0)
			{
				IngredientCount ingredientCount = bill.recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient)
				{
					ingredientsOrdered.Add(ingredientCount);
				}
			}
		}
		for (int j = 0; j < bill.recipe.ingredients.Count; j++)
		{
			IngredientCount item = bill.recipe.ingredients[j];
			if (!ingredientsOrdered.Contains(item))
			{
				ingredientsOrdered.Add(item);
			}
		}
	}

	public static bool BillGiverCanContainIngredients(Bill bill)
	{
		return GetBillGiverContainer(bill) != null;
	}

	public static ThingOwner<Thing> GetBillGiverContainer(Bill bill)
	{
		if (bill.billStack.billGiver is Building_WorkTableAutonomous building_WorkTableAutonomous)
		{
			return (ThingOwner<Thing>)building_WorkTableAutonomous.innerContainer;
		}
		return null;
	}

	public static bool ContainedInBillGiver(Bill bill, Thing thing)
	{
		return GetBillGiverContainer(bill)?.Contains(thing) ?? false;
	}

	public static void DoBillInfoWindow(int billIndex, string label, Rect rect, RecipeDef recipe, BodyPartRecord part = null, Pawn pawn = null)
	{
		LayoutTooltip(recipe, part, pawn, draw: false);
		if (!recipeTooltipLayout.Empty)
		{
			Rect windowRect = Find.WindowStack.currentlyDrawnWindow.windowRect;
			Rect immRect = new Rect(windowRect.x + rect.xMax + 10f, windowRect.y + rect.y, recipeTooltipLayout.Size.x, recipeTooltipLayout.Size.y);
			immRect.x = Mathf.Min(immRect.x, (float)UI.screenWidth - immRect.width);
			immRect.y = Mathf.Min(immRect.y, (float)UI.screenHeight - immRect.height);
			Find.WindowStack.ImmediateWindow(123 * (billIndex + 1), immRect, WindowLayer.Super, delegate
			{
				GameFont font = Text.Font;
				Text.Font = GameFont.Small;
				GUI.BeginGroup(immRect.AtZero());
				LayoutTooltip(recipe, part, pawn, draw: true);
				GUI.EndGroup();
				Text.Font = font;
			});
		}
	}

	private static void LayoutTooltip(RecipeDef recipe, BodyPartRecord part, Pawn pawn, bool draw)
	{
		recipeTooltipLayout.Reset(6f);
		tmpSpecialProducts.Clear();
		tmpSortedIngredients.Clear();
		if (!recipe.skillRequirements.NullOrEmpty())
		{
			recipeTooltipLayout.Label(("Requires".Translate() + ": ").AsTipTitle() + recipe.skillRequirements.Select((SkillRequirement x) => $"{x.skill.LabelCap} {x.minLevel}").ToCommaList(), draw);
		}
		if (!recipe.ingredients.NullOrEmpty())
		{
			bool flag = false;
			tmpSortedIngredients.AddRange(recipe.ingredients);
			tmpSortedIngredients.Sort((IngredientCount a, IngredientCount b) => Mathf.RoundToInt(b.CountFor(recipe) - a.CountFor(recipe)));
			foreach (IngredientCount ing in tmpSortedIngredients)
			{
				if (ing.filter?.AllowedThingDefs == null)
				{
					continue;
				}
				IEnumerable<ThingDef> enumerable = ing.filter.AllowedThingDefs.Where((ThingDef x) => Widgets.GetIconFor(x) != BaseContent.BadTex && ((ing.IsFixedIngredient && ing.filter.Allows(x)) || recipe.fixedIngredientFilter.AllowedDefCount == 0 || (recipe.fixedIngredientFilter.Allows(x) && !recipe.fixedIngredientFilter.IsAlwaysDisallowedDueToSpecialFilters(x))));
				GetDistinctProductsFromIngredients(recipe, enumerable, tmpSpecialProducts);
				IEnumerable<TextureAndColor> enumerable2 = enumerable.Select((ThingDef x) => x.ToTextureAndColor()).Distinct();
				if (!enumerable2.Any())
				{
					continue;
				}
				if (!flag)
				{
					if (!recipeTooltipLayout.Empty)
					{
						recipeTooltipLayout.Newline();
					}
					recipeTooltipLayout.Label(("Ingredients".Translate() + ": ").AsTipTitle(), draw);
					flag = true;
				}
				recipeTooltipLayout.Newline();
				ThingDef singleIngredient = null;
				if (enumerable.EnumerableCount() == 1)
				{
					singleIngredient = enumerable.First();
				}
				DisplayIngredientIconRow(enumerable2, draw, ing.CountFor(recipe), singleIngredient);
			}
		}
		if (!recipe.products.NullOrEmpty() || tmpSpecialProducts.Count > 0)
		{
			if (!recipeTooltipLayout.Empty)
			{
				recipeTooltipLayout.Newline();
			}
			recipeTooltipLayout.Label(("Products".Translate() + ": ").AsTipTitle(), draw);
			recipeTooltipLayout.Newline();
			if (!recipe.products.NullOrEmpty())
			{
				foreach (ThingDefCountClass product in recipe.products)
				{
					DisplayIngredientIconRow(Gen.YieldSingle(product.thingDef.ToTextureAndColor()), draw, product.count);
				}
				if (tmpSpecialProducts.Count > 0)
				{
					recipeTooltipLayout.Newline();
				}
			}
			if (tmpSpecialProducts.Count > 0)
			{
				DisplayIngredientIconRow(tmpSpecialProducts.Select((ThingDef x) => x.ToTextureAndColor()).Distinct(), draw);
			}
		}
		if (recipe.addsHediff != null && part != null && pawn != null)
		{
			AddedBodyPartProps addedPartProps = recipe.addsHediff.addedPartProps;
			if (addedPartProps != null && addedPartProps.solid)
			{
				List<Hediff> list = new List<Hediff>();
				List<Hediff_MissingPart> list2 = new List<Hediff_MissingPart>();
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				foreach (BodyPartRecord childPart in part.GetPartAndAllChildParts())
				{
					if (pawn.health.hediffSet.TryGetDirectlyAddedPartFor(childPart, out var hediff))
					{
						list.Add(hediff);
						continue;
					}
					Hediff hediff2 = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.Part == childPart && x is Hediff_Implant);
					if (hediff2 != null)
					{
						list.Add(hediff2);
						continue;
					}
					Hediff_MissingPart hediff_MissingPart = missingPartsCommonAncestors.Find((Hediff_MissingPart h) => h.Part == childPart);
					if (hediff_MissingPart != null)
					{
						list2.Add(hediff_MissingPart);
					}
				}
				if (list.Count > 0)
				{
					if (!recipeTooltipLayout.Empty)
					{
						recipeTooltipLayout.Newline();
					}
					recipeTooltipLayout.Label(("Replaces".Translate() + ": ").Colorize(ColoredText.WarningColor), draw);
					recipeTooltipLayout.Label(list.Select((Hediff p) => p.Label).ToCommaList().CapitalizeFirst()
						.Colorize(ColoredText.WarningColor), draw);
				}
				if (list2.Count > 0)
				{
					if (!recipeTooltipLayout.Empty)
					{
						recipeTooltipLayout.Newline();
					}
					recipeTooltipLayout.Label(("Replaces".Translate() + ": ").AsTipTitle(), draw);
					recipeTooltipLayout.Label(list2.Select((Hediff_MissingPart p) => "MissingPartWithLabel".Translate(p.Part.Label).ToString()).ToCommaList().CapitalizeFirst(), draw);
				}
			}
		}
		if (!recipeTooltipLayout.Empty)
		{
			recipeTooltipLayout.Expand(6f, 6f);
		}
	}

	private static void GetDistinctProductsFromIngredients(RecipeDef recipe, IEnumerable<ThingDef> ingredients, HashSet<ThingDef> outProducts)
	{
		if (recipe.specialProducts.NullOrEmpty())
		{
			return;
		}
		foreach (SpecialProductType specialProduct in recipe.specialProducts)
		{
			foreach (ThingDef ingredient in ingredients)
			{
				ThingDef thingDef = ingredient;
				if (thingDef.IsCorpse && thingDef.ingestible?.sourceDef != null)
				{
					thingDef = thingDef.ingestible.sourceDef;
				}
				switch (specialProduct)
				{
				case SpecialProductType.Butchery:
					if (!thingDef.butcherProducts.NullOrEmpty())
					{
						foreach (ThingDefCountClass butcherProduct in thingDef.butcherProducts)
						{
							outProducts.Add(butcherProduct.thingDef);
						}
					}
					if (thingDef.race?.meatDef != null)
					{
						outProducts.Add(thingDef.race.meatDef);
					}
					if (thingDef.race?.leatherDef != null)
					{
						outProducts.Add(thingDef.race.leatherDef);
					}
					break;
				case SpecialProductType.Smelted:
					if (!thingDef.CostList.NullOrEmpty())
					{
						foreach (ThingDefCountClass cost in thingDef.CostList)
						{
							if (!cost.thingDef.intricate && Mathf.Round((float)cost.count * 0.25f) > 0f)
							{
								outProducts.Add(cost.thingDef);
							}
						}
					}
					if (thingDef.smeltProducts.NullOrEmpty())
					{
						break;
					}
					foreach (ThingDefCountClass smeltProduct in thingDef.smeltProducts)
					{
						outProducts.Add(smeltProduct.thingDef);
					}
					break;
				}
			}
		}
	}

	private static void DisplayIngredientIconRow(IEnumerable<TextureAndColor> icons, bool draw, float? count = null, ThingDef singleIngredient = null)
	{
		int num = icons.EnumerableCount();
		if (num > 0)
		{
			int num2 = Mathf.Min(9, num);
			recipeTooltipLayout.Gap(8f, 0f);
			if (count.HasValue)
			{
				recipeTooltipLayout.Label(count.Value + "x ", draw);
			}
			for (int i = 0; i < num2; i++)
			{
				TextureAndColor textureAndColor = icons.ElementAt(i);
				recipeTooltipLayout.Icon(textureAndColor.Texture, textureAndColor.Color, Text.LineHeightOf(GameFont.Small), draw);
				recipeTooltipLayout.Gap(4f, 0f);
			}
			if (num > 9)
			{
				Text.Anchor = TextAnchor.MiddleLeft;
				recipeTooltipLayout.Label(" " + "Etc".Translate() + "...", draw);
				Text.Anchor = TextAnchor.UpperLeft;
			}
			else if (singleIngredient != null)
			{
				recipeTooltipLayout.Label(singleIngredient.LabelCap, draw);
			}
		}
	}

	private static TextureAndColor ToTextureAndColor(this ThingDef td)
	{
		return new TextureAndColor(Widgets.GetIconFor(td), td.uiIconColor);
	}
}
