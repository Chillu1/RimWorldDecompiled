using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class Bill_Medical : Bill
{
	private BodyPartRecord part;

	public Dictionary<ThingDef, int> consumedMedicine = new Dictionary<ThingDef, int>();

	public List<Thing> uniqueRequiredIngredients;

	public int temp_partIndexToSetLater;

	public bool iterationCompleted;

	private const float MetalhorrorInfectionChance = 0.5f;

	public override bool CheckIngredientsIfSociallyProper => false;

	protected override bool CanCopy => false;

	public override bool CompletableEver
	{
		get
		{
			if (recipe.targetsBodyPart && !recipe.Worker.GetPartsToApplyOn(GiverPawn, recipe).Contains(part))
			{
				return false;
			}
			if (recipe.Worker is Recipe_Surgery recipe_Surgery && !recipe_Surgery.CompletableEver(GiverPawn))
			{
				return false;
			}
			if (!uniqueRequiredIngredients.NullOrEmpty() && !iterationCompleted)
			{
				foreach (Thing uniqueRequiredIngredient in uniqueRequiredIngredients)
				{
					if (uniqueRequiredIngredient.DestroyedOrNull())
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public BodyPartRecord Part
	{
		get
		{
			return part;
		}
		set
		{
			if (billStack == null && part != null)
			{
				Log.Error("Can only set Bill_Medical.Part after the bill has been added to a pawn's bill stack.");
			}
			else
			{
				part = value;
			}
		}
	}

	public Pawn GiverPawn
	{
		get
		{
			Pawn pawn = billStack.billGiver as Pawn;
			if (billStack.billGiver is Corpse corpse)
			{
				pawn = corpse.InnerPawn;
			}
			if (pawn == null)
			{
				throw new InvalidOperationException("Medical bill on non-pawn.");
			}
			return pawn;
		}
	}

	public override string Label
	{
		get
		{
			string text = recipe.Worker.LabelFromUniqueIngredients(this);
			if (!text.NullOrEmpty())
			{
				return text;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(recipe.Worker.GetLabelWhenUsedOn(GiverPawn, part));
			if (Part != null && !recipe.hideBodyPartNames)
			{
				stringBuilder.Append(" (" + Part.Label + ")");
			}
			return stringBuilder.ToString();
		}
	}

	public Bill_Medical()
	{
	}

	public Bill_Medical(RecipeDef recipe, List<Thing> uniqueIngredients)
		: base(recipe)
	{
		uniqueRequiredIngredients = uniqueIngredients;
	}

	public override bool ShouldDoNow()
	{
		if (suspended)
		{
			return false;
		}
		return true;
	}

	public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
	{
		base.Notify_IterationCompleted(billDoer, ingredients);
		iterationCompleted = true;
		if (CompletableEver)
		{
			Pawn giverPawn = GiverPawn;
			recipe.Worker.ApplyOnPawn(giverPawn, Part, billDoer, ingredients, this);
			if (giverPawn.RaceProps.IsFlesh)
			{
				giverPawn.records.Increment(RecordDefOf.OperationsReceived);
				billDoer.records.Increment(RecordDefOf.OperationsPerformed);
				if (ModsConfig.AnomalyActive && Rand.Chance(0.5f) && MetalhorrorUtility.IsInfected(billDoer))
				{
					MetalhorrorUtility.Infect(giverPawn, billDoer, "SurgeryImplant");
				}
			}
		}
		billStack.Delete(this);
	}

	public override void Notify_BillWorkStarted(Pawn billDoer)
	{
		base.Notify_BillWorkStarted(billDoer);
		consumedMedicine.Clear();
		if (GiverPawn.Dead || !recipe.anesthetize || !HealthUtility.TryAnesthetize(GiverPawn))
		{
			return;
		}
		List<ThingCountClass> placedThings = billDoer.CurJob.placedThings;
		if (placedThings == null)
		{
			return;
		}
		for (int i = 0; i < placedThings.Count; i++)
		{
			if (placedThings[i].thing is Medicine)
			{
				recipe.Worker.ConsumeIngredient(placedThings[i].thing.SplitOff(1), recipe, billDoer.MapHeld);
				placedThings[i].Count--;
				consumedMedicine.Add(placedThings[i].thing.def, 1);
				if (placedThings[i].thing.Destroyed || placedThings[i].Count <= 0)
				{
					placedThings.RemoveAt(i);
				}
				break;
			}
		}
	}

	public override bool PawnAllowedToStartAnew(Pawn pawn)
	{
		if (!base.PawnAllowedToStartAnew(pawn))
		{
			return false;
		}
		if (recipe.Worker is Recipe_AdministerIngestible)
		{
			ThingDef singleDef = recipe.ingredients[0].filter.BestThingRequest.singleDef;
			if (singleDef.IsDrug && !new HistoryEvent(HistoryEventDefOf.AdministeredDrug, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
			{
				return false;
			}
			if (singleDef.IsDrug && singleDef.ingestible.drugCategory == DrugCategory.Hard && !new HistoryEvent(HistoryEventDefOf.AdministeredHardDrug, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
			{
				return false;
			}
			if (singleDef.IsNonMedicalDrug && !new HistoryEvent(HistoryEventDefOf.AdministeredRecreationalDrug, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
			{
				return false;
			}
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_BodyParts.Look(ref part, "part");
		Scribe_Collections.Look(ref consumedMedicine, "consumedMedicine");
		Scribe_Collections.Look(ref uniqueRequiredIngredients, "uniqueRequiredIngredients", LookMode.Reference);
		Scribe_Values.Look(ref iterationCompleted, "iterationCompleted", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			ThingDef value = null;
			Scribe_Defs.Look(ref value, "consumedInitialMedicineDef");
			if (value != null)
			{
				if (consumedMedicine == null)
				{
					consumedMedicine = new Dictionary<ThingDef, int>();
				}
				consumedMedicine.Add(value, 1);
			}
		}
		else if (Scribe.mode == LoadSaveMode.PostLoadInit && consumedMedicine == null)
		{
			consumedMedicine = new Dictionary<ThingDef, int>();
		}
		BackCompatibility.PostExposeData(this);
	}

	public override Bill Clone()
	{
		Bill_Medical obj = (Bill_Medical)base.Clone();
		obj.part = part;
		obj.consumedMedicine = new Dictionary<ThingDef, int>(consumedMedicine);
		return obj;
	}
}
