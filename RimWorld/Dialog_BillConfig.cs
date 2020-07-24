using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_BillConfig : Window
	{
		private IntVec3 billGiverPos;

		private Bill_Production bill;

		private Vector2 thingFilterScrollPosition;

		private string repeatCountEditBuffer;

		private string targetCountEditBuffer;

		private string unpauseCountEditBuffer;

		[TweakValue("Interface", 0f, 400f)]
		private static int RepeatModeSubdialogHeight = 324;

		[TweakValue("Interface", 0f, 400f)]
		private static int StoreModeSubdialogHeight = 30;

		[TweakValue("Interface", 0f, 400f)]
		private static int WorkerSelectionSubdialogHeight = 85;

		[TweakValue("Interface", 0f, 400f)]
		private static int IngredientRadiusSubdialogHeight = 50;

		public override Vector2 InitialSize => new Vector2(800f, 634f);

		public Dialog_BillConfig(Bill_Production bill, IntVec3 billGiverPos)
		{
			this.billGiverPos = billGiverPos;
			this.bill = bill;
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
		}

		private void AdjustCount(int offset)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			bill.repeatCount += offset;
			if (bill.repeatCount < 1)
			{
				bill.repeatCount = 1;
			}
		}

		public override void WindowUpdate()
		{
			bill.TryDrawIngredientSearchRadiusOnMap(billGiverPos);
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, 400f, 50f), bill.LabelCap);
			float width = (int)((inRect.width - 34f) / 3f);
			Rect rect = new Rect(0f, 80f, width, inRect.height - 80f);
			Rect rect2 = new Rect(rect.xMax + 17f, 50f, width, inRect.height - 50f - CloseButSize.y);
			Rect rect3 = new Rect(rect2.xMax + 17f, 50f, 0f, inRect.height - 50f - CloseButSize.y);
			rect3.xMax = inRect.xMax;
			Text.Font = GameFont.Small;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect2);
			Listing_Standard listing_Standard2 = listing_Standard.BeginSection(RepeatModeSubdialogHeight);
			if (listing_Standard2.ButtonText(bill.repeatMode.LabelCap))
			{
				BillRepeatModeUtility.MakeConfigFloatMenu(bill);
			}
			listing_Standard2.Gap();
			if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				listing_Standard2.Label("RepeatCount".Translate(bill.repeatCount));
				listing_Standard2.IntEntry(ref bill.repeatCount, ref repeatCountEditBuffer);
			}
			else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				string arg = "CurrentlyHave".Translate() + ": ";
				arg += bill.recipe.WorkerCounter.CountProducts(bill);
				arg += " / ";
				arg += ((bill.targetCount < 999999) ? bill.targetCount.ToString() : "Infinite".Translate().ToLower().ToString());
				string str = bill.recipe.WorkerCounter.ProductsDescription(bill);
				if (!str.NullOrEmpty())
				{
					arg += "\n" + "CountingProducts".Translate() + ": " + str.CapitalizeFirst();
				}
				listing_Standard2.Label(arg);
				int targetCount = bill.targetCount;
				listing_Standard2.IntEntry(ref bill.targetCount, ref targetCountEditBuffer, bill.recipe.targetCountAdjustment);
				bill.unpauseWhenYouHave = Mathf.Max(0, bill.unpauseWhenYouHave + (bill.targetCount - targetCount));
				ThingDef producedThingDef = bill.recipe.ProducedThingDef;
				if (producedThingDef != null)
				{
					if (producedThingDef.IsWeapon || producedThingDef.IsApparel)
					{
						listing_Standard2.CheckboxLabeled("IncludeEquipped".Translate(), ref bill.includeEquipped);
					}
					if (producedThingDef.IsApparel && producedThingDef.apparel.careIfWornByCorpse)
					{
						listing_Standard2.CheckboxLabeled("IncludeTainted".Translate(), ref bill.includeTainted);
					}
					Widgets.Dropdown(listing_Standard2.GetRect(30f), bill, (Bill_Production b) => b.includeFromZone, (Bill_Production b) => GenerateStockpileInclusion(), (bill.includeFromZone == null) ? "IncludeFromAll".Translate() : "IncludeSpecific".Translate(bill.includeFromZone.label));
					if (bill.recipe.products.Any((ThingDefCountClass prod) => prod.thingDef.useHitPoints))
					{
						Widgets.FloatRange(listing_Standard2.GetRect(28f), 975643279, ref bill.hpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
						bill.hpRange.min = Mathf.Round(bill.hpRange.min * 100f) / 100f;
						bill.hpRange.max = Mathf.Round(bill.hpRange.max * 100f) / 100f;
					}
					if (producedThingDef.HasComp(typeof(CompQuality)))
					{
						Widgets.QualityRange(listing_Standard2.GetRect(28f), 1098906561, ref bill.qualityRange);
					}
					if (producedThingDef.MadeFromStuff)
					{
						listing_Standard2.CheckboxLabeled("LimitToAllowedStuff".Translate(), ref bill.limitToAllowedStuff);
					}
				}
			}
			if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				listing_Standard2.CheckboxLabeled("PauseWhenSatisfied".Translate(), ref bill.pauseWhenSatisfied);
				if (bill.pauseWhenSatisfied)
				{
					listing_Standard2.Label("UnpauseWhenYouHave".Translate() + ": " + bill.unpauseWhenYouHave.ToString("F0"));
					listing_Standard2.IntEntry(ref bill.unpauseWhenYouHave, ref unpauseCountEditBuffer, bill.recipe.targetCountAdjustment);
					if (bill.unpauseWhenYouHave >= bill.targetCount)
					{
						bill.unpauseWhenYouHave = bill.targetCount - 1;
						unpauseCountEditBuffer = bill.unpauseWhenYouHave.ToStringCached();
					}
				}
			}
			listing_Standard.EndSection(listing_Standard2);
			listing_Standard.Gap();
			Listing_Standard listing_Standard3 = listing_Standard.BeginSection(StoreModeSubdialogHeight);
			string text = string.Format(bill.GetStoreMode().LabelCap, (bill.GetStoreZone() != null) ? bill.GetStoreZone().SlotYielderLabel() : "");
			if (bill.GetStoreZone() != null && !bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone()))
			{
				text += string.Format(" ({0})", "IncompatibleLower".Translate());
				Text.Font = GameFont.Tiny;
			}
			if (listing_Standard3.ButtonText(text))
			{
				Text.Font = GameFont.Small;
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (BillStoreModeDef item in DefDatabase<BillStoreModeDef>.AllDefs.OrderBy((BillStoreModeDef bsm) => bsm.listOrder))
				{
					if (item == BillStoreModeDefOf.SpecificStockpile)
					{
						List<SlotGroup> allGroupsListInPriorityOrder = bill.billStack.billGiver.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
						int count = allGroupsListInPriorityOrder.Count;
						for (int i = 0; i < count; i++)
						{
							SlotGroup group = allGroupsListInPriorityOrder[i];
							Zone_Stockpile zone_Stockpile = group.parent as Zone_Stockpile;
							if (zone_Stockpile == null)
							{
								continue;
							}
							if (!bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, zone_Stockpile))
							{
								list.Add(new FloatMenuOption(string.Format("{0} ({1})", string.Format(item.LabelCap, group.parent.SlotYielderLabel()), "IncompatibleLower".Translate()), null));
								continue;
							}
							list.Add(new FloatMenuOption(string.Format(item.LabelCap, group.parent.SlotYielderLabel()), delegate
							{
								bill.SetStoreMode(BillStoreModeDefOf.SpecificStockpile, (Zone_Stockpile)group.parent);
							}));
						}
					}
					else
					{
						BillStoreModeDef smLocal = item;
						list.Add(new FloatMenuOption(smLocal.LabelCap, delegate
						{
							bill.SetStoreMode(smLocal);
						}));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Text.Font = GameFont.Small;
			listing_Standard.EndSection(listing_Standard3);
			listing_Standard.Gap();
			Listing_Standard listing_Standard4 = listing_Standard.BeginSection(WorkerSelectionSubdialogHeight);
			Widgets.Dropdown(listing_Standard4.GetRect(30f), bill, (Bill_Production b) => b.pawnRestriction, (Bill_Production b) => GeneratePawnRestrictionOptions(), (bill.pawnRestriction == null) ? "AnyWorker".TranslateSimple() : bill.pawnRestriction.LabelShortCap);
			if (bill.pawnRestriction == null && bill.recipe.workSkill != null)
			{
				listing_Standard4.Label("AllowedSkillRange".Translate(bill.recipe.workSkill.label));
				listing_Standard4.IntRange(ref bill.allowedSkillRange, 0, 20);
			}
			listing_Standard.EndSection(listing_Standard4);
			listing_Standard.End();
			Rect rect4 = rect3;
			bool flag = true;
			for (int j = 0; j < bill.recipe.ingredients.Count; j++)
			{
				if (!bill.recipe.ingredients[j].IsFixedIngredient)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				rect4.yMin = rect4.yMax - (float)IngredientRadiusSubdialogHeight;
				rect3.yMax = rect4.yMin - 17f;
				bool num = bill.GetStoreZone() == null || bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone());
				ThingFilterUI.DoThingFilterConfigWindow(rect3, ref thingFilterScrollPosition, bill.ingredientFilter, bill.recipe.fixedIngredientFilter, 4, null, bill.recipe.forceHiddenSpecialFilters, forceHideHitPointsConfig: false, bill.recipe.GetPremultipliedSmallIngredients(), bill.Map);
				bool flag2 = bill.GetStoreZone() == null || bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone());
				if (num && !flag2)
				{
					Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(bill.LabelCap, bill.billStack.billGiver.LabelShort.CapitalizeFirst(), bill.GetStoreZone().label), bill.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			else
			{
				rect4.yMin = 50f;
			}
			Listing_Standard listing_Standard5 = new Listing_Standard();
			listing_Standard5.Begin(rect4);
			string str2 = "IngredientSearchRadius".Translate().Truncate(rect4.width * 0.6f);
			string str3 = (bill.ingredientSearchRadius == 999f) ? "Unlimited".TranslateSimple().Truncate(rect4.width * 0.3f) : bill.ingredientSearchRadius.ToString("F0");
			listing_Standard5.Label(str2 + ": " + str3);
			bill.ingredientSearchRadius = listing_Standard5.Slider((bill.ingredientSearchRadius > 100f) ? 100f : bill.ingredientSearchRadius, 3f, 100f);
			if (bill.ingredientSearchRadius >= 100f)
			{
				bill.ingredientSearchRadius = 999f;
			}
			listing_Standard5.End();
			Listing_Standard listing_Standard6 = new Listing_Standard();
			listing_Standard6.Begin(rect);
			if (bill.suspended)
			{
				if (listing_Standard6.ButtonText("Suspended".Translate()))
				{
					bill.suspended = false;
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
			}
			else if (listing_Standard6.ButtonText("NotSuspended".Translate()))
			{
				bill.suspended = true;
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (bill.recipe.description != null)
			{
				stringBuilder.AppendLine(bill.recipe.description);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("WorkAmount".Translate() + ": " + bill.recipe.WorkAmountTotal(null).ToStringWorkAmount());
			for (int k = 0; k < bill.recipe.ingredients.Count; k++)
			{
				IngredientCount ingredientCount = bill.recipe.ingredients[k];
				if (!ingredientCount.filter.Summary.NullOrEmpty())
				{
					stringBuilder.AppendLine(bill.recipe.IngredientValueGetter.BillRequirementsDescription(bill.recipe, ingredientCount));
				}
			}
			stringBuilder.AppendLine();
			string text2 = bill.recipe.IngredientValueGetter.ExtraDescriptionLine(bill.recipe);
			if (text2 != null)
			{
				stringBuilder.AppendLine(text2);
				stringBuilder.AppendLine();
			}
			if (!bill.recipe.skillRequirements.NullOrEmpty())
			{
				stringBuilder.AppendLine("MinimumSkills".Translate());
				stringBuilder.AppendLine(bill.recipe.MinSkillString);
			}
			Text.Font = GameFont.Small;
			string text3 = stringBuilder.ToString();
			if (Text.CalcHeight(text3, rect.width) > rect.height)
			{
				Text.Font = GameFont.Tiny;
			}
			listing_Standard6.Label(text3);
			Text.Font = GameFont.Small;
			listing_Standard6.End();
			if (bill.recipe.products.Count == 1)
			{
				ThingDef thingDef = bill.recipe.products[0].thingDef;
				Widgets.InfoCardButton(rect.x, rect3.y, thingDef, GenStuff.DefaultStuffFor(thingDef));
			}
		}

		private IEnumerable<Widgets.DropdownMenuElement<Pawn>> GeneratePawnRestrictionOptions()
		{
			_003C_003Ec__DisplayClass16_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass16_0();
			CS_0024_003C_003E8__locals0._003C_003E4__this = this;
			Widgets.DropdownMenuElement<Pawn> dropdownMenuElement = new Widgets.DropdownMenuElement<Pawn>
			{
				option = new FloatMenuOption("AnyWorker".Translate(), delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.bill.pawnRestriction = null;
				}),
				payload = null
			};
			yield return dropdownMenuElement;
			SkillDef workSkill = bill.recipe.workSkill;
			IEnumerable<Pawn> allMaps_FreeColonists = PawnsFinder.AllMaps_FreeColonists;
			allMaps_FreeColonists = allMaps_FreeColonists.OrderBy((Pawn pawn) => pawn.LabelShortCap);
			if (workSkill != null)
			{
				allMaps_FreeColonists = allMaps_FreeColonists.OrderByDescending((Pawn pawn) => pawn.skills.GetSkill(CS_0024_003C_003E8__locals0._003C_003E4__this.bill.recipe.workSkill).Level);
			}
			CS_0024_003C_003E8__locals0.workGiver = bill.billStack.billGiver.GetWorkgiver();
			if (CS_0024_003C_003E8__locals0.workGiver == null)
			{
				Log.ErrorOnce("Generating pawn restrictions for a BillGiver without a Workgiver", 96455148);
				yield break;
			}
			allMaps_FreeColonists = allMaps_FreeColonists.OrderByDescending((Pawn pawn) => pawn.workSettings.WorkIsActive(CS_0024_003C_003E8__locals0.workGiver.workType));
			allMaps_FreeColonists = allMaps_FreeColonists.OrderBy((Pawn pawn) => pawn.WorkTypeIsDisabled(CS_0024_003C_003E8__locals0.workGiver.workType));
			using (IEnumerator<Pawn> enumerator = allMaps_FreeColonists.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003C_003Ec__DisplayClass16_0 _003C_003Ec__DisplayClass16_ = CS_0024_003C_003E8__locals0;
					Pawn pawn2 = enumerator.Current;
					if (pawn2.WorkTypeIsDisabled(_003C_003Ec__DisplayClass16_.workGiver.workType))
					{
						dropdownMenuElement = new Widgets.DropdownMenuElement<Pawn>
						{
							option = new FloatMenuOption(string.Format("{0} ({1})", pawn2.LabelShortCap, "WillNever".Translate(_003C_003Ec__DisplayClass16_.workGiver.verb)), null),
							payload = pawn2
						};
						yield return dropdownMenuElement;
					}
					else if (bill.recipe.workSkill != null && !pawn2.workSettings.WorkIsActive(_003C_003Ec__DisplayClass16_.workGiver.workType))
					{
						dropdownMenuElement = new Widgets.DropdownMenuElement<Pawn>
						{
							option = new FloatMenuOption(string.Format("{0} ({1} {2}, {3})", pawn2.LabelShortCap, pawn2.skills.GetSkill(bill.recipe.workSkill).Level, bill.recipe.workSkill.label, "NotAssigned".Translate()), delegate
							{
								_003C_003Ec__DisplayClass16_._003C_003E4__this.bill.pawnRestriction = pawn2;
							}),
							payload = pawn2
						};
						yield return dropdownMenuElement;
					}
					else if (!pawn2.workSettings.WorkIsActive(_003C_003Ec__DisplayClass16_.workGiver.workType))
					{
						dropdownMenuElement = new Widgets.DropdownMenuElement<Pawn>
						{
							option = new FloatMenuOption(string.Format("{0} ({1})", pawn2.LabelShortCap, "NotAssigned".Translate()), delegate
							{
								_003C_003Ec__DisplayClass16_._003C_003E4__this.bill.pawnRestriction = pawn2;
							}),
							payload = pawn2
						};
						yield return dropdownMenuElement;
					}
					else if (bill.recipe.workSkill != null)
					{
						dropdownMenuElement = new Widgets.DropdownMenuElement<Pawn>
						{
							option = new FloatMenuOption($"{pawn2.LabelShortCap} ({pawn2.skills.GetSkill(bill.recipe.workSkill).Level} {bill.recipe.workSkill.label})", delegate
							{
								_003C_003Ec__DisplayClass16_._003C_003E4__this.bill.pawnRestriction = pawn2;
							}),
							payload = pawn2
						};
						yield return dropdownMenuElement;
					}
					else
					{
						dropdownMenuElement = new Widgets.DropdownMenuElement<Pawn>
						{
							option = new FloatMenuOption($"{pawn2.LabelShortCap}", delegate
							{
								_003C_003Ec__DisplayClass16_._003C_003E4__this.bill.pawnRestriction = pawn2;
							}),
							payload = pawn2
						};
						yield return dropdownMenuElement;
					}
				}
			}
		}

		private IEnumerable<Widgets.DropdownMenuElement<Zone_Stockpile>> GenerateStockpileInclusion()
		{
			Widgets.DropdownMenuElement<Zone_Stockpile> dropdownMenuElement = new Widgets.DropdownMenuElement<Zone_Stockpile>
			{
				option = new FloatMenuOption("IncludeFromAll".Translate(), delegate
				{
					bill.includeFromZone = null;
				}),
				payload = null
			};
			yield return dropdownMenuElement;
			List<SlotGroup> groupList = bill.billStack.billGiver.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
			int groupCount = groupList.Count;
			int i = 0;
			while (i < groupCount)
			{
				Dialog_BillConfig dialog_BillConfig = this;
				SlotGroup slotGroup = groupList[i];
				Zone_Stockpile stockpile = slotGroup.parent as Zone_Stockpile;
				if (stockpile != null)
				{
					if (!bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, stockpile))
					{
						dropdownMenuElement = new Widgets.DropdownMenuElement<Zone_Stockpile>
						{
							option = new FloatMenuOption(string.Format("{0} ({1})", "IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel()), "IncompatibleLower".Translate()), null),
							payload = stockpile
						};
						yield return dropdownMenuElement;
					}
					else
					{
						dropdownMenuElement = new Widgets.DropdownMenuElement<Zone_Stockpile>
						{
							option = new FloatMenuOption("IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel()), delegate
							{
								dialog_BillConfig.bill.includeFromZone = stockpile;
							}),
							payload = stockpile
						};
						yield return dropdownMenuElement;
					}
				}
				int num = i + 1;
				i = num;
			}
		}
	}
}
