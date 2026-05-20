using System.Collections.Generic;
using LudeonTK;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class ITab_Bills : ITab
{
	private float viewHeight = 1000f;

	private Vector2 scrollPosition;

	private Bill mouseoverBill;

	private static readonly Vector2 WinSize = new Vector2(420f, 480f);

	[TweakValue("Interface", 0f, 128f)]
	private static float PasteX = 48f;

	[TweakValue("Interface", 0f, 128f)]
	private static float PasteY = 3f;

	[TweakValue("Interface", 0f, 32f)]
	private static float PasteSize = 24f;

	protected Building_WorkTable SelTable => (Building_WorkTable)base.SelThing;

	public ITab_Bills()
	{
		size = WinSize;
		labelKey = "TabBills";
		tutorTag = "Bills";
	}

	protected override void FillTab()
	{
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BillsTab, KnowledgeAmount.FrameDisplayed);
		Rect rect = new Rect(WinSize.x - PasteX, PasteY, PasteSize, PasteSize);
		if (BillUtility.Clipboard != null)
		{
			if (!SelTable.def.AllRecipes.Contains(BillUtility.Clipboard.recipe) || !BillUtility.Clipboard.recipe.AvailableNow || !BillUtility.Clipboard.recipe.AvailableOnNow(SelTable))
			{
				GUI.color = Color.gray;
				Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
				GUI.color = Color.white;
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, "ClipboardBillNotAvailableHere".Translate() + ": " + BillUtility.Clipboard.LabelCap);
				}
			}
			else if (SelTable.billStack.Count >= 15)
			{
				GUI.color = Color.gray;
				Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
				GUI.color = Color.white;
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, "PasteBillTip".Translate() + " (" + "PasteBillTip_LimitReached".Translate() + "): " + BillUtility.Clipboard.LabelCap);
				}
			}
			else
			{
				if (Widgets.ButtonImageFitted(rect, TexButton.Paste, Color.white))
				{
					Bill bill = BillUtility.Clipboard.Clone();
					bill.InitializeAfterClone();
					SelTable.billStack.AddBill(bill);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, "PasteBillTip".Translate() + ": " + BillUtility.Clipboard.LabelCap);
				}
			}
		}
		Rect rect2 = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
		mouseoverBill = SelTable.billStack.DoListing(rect2, OptionsMaker, ref scrollPosition, ref viewHeight);
		List<FloatMenuOption> OptionsMaker()
		{
			List<FloatMenuOption> opts = new List<FloatMenuOption>();
			for (int i = 0; i < SelTable.def.AllRecipes.Count; i++)
			{
				RecipeDef recipe;
				if (SelTable.def.AllRecipes[i].AvailableNow && SelTable.def.AllRecipes[i].AvailableOnNow(SelTable))
				{
					recipe = SelTable.def.AllRecipes[i];
					Add();
					foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
					{
						foreach (Precept_Building cachedPossibleBuilding in allIdeo.cachedPossibleBuildings)
						{
							if (cachedPossibleBuilding.ThingDef == recipe.ProducedThingDef)
							{
								Add(cachedPossibleBuilding);
							}
						}
					}
				}
				void Add(Precept_ThingStyle precept = null)
				{
					string label = ((precept != null) ? "RecipeMake".Translate(precept.LabelCap).CapitalizeFirst() : recipe.LabelCap);
					opts.Add(new FloatMenuOption(label, delegate
					{
						if (ModsConfig.BiotechActive && recipe.mechanitorOnlyRecipe && !SelTable.Map.mapPawns.FreeColonists.Any(MechanitorUtility.IsMechanitor))
						{
							Find.WindowStack.Add(new Dialog_MessageBox("RecipeRequiresMechanitor".Translate(recipe.LabelCap)));
						}
						else if (!SelTable.Map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
						{
							Bill.CreateNoPawnsWithSkillDialog(recipe);
						}
						Bill bill2 = recipe.MakeNewBill(precept);
						SelTable.billStack.AddBill(bill2);
						if (recipe.conceptLearned != null)
						{
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
						}
						if (TutorSystem.TutorialMode)
						{
							TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap.Resolve());
						}
					}, iconTex: recipe.UIIcon, shownItemForIcon: recipe.UIIconThing, thingStyle: null, forceBasicStyle: false, priority: MenuOptionPriority.Default, mouseoverGuiAction: delegate(Rect rect3)
					{
						BillUtility.DoBillInfoWindow(i, label, rect3, recipe);
					}, revalidateClickTarget: null, extraPartWidth: 29f, extraPartOnGUI: (Rect rect3) => Widgets.InfoCardButton(rect3.x + 5f, rect3.y + (rect3.height - 24f) / 2f, recipe, precept), revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: -recipe.displayPriority));
				}
			}
			if (!opts.Any())
			{
				opts.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
			}
			return opts;
		}
	}

	public override void TabUpdate()
	{
		if (mouseoverBill != null)
		{
			mouseoverBill.TryDrawIngredientSearchRadiusOnMap(SelTable.Position);
			mouseoverBill = null;
		}
	}
}
