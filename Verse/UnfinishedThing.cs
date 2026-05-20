using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class UnfinishedThing : ThingWithComps
{
	private Pawn creatorInt;

	private string creatorName = "ErrorCreatorName";

	private RecipeDef recipeInt;

	public List<Thing> ingredients = new List<Thing>();

	private Bill_ProductionWithUft boundBillInt;

	public float workLeft = -10000f;

	public bool debugCompleted;

	private const float CancelIngredientRecoveryFraction = 0.75f;

	public Pawn Creator
	{
		get
		{
			return creatorInt;
		}
		set
		{
			if (value == null)
			{
				Log.Error("Cannot set creator to null.");
				return;
			}
			creatorInt = value;
			creatorName = value.LabelShort;
		}
	}

	public RecipeDef Recipe => recipeInt;

	public Bill_ProductionWithUft BoundBill
	{
		get
		{
			if (boundBillInt != null && (boundBillInt.DeletedOrDereferenced || boundBillInt.BoundUft != this))
			{
				boundBillInt = null;
			}
			return boundBillInt;
		}
		set
		{
			if (value == boundBillInt)
			{
				return;
			}
			Bill_ProductionWithUft bill_ProductionWithUft = boundBillInt;
			boundBillInt = value;
			if (bill_ProductionWithUft != null && bill_ProductionWithUft.BoundUft == this)
			{
				bill_ProductionWithUft.SetBoundUft(null, setOtherLink: false);
			}
			if (value != null)
			{
				recipeInt = value.recipe;
				if (value.BoundUft != this)
				{
					value.SetBoundUft(this, setOtherLink: false);
				}
			}
		}
	}

	public Thing BoundWorkTable
	{
		get
		{
			if (BoundBill == null)
			{
				return null;
			}
			Thing thing = BoundBill.billStack.billGiver as Thing;
			if (thing.Destroyed)
			{
				return null;
			}
			return thing;
		}
	}

	public override string LabelNoCount
	{
		get
		{
			if (Recipe == null)
			{
				return base.LabelNoCount;
			}
			if (base.Stuff == null)
			{
				return "UnfinishedItem".Translate(Recipe.products[0].thingDef.label);
			}
			return "UnfinishedItemWithStuff".Translate(base.Stuff.LabelAsStuff, Recipe.products[0].thingDef.label);
		}
	}

	public override string DescriptionDetailed
	{
		get
		{
			if (Recipe == null)
			{
				return base.LabelNoCount;
			}
			return Recipe.ProducedThingDef.DescriptionDetailed;
		}
	}

	public override string DescriptionFlavor
	{
		get
		{
			if (Recipe == null)
			{
				return base.LabelNoCount;
			}
			return Recipe.ProducedThingDef.description;
		}
	}

	public bool Initialized => workLeft > -5000f;

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving && boundBillInt != null && boundBillInt.DeletedOrDereferenced)
		{
			boundBillInt = null;
		}
		Scribe_References.Look(ref creatorInt, "creator");
		Scribe_Values.Look(ref creatorName, "creatorName");
		Scribe_References.Look(ref boundBillInt, "bill");
		Scribe_Defs.Look(ref recipeInt, "recipe");
		Scribe_Values.Look(ref workLeft, "workLeft", 0f);
		Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Deep);
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode == DestroyMode.Cancel)
		{
			for (int i = 0; i < ingredients.Count; i++)
			{
				int num = GenMath.RoundRandom((float)ingredients[i].stackCount * 0.75f);
				if (num > 0)
				{
					ingredients[i].stackCount = num;
					GenPlace.TryPlaceThing(ingredients[i], base.Position, base.Map, ThingPlaceMode.Near);
				}
			}
			ingredients.Clear();
		}
		base.Destroy(mode);
		BoundBill = null;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandCancelConstructionLabel".Translate();
		command_Action.defaultDesc = "CommandCancelConstructionDesc".Translate();
		command_Action.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
		command_Action.hotKey = KeyBindingDefOf.Designator_Cancel;
		command_Action.action = delegate
		{
			Destroy(DestroyMode.Cancel);
		};
		yield return command_Action;
		if (Initialized && DebugSettings.ShowDevGizmos && !debugCompleted)
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Complete";
			command_Action2.action = delegate
			{
				debugCompleted = true;
			};
			yield return command_Action2;
		}
	}

	public Bill_ProductionWithUft BillOnTableForMe(Thing workTable)
	{
		if (Recipe.AllRecipeUsers.Contains(workTable.def))
		{
			IBillGiver billGiver = (IBillGiver)workTable;
			for (int i = 0; i < billGiver.BillStack.Count; i++)
			{
				if (billGiver.BillStack[i] is Bill_ProductionWithUft bill_ProductionWithUft && bill_ProductionWithUft.ShouldDoNow() && bill_ProductionWithUft != null && bill_ProductionWithUft.recipe == Recipe)
				{
					return bill_ProductionWithUft;
				}
			}
		}
		return null;
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (BoundWorkTable != null)
		{
			GenDraw.DrawLineBetween(this.TrueCenter(), BoundWorkTable.TrueCenter());
		}
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		text += "Author".Translate() + ": " + creatorName;
		text += "\n" + "WorkLeft".Translate() + ": " + workLeft.ToStringWorkAmount();
		if (BoundBill?.style?.Category != null && base.StyleSourcePrecept == null)
		{
			text += "\n" + "Style".Translate() + ": " + BoundBill.style.Category.LabelCap;
		}
		return text;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats())
		{
			yield return item;
		}
		if (BoundBill?.style?.Category != null && base.StyleSourcePrecept == null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Stat_Thing_StyleLabel".Translate(), BoundBill.style.Category.LabelCap, "Stat_Thing_StyleDesc".Translate(), 1108);
		}
	}
}
