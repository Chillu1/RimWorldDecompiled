using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Bill_Medical : Bill
	{
		private BodyPartRecord part;

		public ThingDef consumedInitialMedicineDef;

		public int temp_partIndexToSetLater;

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
				Corpse corpse = billStack.billGiver as Corpse;
				if (corpse != null)
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

		public Bill_Medical(RecipeDef recipe)
			: base(recipe)
		{
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
			if (CompletableEver)
			{
				Pawn giverPawn = GiverPawn;
				recipe.Worker.ApplyOnPawn(giverPawn, Part, billDoer, ingredients, this);
				if (giverPawn.RaceProps.IsFlesh)
				{
					giverPawn.records.Increment(RecordDefOf.OperationsReceived);
					billDoer.records.Increment(RecordDefOf.OperationsPerformed);
				}
			}
			billStack.Delete(this);
		}

		public override void Notify_DoBillStarted(Pawn billDoer)
		{
			base.Notify_DoBillStarted(billDoer);
			consumedInitialMedicineDef = null;
			if (GiverPawn.Dead || !recipe.anesthetize || !HealthUtility.TryAnesthetize(GiverPawn))
			{
				return;
			}
			List<ThingCountClass> placedThings = billDoer.CurJob.placedThings;
			for (int i = 0; i < placedThings.Count; i++)
			{
				if (placedThings[i].thing is Medicine)
				{
					recipe.Worker.ConsumeIngredient(placedThings[i].thing.SplitOff(1), recipe, billDoer.MapHeld);
					placedThings[i].Count--;
					consumedInitialMedicineDef = placedThings[i].thing.def;
					if (placedThings[i].thing.Destroyed || placedThings[i].Count <= 0)
					{
						placedThings.RemoveAt(i);
					}
					break;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_BodyParts.Look(ref part, "part");
			Scribe_Defs.Look(ref consumedInitialMedicineDef, "consumedInitialMedicineDef");
			BackCompatibility.PostExposeData(this);
		}

		public override Bill Clone()
		{
			Bill_Medical obj = (Bill_Medical)base.Clone();
			obj.part = part;
			obj.consumedInitialMedicineDef = consumedInitialMedicineDef;
			return obj;
		}
	}
}
