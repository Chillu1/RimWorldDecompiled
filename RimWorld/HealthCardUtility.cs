using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class HealthCardUtility
{
	private static Vector2 scrollPosition = Vector2.zero;

	private static float scrollViewHeight = 0f;

	private static bool highlight = true;

	private static bool onOperationTab = false;

	private static Vector2 billsScrollPosition = Vector2.zero;

	private static float billsScrollHeight = 1000f;

	private static bool showAllHediffs = false;

	private static bool showHediffsDebugInfo = false;

	private static float lastMaxIconsTotalWidth;

	private const float TopPadding = 20f;

	private const float ThoughtLevelHeight = 25f;

	private const float ThoughtLevelSpacing = 4f;

	private const float IconSize = 20f;

	private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private static readonly Color StaticHighlightColor = new Color(0.75f, 0.75f, 0.85f, 1f);

	private static readonly Color MediumPainColor = new Color(0.9f, 0.9f, 0f);

	private static readonly Color SeverePainColor = new Color(0.9f, 0.5f, 0f);

	private static readonly Texture2D BleedingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Bleeding");

	private const int HideBloodLossTicksThreshold = 60000;

	private static readonly Dictionary<EfficiencyEstimate, Color> efficiencyToColor = new Dictionary<EfficiencyEstimate, Color>
	{
		{
			EfficiencyEstimate.None,
			ColorLibrary.RedReadable
		},
		{
			EfficiencyEstimate.VeryPoor,
			new Color(0.75f, 0.45f, 0.45f)
		},
		{
			EfficiencyEstimate.Poor,
			new Color(0.55f, 0.55f, 0.55f)
		},
		{
			EfficiencyEstimate.Weakened,
			new Color(0.7f, 0.7f, 0.7f)
		},
		{
			EfficiencyEstimate.GoodCondition,
			HealthUtility.GoodConditionColor
		},
		{
			EfficiencyEstimate.Enhanced,
			new Color(0.5f, 0.5f, 0.9f)
		}
	};

	private static List<Hediff> tmpHediffImpactors = new List<Hediff>();

	private static List<BodyPartRecord> tmpBodyPartImpactors = new List<BodyPartRecord>();

	private static List<Gene> tmpGeneImpactors = new List<Gene>();

	public static void DrawPawnHealthCard(Rect outRect, Pawn pawn, bool allowOperations, bool showBloodLoss, Thing thingForMedBills)
	{
		if (pawn.Dead && allowOperations)
		{
			Log.Error("Called DrawPawnHealthCard with a dead pawn and allowOperations=true. Operations are disallowed on corpses.");
			allowOperations = false;
		}
		outRect.y += 20f;
		outRect.height -= 20f;
		outRect = outRect.Rounded();
		Rect rect = new Rect(outRect.x, outRect.y, outRect.width * 0.375f, outRect.height).Rounded();
		Rect rect2 = new Rect(rect.xMax, outRect.y, outRect.width - rect.width, outRect.height);
		rect.yMin += 11f;
		DrawHealthSummary(rect, pawn, allowOperations, thingForMedBills);
		DrawHediffListing(rect2.ContractedBy(10f), pawn, showBloodLoss, 0f, Prefs.DevMode && Current.ProgramState == ProgramState.Playing);
	}

	public static void DrawHealthSummary(Rect rect, Pawn pawn, bool allowOperations, Thing thingForMedBills)
	{
		GUI.color = Color.white;
		if (!allowOperations)
		{
			onOperationTab = false;
		}
		Widgets.DrawMenuSection(rect);
		List<TabRecord> list = new List<TabRecord>
		{
			new TabRecord("HealthOverview".Translate(), delegate
			{
				onOperationTab = false;
			}, !onOperationTab)
		};
		if (allowOperations)
		{
			string text = (pawn.RaceProps.IsMechanoid ? "MedicalOperationsMechanoidsShort".Translate() : "MedicalOperationsShort".Translate());
			if (pawn.BillStack.Count > 0)
			{
				text = text + " (" + pawn.BillStack.Count + ")";
			}
			list.Add(new TabRecord(text, delegate
			{
				onOperationTab = true;
			}, onOperationTab));
		}
		TabDrawer.DrawTabs(rect, list);
		rect = rect.ContractedBy(9f);
		Widgets.BeginGroup(rect);
		float curY = 0f;
		Text.Font = GameFont.Medium;
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperCenter;
		if (onOperationTab)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MedicalOperations, KnowledgeAmount.FrameDisplayed);
			curY = DrawMedOperationsTab(rect, pawn, thingForMedBills, curY);
		}
		else
		{
			curY = DrawOverviewTab(rect, pawn, curY);
		}
		Text.Font = GameFont.Small;
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.EndGroup();
	}

	public static bool AnyHediffsDisplayed(Pawn pawn, bool showBloodLoss)
	{
		return VisibleHediffGroupsInOrder(pawn, showBloodLoss).Any();
	}

	public static void DrawHediffListing(Rect rect, Pawn pawn, bool showBloodLoss, float rowLeftPad = 0f, bool showDebugOptions = false)
	{
		GUI.color = Color.white;
		if (showDebugOptions)
		{
			DoDebugOptions(rect, pawn);
		}
		Widgets.BeginGroup(rect);
		float lineHeight = Text.LineHeight;
		Rect outRect = new Rect(0f, 0f, rect.width, rect.height - lineHeight);
		Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
		Rect rect2 = rect;
		if (viewRect.height > outRect.height)
		{
			rect2.width -= 16f;
		}
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		GUI.color = Color.white;
		float curY = 0f;
		highlight = true;
		bool flag = false;
		lastMaxIconsTotalWidth = 0f;
		foreach (IGrouping<BodyPartRecord, Hediff> item in VisibleHediffGroupsInOrder(pawn, showBloodLoss))
		{
			flag = true;
			DrawHediffRow(rect2, pawn, item, ref curY, rowLeftPad);
		}
		if (!flag)
		{
			Widgets.NoneLabelCenteredVertically(new Rect(0f, 0f, viewRect.width, outRect.height), "(" + "NoHealthConditions".Translate() + ")");
			curY = outRect.height - 1f;
		}
		if (Event.current.type == EventType.Repaint)
		{
			scrollViewHeight = curY;
		}
		else if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = Mathf.Max(scrollViewHeight, curY);
		}
		Widgets.EndScrollView();
		float bleedRateTotal = pawn.health.hediffSet.BleedRateTotal;
		if (bleedRateTotal > 0.01f)
		{
			Rect rect3 = new Rect(0f, rect.height - lineHeight, rect.width, lineHeight);
			string text = "BleedingRate".Translate() + ": " + bleedRateTotal.ToStringPercent() + "/" + "LetterDay".Translate();
			int num = HealthUtility.TicksUntilDeathDueToBloodLoss(pawn);
			text = ((ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Deathless)) ? ((string)(text + (" (" + "Deathless".Translate() + ")"))) : ((num >= 60000) ? ((string)(text + (" (" + "WontBleedOutSoon".Translate() + ")"))) : ((string)(text + (" (" + "TimeToDeath".Translate(num.ToStringTicksToPeriod()) + ")")))));
			Widgets.Label(rect3, text);
		}
		Widgets.EndGroup();
		GUI.color = Color.white;
	}

	private static IEnumerable<IGrouping<BodyPartRecord, Hediff>> VisibleHediffGroupsInOrder(Pawn pawn, bool showBloodLoss)
	{
		foreach (IGrouping<BodyPartRecord, Hediff> item in from x in VisibleHediffs(pawn, showBloodLoss)
			group x by x.Part into x
			orderby GetListPriority(x.First().Part) descending
			select x)
		{
			yield return item;
		}
	}

	private static float GetListPriority(BodyPartRecord rec)
	{
		if (rec == null)
		{
			return 9999999f;
		}
		return (float)((int)rec.height * 10000) + rec.coverageAbsWithChildren;
	}

	private static IEnumerable<Hediff> VisibleHediffs(Pawn pawn, bool showBloodLoss)
	{
		if (!showAllHediffs)
		{
			List<Hediff_MissingPart> mpca = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
			for (int i = 0; i < mpca.Count; i++)
			{
				yield return mpca[i];
			}
			IEnumerable<Hediff> enumerable = pawn.health.hediffSet.hediffs.Where(delegate(Hediff d)
			{
				if (d is Hediff_MissingPart)
				{
					return false;
				}
				if (!d.Visible)
				{
					return false;
				}
				return (showBloodLoss || d.def != HediffDefOf.BloodLoss) ? true : false;
			});
			foreach (Hediff item in enumerable)
			{
				yield return item;
			}
			yield break;
		}
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			yield return hediff;
		}
	}

	private static float DrawMedOperationsTab(Rect leftRect, Pawn pawn, Thing thingForMedBills, float curY)
	{
		curY += 2f;
		Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			int num = 0;
			foreach (RecipeDef allRecipe in thingForMedBills.def.AllRecipes)
			{
				if (allRecipe.AvailableNow)
				{
					AcceptanceReport report = allRecipe.Worker.AvailableReport(pawn);
					if (report.Accepted || !report.Reason.NullOrEmpty())
					{
						IEnumerable<ThingDef> enumerable = allRecipe.PotentiallyMissingIngredients(null, thingForMedBills.MapHeld);
						if (!enumerable.Any((ThingDef x) => x.isTechHediff) && !enumerable.Any((ThingDef x) => x.IsDrug) && (!enumerable.Any() || !allRecipe.dontShowIfAnyIngredientMissing))
						{
							if (allRecipe.targetsBodyPart)
							{
								foreach (BodyPartRecord item in allRecipe.Worker.GetPartsToApplyOn(pawn, allRecipe))
								{
									if (allRecipe.AvailableOnNow(pawn, item))
									{
										list.Add(GenerateSurgeryOption(pawn, thingForMedBills, allRecipe, enumerable, report, num, item));
										num++;
									}
								}
							}
							else if (!pawn.health.hediffSet.HasHediff(allRecipe.addsHediff))
							{
								list.Add(GenerateSurgeryOption(pawn, thingForMedBills, allRecipe, enumerable, report, num));
								num++;
							}
						}
					}
				}
			}
			return list;
		};
		Rect rect = new Rect(leftRect.x - 9f, curY, leftRect.width, leftRect.height - curY - 20f);
		((IBillGiver)thingForMedBills).BillStack.DoListing(rect, recipeOptionsMaker, ref billsScrollPosition, ref billsScrollHeight);
		return curY;
	}

	private static FloatMenuOption GenerateSurgeryOption(Pawn pawn, Thing thingForMedBills, RecipeDef recipe, IEnumerable<ThingDef> missingIngredients, AcceptanceReport report, int index, BodyPartRecord part = null)
	{
		string label = recipe.Worker.GetLabelWhenUsedOn(pawn, part).CapitalizeFirst();
		if (part != null && !recipe.hideBodyPartNames)
		{
			label = label + " (" + part.Label + ")";
		}
		FloatMenuOption floatMenuOption;
		if (!report.Reason.NullOrEmpty())
		{
			label = label + " (" + report.Reason + ")";
			floatMenuOption = new FloatMenuOption(label, null);
		}
		else if (missingIngredients.Any())
		{
			label += " (";
			bool flag = true;
			foreach (ThingDef missingIngredient in missingIngredients)
			{
				if (!flag)
				{
					label += ", ";
				}
				flag = false;
				label += "MissingMedicalBillIngredient".Translate(missingIngredient.label);
			}
			label += ")";
			floatMenuOption = new FloatMenuOption(label, null);
		}
		else
		{
			Action action = delegate
			{
				Pawn medPawn = thingForMedBills as Pawn;
				if (medPawn != null)
				{
					HediffDef hediffDef = recipe.addsHediff ?? recipe.changesHediffLevel;
					if (hediffDef != null)
					{
						TaggedString text = CompRoyalImplant.CheckForViolations(medPawn, hediffDef, recipe.hediffLevelOffset);
						if (!text.NullOrEmpty())
						{
							Find.WindowStack.Add(new Dialog_MessageBox(text, "Yes".Translate(), delegate
							{
								CreateSurgeryBill(medPawn, recipe, part);
							}, "No".Translate()));
						}
						else
						{
							TaggedString confirmation = recipe.Worker.GetConfirmation(medPawn);
							if (!confirmation.NullOrEmpty())
							{
								Find.WindowStack.Add(new Dialog_MessageBox(confirmation, "Yes".Translate(), delegate
								{
									CreateSurgeryBill(medPawn, recipe, part);
								}, "No".Translate()));
							}
							else
							{
								CreateSurgeryBill(medPawn, recipe, part);
							}
						}
					}
					else if (recipe.Worker is Recipe_ImplantXenogerm)
					{
						Find.WindowStack.Add(new Dialog_SelectXenogerm(pawn, pawn.Map, null, delegate(Xenogerm x)
						{
							x.SetTargetPawn(pawn);
						}));
					}
					else
					{
						TaggedString confirmation2 = recipe.Worker.GetConfirmation(medPawn);
						if (!confirmation2.NullOrEmpty())
						{
							Find.WindowStack.Add(new Dialog_MessageBox(confirmation2, "Yes".Translate(), delegate
							{
								CreateSurgeryBill(medPawn, recipe, part);
							}, "No".Translate()));
						}
						else
						{
							CreateSurgeryBill(medPawn, recipe, part);
						}
					}
				}
			};
			floatMenuOption = ((recipe.Worker is Recipe_AdministerIngestible) ? new FloatMenuOption(label, action, recipe.ingredients.FirstOrDefault()?.FixedIngredient) : ((!(recipe.Worker is Recipe_RemoveBodyPart)) ? new FloatMenuOption(label, action, recipe.UIIconThing, recipe.UIIcon) : new FloatMenuOption(label, action, part.def.spawnThingOnRemoved)));
		}
		floatMenuOption.extraPartWidth = 29f;
		floatMenuOption.extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe);
		floatMenuOption.mouseoverGuiAction = delegate(Rect rect)
		{
			BillUtility.DoBillInfoWindow(index, label, rect, recipe, part, pawn);
		};
		return floatMenuOption;
	}

	public static Bill_Medical CreateSurgeryBill(Pawn medPawn, RecipeDef recipe, BodyPartRecord part, List<Thing> uniqueIngredients = null, bool sendMessages = true)
	{
		Bill_Medical bill_Medical = new Bill_Medical(recipe, uniqueIngredients);
		medPawn.BillStack.AddBill(bill_Medical);
		bill_Medical.Part = part;
		if (recipe.conceptLearned != null)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
		}
		if (sendMessages)
		{
			Map mapHeld = medPawn.MapHeld;
			if (!(from p in mapHeld.mapPawns.PawnsInFaction(Faction.OfPlayer)
				where p.IsFreeColonist || p.IsColonyMechPlayerControlled
				select p).Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
			{
				Bill.CreateNoPawnsWithSkillDialog(recipe);
			}
			if (!medPawn.InBed() && medPawn.RaceProps.IsFlesh)
			{
				if (medPawn.RaceProps.Humanlike)
				{
					if (!mapHeld.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(medPawn, x.def) && ((Building_Bed)x).Medical))
					{
						Messages.Message("MessageNoMedicalBeds".Translate(), medPawn, MessageTypeDefOf.CautionInput, historical: false);
					}
				}
				else if (!mapHeld.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(medPawn, x.def)))
				{
					Messages.Message("MessageNoAnimalBeds".Translate(), medPawn, MessageTypeDefOf.CautionInput, historical: false);
				}
			}
			if (medPawn.Faction != null && !medPawn.Faction.Hidden && !medPawn.Faction.HostileTo(Faction.OfPlayer) && recipe.Worker.IsViolationOnPawn(medPawn, part, Faction.OfPlayer))
			{
				Messages.Message("MessageMedicalOperationWillAngerFaction".Translate(medPawn.HomeFaction), medPawn, MessageTypeDefOf.CautionInput, historical: false);
			}
			if (!CanDoRecipeWithMedicineRestriction(medPawn, recipe))
			{
				Messages.Message("MessageWarningNoMedicineForRestriction".Translate(medPawn.Named("PAWN"), medPawn.playerSettings.medCare.GetLabel().Named("RESTRICTIONLABEL")), medPawn, MessageTypeDefOf.CautionInput, historical: false);
			}
			recipe.Worker.CheckForWarnings(medPawn);
		}
		return bill_Medical;
	}

	private static float DrawOverviewTab(Rect rect, Pawn pawn, float curY)
	{
		curY += 4f;
		bool flag = false;
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = new Color(0.9f, 0.9f, 0.9f);
		if (pawn.foodRestriction != null && pawn.foodRestriction.Configurable && !pawn.DevelopmentalStage.Baby() && pawn.needs?.food != null && (!pawn.IsMutant || !pawn.mutant.Def.disablePolicies))
		{
			Rect rect2 = new Rect(0f, curY, rect.width, 23f);
			flag = true;
			TooltipHandler.TipRegionByKey(rect2, "FoodRestrictionDescription");
			Widgets.DrawHighlightIfMouseover(rect2);
			Rect rect3 = rect2;
			rect3.xMax = rect2.center.x - 4f;
			Rect rect4 = rect2;
			rect4.xMin = rect2.center.x + 4f;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect3, string.Format("{0}:", "AllowFood".Translate()));
			Text.Anchor = TextAnchor.UpperLeft;
			if (Widgets.ButtonText(rect4, pawn.foodRestriction.CurrentFoodPolicy.label))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (FoodPolicy restriction in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
				{
					list.Add(new FloatMenuOption(restriction.label, delegate
					{
						pawn.foodRestriction.CurrentFoodPolicy = restriction;
					}));
				}
				list.Add(new FloatMenuOption("ManageFoodPolicies".Translate() + "...", delegate
				{
					Find.WindowStack.Add(new Dialog_ManageFoodPolicies(pawn.foodRestriction.CurrentFoodPolicy));
				}));
				Find.WindowStack.Add(new FloatMenu(list));
			}
			curY += rect2.height + 4f;
		}
		bool flag2 = pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer;
		bool flag3 = pawn.NonHumanlikeOrWildMan() && pawn.InBed() && pawn.CurrentBed().Faction == Faction.OfPlayer;
		if (pawn.RaceProps.IsFlesh && (flag2 || flag3) && (!pawn.IsMutant || pawn.mutant.Def.entitledToMedicalCare) && pawn.playerSettings != null && !pawn.Dead && Current.ProgramState == ProgramState.Playing)
		{
			Rect rect5 = new Rect(0f, curY, rect.width, 23f);
			flag = true;
			TooltipHandler.TipRegionByKey(rect5, "MedicineQualityDescription");
			Widgets.DrawHighlightIfMouseover(rect5);
			Rect rect6 = rect5;
			rect6.xMax = rect5.center.x - 4f;
			Rect rect7 = rect5;
			rect7.xMin = rect5.center.x + 4f;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect6, string.Format("{0}:", "AllowMedicine".Translate()));
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.DrawButtonGraphic(rect7);
			MedicalCareUtility.MedicalCareSelectButton(rect7, pawn);
			curY += rect5.height + 4f;
		}
		if (Current.ProgramState == ProgramState.Playing && pawn.IsColonist && !pawn.Dead && !pawn.DevelopmentalStage.Baby() && pawn.playerSettings != null)
		{
			Rect rect8 = new Rect(0f, curY, rect.width, 23f);
			flag = true;
			TooltipHandler.TipRegion(rect8, "AllowSelfTendTip".Translate(Faction.OfPlayer.def.pawnsPlural, 0.7f.ToStringPercent()).CapitalizeFirst());
			Widgets.DrawHighlightIfMouseover(rect8);
			Rect rect9 = rect8;
			rect9.xMax = rect8.center.x - 4f;
			Rect rect10 = rect8;
			rect10.xMin = rect8.center.x + 4f;
			rect10.width = rect10.height;
			rect10.ContractedBy(4f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect9, string.Format("{0}:", "AllowSelfTend".Translate()));
			Text.Anchor = TextAnchor.UpperLeft;
			bool selfTend = pawn.playerSettings.selfTend;
			Widgets.Checkbox(rect10.x, rect10.y, ref pawn.playerSettings.selfTend, rect10.height);
			if (pawn.playerSettings.selfTend && !selfTend)
			{
				if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
				{
					pawn.playerSettings.selfTend = false;
					Messages.Message("MessageCannotSelfTendEver".Translate(pawn.LabelShort, pawn), MessageTypeDefOf.RejectInput, historical: false);
				}
				else if (pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor) == 0)
				{
					Messages.Message("MessageSelfTendUnsatisfied".Translate(pawn.LabelShort, pawn), MessageTypeDefOf.CautionInput, historical: false);
				}
			}
			curY += rect8.height + 10f;
		}
		if (flag)
		{
			Widgets.DrawLineHorizontal(rect.x - 8f, curY, rect.width, Color.gray);
		}
		curY += 10f;
		if (pawn.def.race.IsFlesh)
		{
			Pair<string, Color> painLabel = GetPainLabel(pawn);
			string painTip = GetPainTip(pawn);
			DrawLeftRow(rect, ref curY, "PainLevel".Translate(), painLabel.First, painLabel.Second, painTip);
		}
		curY += 6f;
		if (!pawn.Dead)
		{
			IEnumerable<PawnCapacityDef> source = (pawn.def.race.Humanlike ? DefDatabase<PawnCapacityDef>.AllDefs.Where((PawnCapacityDef x) => x.showOnHumanlikes) : (pawn.def.race.Animal ? DefDatabase<PawnCapacityDef>.AllDefs.Where((PawnCapacityDef x) => x.showOnAnimals) : (pawn.def.race.IsAnomalyEntity ? DefDatabase<PawnCapacityDef>.AllDefs.Where((PawnCapacityDef x) => x.showOnAnomalyEntities) : ((!pawn.def.race.IsDrone) ? DefDatabase<PawnCapacityDef>.AllDefs.Where((PawnCapacityDef x) => x.showOnMechanoids) : DefDatabase<PawnCapacityDef>.AllDefs.Where((PawnCapacityDef x) => x.showOnDrones)))));
			foreach (PawnCapacityDef item in source.OrderBy((PawnCapacityDef act) => act.listOrder))
			{
				PawnCapacityDef activityLocal;
				if (PawnCapacityUtility.BodyCanEverDoCapacity(pawn.RaceProps.body, item))
				{
					activityLocal = item;
					Pair<string, Color> efficiencyLabel = GetEfficiencyLabel(pawn, item);
					DrawLeftRow(rect, ref curY, item.GetLabelFor(pawn).CapitalizeFirst(), efficiencyLabel.First, efficiencyLabel.Second, new TipSignal(TipGetter, pawn.thingIDNumber ^ item.index));
				}
				string TipGetter()
				{
					if (!pawn.Dead)
					{
						return GetPawnCapacityTip(pawn, activityLocal);
					}
					return "";
				}
			}
		}
		return curY;
	}

	private static void DrawLeftRow(Rect rect, ref float curY, string leftLabel, string rightLabel, Color rightLabelColor, TipSignal tipSignal)
	{
		Rect rect2 = new Rect(17f, curY, rect.width - 34f - 10f, 22f);
		if (Mouse.IsOver(rect2))
		{
			using (new TextBlock(HighlightColor))
			{
				GUI.DrawTexture(rect2, TexUI.HighlightTex);
			}
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect2, leftLabel);
		GUI.color = rightLabelColor;
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.Label(rect2, rightLabel);
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect3 = new Rect(0f, curY, rect.width, 20f);
		if (Mouse.IsOver(rect3))
		{
			TooltipHandler.TipRegion(rect3, tipSignal);
		}
		curY += rect2.height;
	}

	private static void DrawHediffRow(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY, float rowLeftPad = 0f)
	{
		float num = rect.width * 0.375f;
		float width = rect.width - num - lastMaxIconsTotalWidth;
		BodyPartRecord part = diffs.First().Part;
		float a = ((part != null) ? Text.CalcHeight(part.LabelCap, num) : Text.CalcHeight("WholeBody".Translate(), num));
		float num2 = 0f;
		float num3 = curY;
		float num4 = 0f;
		foreach (IGrouping<int, Hediff> item in from x in diffs
			group x by x.UIGroupKey)
		{
			int num5 = item.Count();
			string text = item.First().LabelCap;
			if (num5 != 1)
			{
				text = text + " x" + num5;
			}
			num4 += Text.CalcHeight(text, width);
		}
		num2 = num4;
		DoRightRowHighlight(new Rect(0f, curY, rect.width, Mathf.Max(a, num2)));
		if (part != null)
		{
			GUI.color = HealthUtility.GetPartConditionLabel(pawn, part).Second;
			Widgets.Label(new Rect(rowLeftPad, curY, num, 100f), part.LabelCap);
		}
		else
		{
			GUI.color = HealthUtility.RedColor;
			Widgets.Label(new Rect(rowLeftPad, curY, num, 100f), "WholeBody".Translate());
		}
		GUI.color = Color.white;
		foreach (IGrouping<int, Hediff> item2 in from x in diffs
			group x by x.UIGroupKey)
		{
			int num6 = 0;
			Hediff hediff = null;
			Texture2D bleedingIcon = null;
			TextureAndColor stateIcon = null;
			float totalBleedRate = 0f;
			foreach (Hediff item3 in item2)
			{
				if (num6 == 0)
				{
					hediff = item3;
				}
				stateIcon = item3.StateIcon;
				if (item3.Bleeding)
				{
					bleedingIcon = BleedingIcon;
				}
				totalBleedRate += item3.BleedRate;
				num6++;
			}
			string text2 = hediff.LabelCap;
			if (num6 != 1)
			{
				text2 = text2 + " x" + num6.ToStringCached();
			}
			float num7 = Text.CalcHeight(text2, width);
			Rect rect2 = new Rect(num, curY, width, num7);
			Widgets.DrawHighlightIfMouseover(rect2);
			GUI.color = hediff.LabelColor;
			Widgets.Label(rect2, text2);
			GUI.color = Color.white;
			Rect iconsRect = new Rect(rect2.x + 10f, rect2.y, rect.width - num - 10f, rect2.height);
			List<GenUI.AnonymousStackElement> list = new List<GenUI.AnonymousStackElement>();
			Hediff localHediff = hediff;
			if (DebugSettings.ShowDevGizmos && Current.ProgramState == ProgramState.Playing)
			{
				list.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						float num9 = iconsRect.width - (r.x - iconsRect.x) - 20f;
						r = new Rect(iconsRect.x + num9, r.y, 20f, 20f);
						GUI.color = Color.red;
						TooltipHandler.TipRegion(r, () => "DEV: Remove hediff", 1071045645);
						if (GUI.Button(r, TexButton.Delete))
						{
							pawn.health.RemoveHediff(localHediff);
						}
						GUI.color = Color.white;
					},
					width = 20f
				});
				if (localHediff.def.maxSeverity < float.MaxValue || localHediff.def.lethalSeverity > 0f)
				{
					list.Add(new GenUI.AnonymousStackElement
					{
						drawer = delegate(Rect r)
						{
							float num9 = iconsRect.width - (r.x - iconsRect.x) - 20f;
							r = new Rect(iconsRect.x + num9, r.y, 20f, 20f);
							GUI.color = Color.cyan;
							TooltipHandler.TipRegion(r, () => "DEV: Set severity", 2131648723);
							if (GUI.Button(r, TexButton.Save))
							{
								Find.WindowStack.Add(new Dialog_DebugSetSeverity(localHediff));
							}
							GUI.color = Color.white;
						},
						width = 20f
					});
				}
				if (localHediff.TryGetComp<HediffComp_Disappears>() != null)
				{
					list.Add(new GenUI.AnonymousStackElement
					{
						drawer = delegate(Rect r)
						{
							float num9 = iconsRect.width - (r.x - iconsRect.x) - 20f;
							r = new Rect(iconsRect.x + num9, r.y, 20f, 20f);
							GUI.color = Color.yellow;
							TooltipHandler.TipRegion(r, () => "DEV: Set remaining time", 6234623);
							if (GUI.Button(r, TexButton.Save))
							{
								Find.WindowStack.Add(new Dialog_DebugSetHediffRemaining(localHediff));
							}
							GUI.color = Color.white;
						},
						width = 20f
					});
				}
			}
			list.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					float num9 = iconsRect.width - (r.x - iconsRect.x) - 20f;
					r = new Rect(iconsRect.x + num9, r.y, 20f, 20f);
					Widgets.InfoCardButton(r, localHediff);
				},
				width = 20f
			});
			if ((bool)bleedingIcon)
			{
				list.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						float num9 = iconsRect.width - (r.x - iconsRect.x) - 20f;
						r = new Rect(iconsRect.x + num9, r.y, 20f, 20f);
						GUI.DrawTexture(r.ContractedBy(GenMath.LerpDouble(0f, 0.6f, 5f, 0f, Mathf.Min(totalBleedRate, 1f))), bleedingIcon);
					},
					width = 20f
				});
			}
			if (stateIcon.HasValue)
			{
				list.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						float num9 = iconsRect.width - (r.x - iconsRect.x) - 20f;
						r = new Rect(iconsRect.x + num9, r.y, 20f, 20f);
						GUI.color = stateIcon.Color;
						GUI.DrawTexture(r, stateIcon.Texture);
						GUI.color = Color.white;
					},
					width = 20f
				});
			}
			GenUI.DrawElementStack(iconsRect, num7, list, delegate(Rect r, GenUI.AnonymousStackElement obj)
			{
				obj.drawer(r);
			}, (GenUI.AnonymousStackElement obj) => obj.width);
			lastMaxIconsTotalWidth = Mathf.Max(lastMaxIconsTotalWidth, list.Sum((GenUI.AnonymousStackElement x) => x.width + 5f) - 5f);
			if (Mouse.IsOver(rect2))
			{
				int num8 = 0;
				foreach (Hediff hediff2 in item2)
				{
					TooltipHandler.TipRegion(rect2, new TipSignal(() => hediff2.GetTooltip(pawn, showHediffsDebugInfo), (int)curY + 7857 + num8++, TooltipPriority.Default));
				}
				if (part != null)
				{
					TooltipHandler.TipRegion(rect2, new TipSignal(() => GetTooltip(pawn, part), (int)curY + 7857 + ++num8, TooltipPriority.Pawn));
					if (DebugViewSettings.drawWoundAnchorsOnHover && pawn.Drawer.renderer.WoundOverlays.debugDrawPart != part)
					{
						pawn.Drawer.renderer.WoundOverlays.debugDrawPart = part;
						pawn.Drawer.renderer.WoundOverlays.ClearCache();
						PortraitsCache.SetDirty(pawn);
						GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
					}
				}
			}
			if (Widgets.ButtonInvisible(rect2, CanEntryBeClicked(item2, pawn)))
			{
				EntryClicked(item2, pawn);
			}
			curY += num7;
		}
		GUI.color = Color.white;
		curY = num3 + Mathf.Max(a, num2);
	}

	private static bool CanDoRecipeWithMedicineRestriction(IBillGiver giver, RecipeDef recipe)
	{
		if (!(giver is Pawn { playerSettings: not null } pawn))
		{
			return true;
		}
		if (!recipe.ingredients.Any((IngredientCount x) => x.filter.AnyAllowedDef.IsMedicine))
		{
			return true;
		}
		MedicalCareCategory medicalCareCategory = WorkGiver_DoBill.GetMedicalCareCategory(pawn);
		foreach (Thing item in pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Medicine))
		{
			foreach (IngredientCount ingredient in recipe.ingredients)
			{
				if (ingredient.filter.Allows(item) && medicalCareCategory.AllowsMedicine(item.def))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetPainTip(Pawn pawn)
	{
		return "PainLevel".Translate() + ": " + (pawn.health.hediffSet.PainTotal * 100f).ToString("F0") + "%";
	}

	private static string GetTooltip(Pawn pawn, BodyPartRecord part)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(part.LabelCap + ": ");
		stringBuilder.AppendLine(" " + pawn.health.hediffSet.GetPartHealth(part) + " / " + part.def.GetMaxHealth(pawn));
		float num = PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, part);
		if (num != 1f)
		{
			stringBuilder.AppendLine("Efficiency".Translate() + ": " + num.ToStringPercent());
		}
		return stringBuilder.ToString();
	}

	public static string GetPawnCapacityTip(Pawn pawn, PawnCapacityDef capacity)
	{
		List<PawnCapacityUtility.CapacityImpactor> list = new List<PawnCapacityUtility.CapacityImpactor>();
		float eff = PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, capacity, list);
		list.RemoveAll((PawnCapacityUtility.CapacityImpactor x) => x is PawnCapacityUtility.CapacityImpactorCapacity capacityImpactorCapacity && !capacityImpactorCapacity.capacity.CanShowOnPawn(pawn));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(capacity.GetLabelFor(pawn).CapitalizeFirst() + ": " + GetEfficiencyEstimateLabel(eff));
		if (list.Count > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("AffectedBy".Translate());
			for (int num = 0; num < list.Count; num++)
			{
				if (list[num] is PawnCapacityUtility.CapacityImpactorHediff capacityImpactorHediff && !tmpHediffImpactors.Contains(capacityImpactorHediff.hediff))
				{
					stringBuilder.AppendLine($"  {list[num].Readable(pawn)}");
					tmpHediffImpactors.Add(capacityImpactorHediff.hediff);
				}
			}
			tmpHediffImpactors.Clear();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				if (list[num2] is PawnCapacityUtility.CapacityImpactorBodyPartHealth capacityImpactorBodyPartHealth && !tmpBodyPartImpactors.Contains(capacityImpactorBodyPartHealth.bodyPart))
				{
					stringBuilder.AppendLine($"  {list[num2].Readable(pawn)}");
					tmpBodyPartImpactors.Add(capacityImpactorBodyPartHealth.bodyPart);
				}
			}
			tmpBodyPartImpactors.Clear();
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				if (list[num3] is PawnCapacityUtility.CapacityImpactorGene capacityImpactorGene && !tmpGeneImpactors.Contains(capacityImpactorGene.gene))
				{
					stringBuilder.AppendLine($"  {list[num3].Readable(pawn)}");
					tmpGeneImpactors.Add(capacityImpactorGene.gene);
				}
			}
			tmpGeneImpactors.Clear();
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				if (list[num4] is PawnCapacityUtility.CapacityImpactorCapacity)
				{
					stringBuilder.AppendLine($"  {list[num4].Readable(pawn)}");
				}
			}
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				if (list[num5] is PawnCapacityUtility.CapacityImpactorPain)
				{
					stringBuilder.AppendLine($"  {list[num5].Readable(pawn)}");
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static bool CanEntryBeClicked(IEnumerable<Hediff> diffs, Pawn pawn)
	{
		if (!GetCombatLogInfo(diffs, out var _, out var combatLogEntry) || combatLogEntry == null)
		{
			return false;
		}
		if (!Find.BattleLog.Battles.Any((Battle b) => b.Concerns(pawn) && b.Entries.Any((LogEntry e) => e == combatLogEntry)))
		{
			return false;
		}
		return true;
	}

	private static void EntryClicked(IEnumerable<Hediff> diffs, Pawn pawn)
	{
		if (GetCombatLogInfo(diffs, out var _, out var combatLogEntry) && combatLogEntry != null && Find.BattleLog.Battles.Any((Battle b) => b.Concerns(pawn) && b.Entries.Any((LogEntry e) => e == combatLogEntry)) && InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Log)) is ITab_Pawn_Log tab_Pawn_Log)
		{
			tab_Pawn_Log.SeekTo(combatLogEntry);
			tab_Pawn_Log.Highlight(combatLogEntry);
		}
	}

	public static bool GetCombatLogInfo(IEnumerable<Hediff> diffs, out TaggedString combatLogText, out LogEntry combatLogEntry)
	{
		combatLogText = null;
		combatLogEntry = null;
		foreach (Hediff diff in diffs)
		{
			if ((diff.combatLogEntry != null && diff.combatLogEntry.Target != null) || (combatLogText.NullOrEmpty() && !diff.combatLogText.NullOrEmpty()))
			{
				combatLogEntry = ((diff.combatLogEntry != null) ? diff.combatLogEntry.Target : null);
				combatLogText = diff.combatLogText;
			}
			if (combatLogEntry != null)
			{
				return true;
			}
		}
		return false;
	}

	private static void DoRightRowHighlight(Rect rowRect)
	{
		if (highlight)
		{
			GUI.color = StaticHighlightColor;
			GUI.DrawTexture(rowRect, TexUI.HighlightTex);
		}
		highlight = !highlight;
	}

	private static void DoDebugOptions(Rect rightRect, Pawn pawn)
	{
		if (!Widgets.ButtonText(new Rect(rightRect.x + 240f, rightRect.y - 27f, 115f, 25f), "Dev tool..."))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>(6);
		list.Add(new FloatMenuOption("Add hediff", delegate
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_AddHediff(pawn)));
		}));
		FloatMenuOption floatMenuOption = FloatMenuOption.CheckboxLabeled("Hediff debug tooltips", delegate
		{
			showHediffsDebugInfo = !showHediffsDebugInfo;
		}, showHediffsDebugInfo);
		floatMenuOption.tooltip = "Hover over hediffs in the health window to get extra debug information about them.";
		list.Add(floatMenuOption);
		list.Add(FloatMenuOption.CheckboxLabeled("Show hidden Hediffs", delegate
		{
			showAllHediffs = !showAllHediffs;
		}, showAllHediffs));
		list.Add(new FloatMenuOption("Table: BodyPartRecord info", delegate
		{
			float totalCorpseNutrition = 0f;
			if (pawn.RaceProps.hasCorpse)
			{
				totalCorpseNutrition = StatDefOf.Nutrition.Worker.GetValueAbstract(pawn.RaceProps.corpseDef);
			}
			DebugTables.MakeTablesDialog(pawn.RaceProps.body.AllParts, new TableDataGetter<BodyPartRecord>("defName", (BodyPartRecord b) => b.def.defName), new TableDataGetter<BodyPartRecord>("Coverage", (BodyPartRecord b) => pawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(b).ToStringPercent()), new TableDataGetter<BodyPartRecord>("Hit chance\n(this or any child)", (BodyPartRecord b) => b.coverageAbsWithChildren.ToStringPercent()), new TableDataGetter<BodyPartRecord>("Hit chance\n(this part)", (BodyPartRecord b) => b.coverageAbs.ToStringPercent()), new TableDataGetter<BodyPartRecord>("Efficiency", (BodyPartRecord b) => PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, b).ToStringPercent()), new TableDataGetter<BodyPartRecord>("Nutrition", (BodyPartRecord b) => (!(totalCorpseNutrition > 0f)) ? 0f : FoodUtility.GetBodyPartNutrition(totalCorpseNutrition, pawn, b)), new TableDataGetter<BodyPartRecord>("Solid", (BodyPartRecord b) => (!pawn.health.hediffSet.PartIsMissing(b)) ? b.def.IsSolid(b, pawn.health.hediffSet.hediffs).ToStringCheckBlank() : ""), new TableDataGetter<BodyPartRecord>("Skin covered", (BodyPartRecord b) => (!pawn.health.hediffSet.PartIsMissing(b)) ? b.def.IsSkinCovered(b, pawn.health.hediffSet).ToStringCheckBlank() : ""), new TableDataGetter<BodyPartRecord>("Is missing", (BodyPartRecord b) => pawn.health.hediffSet.PartIsMissing(b).ToStringCheckBlank()), new TableDataGetter<BodyPartRecord>("Is missing parts", (BodyPartRecord b) => pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(b).ToStringCheckBlank()));
		}));
		list.Add(new FloatMenuOption("Table: BodyPartGroupDef info", delegate
		{
			DebugTables.MakeTablesDialog(DefDatabase<BodyPartGroupDef>.AllDefs.Where((BodyPartGroupDef x) => pawn.RaceProps.body.AllParts.Any((BodyPartRecord y) => y.groups.Contains(x))), new TableDataGetter<BodyPartGroupDef>("defName", (BodyPartGroupDef b) => b.defName), new TableDataGetter<BodyPartGroupDef>("Efficiency", (BodyPartGroupDef b) => PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(pawn.health.hediffSet, b).ToStringPercent()));
		}));
		list.Add(new FloatMenuOption("Table: HediffGiver_Birthday", delegate
		{
			List<TableDataGetter<HediffGiver_Birthday>> list2 = new List<TableDataGetter<HediffGiver_Birthday>>
			{
				new TableDataGetter<HediffGiver_Birthday>("label", (HediffGiver_Birthday b) => b.hediff.LabelCap)
			};
			for (int num = 1; (float)num < pawn.RaceProps.lifeExpectancy + 20f; num++)
			{
				int age = num;
				list2.Add(new TableDataGetter<HediffGiver_Birthday>("Chance at\n" + num, (HediffGiver_Birthday h) => h.DebugChanceToHaveAtAge(pawn, age).ToStringPercent()));
			}
			list2.Add(new TableDataGetter<HediffGiver_Birthday>("Spacing", (HediffGiver_Birthday h) => ""));
			for (int num2 = 1; (float)num2 < pawn.RaceProps.lifeExpectancy + 20f; num2++)
			{
				int age2 = num2;
				list2.Add(new TableDataGetter<HediffGiver_Birthday>("Count at\n" + num2, delegate
				{
					float num3 = 0f;
					foreach (HediffGiverSetDef hediffGiverSet in pawn.RaceProps.hediffGiverSets)
					{
						foreach (HediffGiver_Birthday item in hediffGiverSet.hediffGivers.OfType<HediffGiver_Birthday>())
						{
							num3 += item.DebugChanceToHaveAtAge(pawn, age2);
						}
					}
					return num3.ToStringDecimalIfSmall();
				}));
			}
			DebugTables.MakeTablesDialog(pawn.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef x) => x.hediffGivers.OfType<HediffGiver_Birthday>()), list2.ToArray());
		}));
		if (pawn.health.immunity.ImmunityListForReading.Any())
		{
			list.Add(new FloatMenuOption("Table: Immunities", delegate
			{
				List<TableDataGetter<ImmunityRecord>> list2 = new List<TableDataGetter<ImmunityRecord>>
				{
					new TableDataGetter<ImmunityRecord>("hediff", (ImmunityRecord i) => i.hediffDef.LabelCap),
					new TableDataGetter<ImmunityRecord>("source", (ImmunityRecord i) => i.source.LabelCap),
					new TableDataGetter<ImmunityRecord>("immunity", (ImmunityRecord i) => i.immunity.ToStringPercent())
				};
				DebugTables.MakeTablesDialog(pawn.health.immunity.ImmunityListForReading, list2.ToArray());
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public static Pair<string, Color> GetEfficiencyLabel(Pawn pawn, PawnCapacityDef activity)
	{
		float level = pawn.health.capacities.GetLevel(activity);
		return new Pair<string, Color>(PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, activity).ToStringPercent(), efficiencyToColor[EfficiencyValueToEstimate(level)]);
	}

	public static string GetEfficiencyEstimateLabel(float eff)
	{
		return EfficiencyValueToEstimate(eff).ToString().Translate();
	}

	public static EfficiencyEstimate EfficiencyValueToEstimate(float eff)
	{
		if (eff <= 0f)
		{
			return EfficiencyEstimate.None;
		}
		if (eff < 0.4f)
		{
			return EfficiencyEstimate.VeryPoor;
		}
		if (eff < 0.7f)
		{
			return EfficiencyEstimate.Poor;
		}
		if (eff < 1f && !Mathf.Approximately(eff, 1f))
		{
			return EfficiencyEstimate.Weakened;
		}
		if (Mathf.Approximately(eff, 1f))
		{
			return EfficiencyEstimate.GoodCondition;
		}
		return EfficiencyEstimate.Enhanced;
	}

	public static Pair<string, Color> GetPainLabel(Pawn pawn)
	{
		float painTotal = pawn.health.hediffSet.PainTotal;
		string text = "";
		Color white = Color.white;
		if (Mathf.Approximately(painTotal, 0f))
		{
			text = "NoPain".Translate();
			white = HealthUtility.GoodConditionColor;
		}
		else if (painTotal < 0.15f)
		{
			text = "LittlePain".Translate();
			white = Color.gray;
		}
		else if (painTotal < 0.4f)
		{
			text = "MediumPain".Translate();
			white = MediumPainColor;
		}
		else if (painTotal < 0.8f)
		{
			text = "SeverePain".Translate();
			white = SeverePainColor;
		}
		else
		{
			text = "ExtremePain".Translate();
			white = HealthUtility.RedColor;
		}
		return new Pair<string, Color>(text, white);
	}

	public static bool ShowBloodLoss(Thing thing)
	{
		if (thing is Corpse corpse)
		{
			return corpse.Age < 60000;
		}
		return true;
	}
}
