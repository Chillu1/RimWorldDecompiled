using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class Bill_Autonomous : Bill_Production
{
	public const int CompleteBillTicks = 300;

	public float formingTicks;

	protected int startedTick;

	protected FormingState state;

	private List<IngredientCount> ingredients = new List<IngredientCount>();

	public FormingState State => state;

	private Building_WorkTableAutonomous WorkTable => (Building_WorkTableAutonomous)billStack.billGiver;

	public override bool CanFinishNow => state == FormingState.Formed;

	private List<IngredientCount> CurrentBillIngredients
	{
		get
		{
			if (ingredients.Count == 0)
			{
				this.MakeIngredientsListInProcessingOrder(ingredients);
			}
			return ingredients;
		}
	}

	public Bill_Autonomous()
	{
	}

	public Bill_Autonomous(RecipeDef recipe, Precept_ThingStyle precept = null)
		: base(recipe, precept)
	{
	}

	public virtual Thing CreateProducts()
	{
		return null;
	}

	public override bool ShouldDoNow()
	{
		if (!base.ShouldDoNow())
		{
			return false;
		}
		return state != FormingState.Forming;
	}

	public override bool PawnAllowedToStartAnew(Pawn p)
	{
		if (!base.PawnAllowedToStartAnew(p))
		{
			return false;
		}
		if (WorkTable.ActiveBill != null && WorkTable.ActiveBill != this && (!WorkTable.ActiveBill.suspended || WorkTable.ActiveBill.State != FormingState.Gathering || WorkTable.innerContainer.Any()))
		{
			return false;
		}
		return true;
	}

	public override void Notify_DoBillStarted(Pawn billDoer)
	{
		base.Notify_DoBillStarted(billDoer);
		WorkTable.ActiveBill = this;
		startedTick = Find.TickManager.TicksGame;
	}

	public override void Notify_BillWorkFinished(Pawn billDoer)
	{
		base.Notify_BillWorkFinished(billDoer);
		switch (state)
		{
		case FormingState.Gathering:
			state = FormingState.Forming;
			formingTicks = recipe.formingTicks;
			WorkTable.Notify_StartForming(billDoer);
			break;
		case FormingState.Preparing:
			formingTicks = recipe.formingTicks;
			state = FormingState.Forming;
			break;
		case FormingState.Forming:
		case FormingState.Formed:
			break;
		}
	}

	public virtual void Reset()
	{
		ingredients.Clear();
		state = FormingState.Gathering;
	}

	public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
	{
		base.Notify_IterationCompleted(billDoer, ingredients);
		Reset();
		if (WorkTable.ActiveBill == this)
		{
			WorkTable.ActiveBill = null;
		}
	}

	public override float GetWorkAmount(Thing thing = null)
	{
		if (state == FormingState.Formed)
		{
			return 300f;
		}
		return base.GetWorkAmount(thing);
	}

	public virtual void BillTick()
	{
		if (!suspended && state == FormingState.Forming)
		{
			formingTicks -= 1f;
			if (formingTicks <= 0f)
			{
				state = FormingState.Formed;
				WorkTable.Notify_FormingCompleted();
			}
		}
	}

	public void AppendCurrentIngredientCount(StringBuilder sb)
	{
		foreach (IngredientCount currentBillIngredient in CurrentBillIngredients)
		{
			if (currentBillIngredient != null && currentBillIngredient.IsFixedIngredient)
			{
				TaggedString labelCap = currentBillIngredient.FixedIngredient.LabelCap;
				int num = WorkTable.innerContainer.TotalStackCountOfDef(currentBillIngredient.FixedIngredient);
				labelCap += $" {num} / {currentBillIngredient.CountRequiredOfFor(currentBillIngredient.FixedIngredient, recipe, this)}";
				sb.AppendLine(labelCap);
			}
		}
	}

	public virtual void AppendInspectionData(StringBuilder sb)
	{
		if (State == FormingState.Forming || State == FormingState.Preparing)
		{
			sb.AppendTagged("FinishesIn".Translate() + ": " + ((int)formingTicks).ToStringTicksToPeriod());
		}
		if (State == FormingState.Formed)
		{
			sb.AppendLine("Finished".Translate());
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref formingTicks, "formingTicks", 0f);
		Scribe_Values.Look(ref state, "state", FormingState.Gathering);
		Scribe_Values.Look(ref startedTick, "startedTick", 0);
	}
}
