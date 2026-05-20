using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI;

public static class Toils_Recipe
{
	private const int LongCraftingProjectThreshold = 10000;

	public static Toil MakeUnfinishedThingIfNeeded()
	{
		Toil toil = ToilMaker.MakeToil("MakeUnfinishedThingIfNeeded");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			if (curJob.RecipeDef.UsesUnfinishedThing && !(curJob.GetTarget(TargetIndex.B).Thing is UnfinishedThing))
			{
				List<Thing> list = CalculateIngredients(curJob, actor);
				Thing thing = CalculateDominantIngredient(curJob, list);
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing2 = list[i];
					actor.Map.designationManager.RemoveAllDesignationsOn(thing2);
					thing2.DeSpawnOrDeselect();
				}
				ThingDef stuff = (curJob.RecipeDef.unfinishedThingDef.MadeFromStuff ? thing.def : null);
				UnfinishedThing unfinishedThing = (UnfinishedThing)ThingMaker.MakeThing(curJob.RecipeDef.unfinishedThingDef, stuff);
				unfinishedThing.Creator = actor;
				unfinishedThing.BoundBill = (Bill_ProductionWithUft)curJob.bill;
				unfinishedThing.ingredients = list;
				unfinishedThing.workLeft = curJob.bill.GetWorkAmount(unfinishedThing);
				unfinishedThing.TryGetComp<CompColorable>()?.SetColor(thing.DrawColor);
				GenSpawn.Spawn(unfinishedThing, curJob.GetTarget(TargetIndex.A).Cell, actor.Map);
				curJob.SetTarget(TargetIndex.B, unfinishedThing);
				actor.Reserve(unfinishedThing, curJob);
			}
		};
		return toil;
	}

	public static Toil DoRecipeWork()
	{
		Toil toil = ToilMaker.MakeToil("DoRecipeWork");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
			Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
			UnfinishedThing unfinishedThing = thing as UnfinishedThing;
			_ = curJob.GetTarget(TargetIndex.A).Thing.def.building;
			if (unfinishedThing != null && unfinishedThing.Initialized)
			{
				jobDriver_DoBill.workLeft = unfinishedThing.workLeft;
			}
			else
			{
				jobDriver_DoBill.workLeft = curJob.bill.GetWorkAmount(thing);
				if (unfinishedThing != null)
				{
					if (unfinishedThing.debugCompleted)
					{
						unfinishedThing.workLeft = (jobDriver_DoBill.workLeft = 0f);
					}
					else
					{
						unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
					}
				}
			}
			jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
			jobDriver_DoBill.ticksSpentDoingRecipeWork = 0;
			curJob.bill.Notify_BillWorkStarted(actor);
		};
		toil.tickAction = delegate
		{
			Pawn actor = toil.actor;
			Thing thing = actor.jobs.curJob.GetTarget(TargetIndex.B).Thing;
			if (thing is UnfinishedThing && thing.Destroyed)
			{
				actor.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			else if (toil.actor.CurJob.GetTarget(TargetIndex.A).Thing is IBillGiverWithTickAction billGiverWithTickAction)
			{
				billGiverWithTickAction.UsedThisTick();
			}
		};
		toil.tickIntervalAction = delegate(int delta)
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
			UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
			if (unfinishedThing != null && unfinishedThing.Destroyed)
			{
				actor.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			else
			{
				jobDriver_DoBill.ticksSpentDoingRecipeWork += delta;
				curJob.bill.Notify_PawnDidWork(actor);
				if (curJob.RecipeDef.workSkill != null && curJob.RecipeDef.UsesUnfinishedThing && actor.skills != null)
				{
					actor.skills.Learn(curJob.RecipeDef.workSkill, 0.1f * curJob.RecipeDef.workSkillLearnFactor * (float)delta);
				}
				float num = ((curJob.RecipeDef.workSpeedStat == null) ? 1f : actor.GetStatValue(curJob.RecipeDef.workSpeedStat));
				if (curJob.RecipeDef.workTableSpeedStat != null && jobDriver_DoBill.BillGiver is Building_WorkTable thing)
				{
					num *= thing.GetStatValue(curJob.RecipeDef.workTableSpeedStat);
				}
				if (DebugSettings.fastCrafting)
				{
					num *= 30f;
				}
				jobDriver_DoBill.workLeft -= num * (float)delta;
				if (unfinishedThing != null)
				{
					if (unfinishedThing.debugCompleted)
					{
						unfinishedThing.workLeft = (jobDriver_DoBill.workLeft = 0f);
					}
					else
					{
						unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
					}
				}
				actor.GainComfortFromCellIfPossible(delta, chairsOnly: true);
				if (jobDriver_DoBill.workLeft <= 0f)
				{
					curJob.bill.Notify_BillWorkFinished(actor);
					jobDriver_DoBill.ReadyForNextToil();
				}
				else if (curJob.bill.recipe.UsesUnfinishedThing && Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick >= 3000 && actor.IsHashIntervalTick(1000, delta))
				{
					actor.jobs.CheckForJobOverride();
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
		toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
		toil.WithProgressBar(TargetIndex.A, delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.CurJob;
			Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
			float workLeft = ((JobDriver_DoBill)actor.jobs.curDriver).workLeft;
			float num = ((curJob.bill is Bill_Mech { State: FormingState.Formed }) ? 300f : curJob.bill.recipe.WorkAmountTotal(thing));
			return 1f - workLeft / num;
		});
		toil.FailOn((Func<bool>)delegate
		{
			RecipeDef recipeDef = toil.actor.CurJob.RecipeDef;
			if (recipeDef != null && recipeDef.interruptIfIngredientIsRotting)
			{
				LocalTargetInfo target = toil.actor.CurJob.GetTarget(TargetIndex.B);
				if (target.HasThing && (int)target.Thing.GetRotStage() > 0)
				{
					return true;
				}
			}
			return toil.actor.CurJob.bill.suspended;
		});
		toil.activeSkill = () => toil.actor.CurJob.bill.recipe.workSkill;
		return toil;
	}

	public static Toil FinishRecipeAndStartStoringProduct(TargetIndex productIndex = TargetIndex.A)
	{
		Toil toil = ToilMaker.MakeToil("FinishRecipeAndStartStoringProduct");
		toil.AddFinishAction(delegate
		{
			if (toil.actor.jobs.curJob.bill is Bill_Production bill_Production && bill_Production.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				toil.actor.Map.resourceCounter.UpdateResourceCounts();
			}
		});
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
			if (curJob.RecipeDef.workSkill != null && !curJob.RecipeDef.UsesUnfinishedThing && actor.skills != null)
			{
				float xp = (float)jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.1f * curJob.RecipeDef.workSkillLearnFactor;
				actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp);
			}
			List<Thing> ingredients = CalculateIngredients(curJob, actor);
			Thing dominantIngredient = CalculateDominantIngredient(curJob, ingredients);
			ThingStyleDef style = null;
			if (ModsConfig.IdeologyActive && curJob.bill.recipe.products != null && curJob.bill.recipe.products.Count == 1)
			{
				style = ((!curJob.bill.globalStyle) ? curJob.bill.style : Faction.OfPlayer.ideos.PrimaryIdeo.style.StyleForThingDef(curJob.bill.recipe.ProducedThingDef)?.styleDef);
			}
			List<Thing> list = ((curJob.bill is Bill_Mech bill) ? GenRecipe.FinalizeGestatedPawns(bill, actor, style).ToList() : GenRecipe.MakeRecipeProducts(curJob.RecipeDef, actor, ingredients, dominantIngredient, jobDriver_DoBill.BillGiver, curJob.bill.precept, style, curJob.bill.graphicIndexOverride).ToList());
			ConsumeIngredients(ingredients, curJob.RecipeDef, actor.Map);
			curJob.bill.Notify_IterationCompleted(actor, ingredients);
			RecordsUtility.Notify_BillDone(actor, list);
			if (curJob?.bill == null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (!GenPlace.TryPlaceThing(list[i], actor.Position, actor.Map, ThingPlaceMode.Near))
					{
						Log.Error(actor?.ToString() + " could not drop recipe product " + list[i]?.ToString() + " near " + actor.Position.ToString());
					}
				}
			}
			else
			{
				Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
				if (curJob.bill.recipe.WorkAmountTotal(thing) >= 10000f && list.Count > 0)
				{
					TaleRecorder.RecordTale(TaleDefOf.CompletedLongCraftingProject, actor, list[0].GetInnerIfMinified().def);
				}
				if (list.Any())
				{
					Find.QuestManager.Notify_ThingsProduced(actor, list);
				}
				if (list.Count == 0)
				{
					actor.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
				else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.DropOnFloor)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (!GenPlace.TryPlaceThing(list[j], actor.Position, actor.Map, ThingPlaceMode.Near))
						{
							Log.Error($"{actor} could not drop recipe product {list[j]} near {actor.Position}");
						}
					}
					actor.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
				else
				{
					if (list.Count > 1)
					{
						for (int k = 1; k < list.Count; k++)
						{
							if (!GenPlace.TryPlaceThing(list[k], actor.Position, actor.Map, ThingPlaceMode.Near))
							{
								Log.Error($"{actor} could not drop recipe product {list[k]} near {actor.Position}");
							}
						}
					}
					IntVec3 foundCell = IntVec3.Invalid;
					if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.BestStockpile)
					{
						StoreUtility.TryFindBestBetterStoreCellFor(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell);
					}
					else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.SpecificStockpile)
					{
						StoreUtility.TryFindBestBetterStoreCellForIn(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetSlotGroup(), out foundCell);
					}
					else
					{
						Log.ErrorOnce("Unknown store mode", 9158246);
					}
					if (foundCell.IsValid)
					{
						int num = actor.carryTracker.MaxStackSpaceEver(list[0].def);
						if (num < list[0].stackCount)
						{
							int count = list[0].stackCount - num;
							Thing thing2 = list[0].SplitOff(count);
							if (!GenPlace.TryPlaceThing(thing2, actor.Position, actor.Map, ThingPlaceMode.Near))
							{
								Log.Error($"{actor} could not drop recipe extra product that pawn couldn't carry, {thing2} near {actor.Position}");
							}
						}
						if (num == 0)
						{
							actor.jobs.EndCurrentJob(JobCondition.Succeeded);
						}
						else
						{
							actor.carryTracker.TryStartCarry(list[0]);
							Pawn_JobTracker jobs = actor.jobs;
							Job newJob = HaulAIUtility.HaulToCellStorageJob(actor, list[0], foundCell, fitInStoreCell: false);
							bool? keepCarryingThingOverride = true;
							jobs.StartJob(newJob, JobCondition.Succeeded, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, keepCarryingThingOverride);
						}
					}
					else
					{
						if (!GenPlace.TryPlaceThing(list[0], actor.Position, actor.Map, ThingPlaceMode.Near))
						{
							Log.Error($"Bill doer could not drop product {list[0]} near {actor.Position}");
						}
						actor.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				}
			}
		};
		return toil;
	}

	private static List<Thing> CalculateIngredients(Job job, Pawn actor)
	{
		if (job.GetTarget(TargetIndex.B).Thing is UnfinishedThing unfinishedThing)
		{
			List<Thing> ingredients = unfinishedThing.ingredients;
			job.RecipeDef.Worker.ConsumeIngredient(unfinishedThing, job.RecipeDef, actor.Map);
			job.placedThings = null;
			return ingredients;
		}
		List<Thing> list = new List<Thing>();
		if (job.placedThings != null)
		{
			for (int i = 0; i < job.placedThings.Count; i++)
			{
				if (job.placedThings[i].Count <= 0)
				{
					Log.Error("PlacedThing " + job.placedThings[i]?.ToString() + " with count " + job.placedThings[i].Count + " for job " + job);
					continue;
				}
				Thing thing = ((job.placedThings[i].Count >= job.placedThings[i].thing.stackCount) ? job.placedThings[i].thing : job.placedThings[i].thing.SplitOff(job.placedThings[i].Count));
				if (job.bill is Bill_Medical bill_Medical && thing.def.IsMedicine)
				{
					if (bill_Medical.consumedMedicine == null)
					{
						bill_Medical.consumedMedicine = new Dictionary<ThingDef, int>();
					}
					bill_Medical.consumedMedicine.TryGetValue(thing.def, out var value);
					bill_Medical.consumedMedicine[thing.def] = value + thing.stackCount;
				}
				job.placedThings[i].Count = 0;
				if (list.Contains(thing))
				{
					Log.Error("Tried to add ingredient from job placed targets twice: " + thing);
					continue;
				}
				list.Add(thing);
				if (job.RecipeDef.autoStripCorpses && thing is IStrippable strippable && strippable.AnythingToStrip())
				{
					strippable.Strip();
				}
			}
		}
		job.placedThings = null;
		return list;
	}

	private static Thing CalculateDominantIngredient(Job job, List<Thing> ingredients)
	{
		Thing thing = job.GetTarget(TargetIndex.B).Thing;
		UnfinishedThing uft = thing as UnfinishedThing;
		if (uft != null && uft.def.MadeFromStuff)
		{
			return uft.ingredients.First((Thing ing) => ing.def == uft.Stuff);
		}
		if (!ingredients.NullOrEmpty())
		{
			RecipeDef recipeDef = job.RecipeDef;
			if (recipeDef.productHasIngredientStuff)
			{
				return ingredients[0];
			}
			if (recipeDef.products.Any((ThingDefCountClass x) => x.thingDef.MadeFromStuff) || (recipeDef.unfinishedThingDef != null && recipeDef.unfinishedThingDef.MadeFromStuff))
			{
				return ingredients.Where((Thing x) => x.def.IsStuff).RandomElementByWeight((Thing x) => x.stackCount);
			}
			return ingredients.RandomElementByWeight((Thing x) => x.stackCount);
		}
		return null;
	}

	private static void ConsumeIngredients(List<Thing> ingredients, RecipeDef recipe, Map map)
	{
		for (int i = 0; i < ingredients.Count; i++)
		{
			recipe.Worker.ConsumeIngredient(ingredients[i], recipe, map);
		}
	}

	public static Toil CheckIfRecipeCanFinishNow()
	{
		Toil toil = ToilMaker.MakeToil("CheckIfRecipeCanFinishNow");
		toil.initAction = delegate
		{
			if (!toil.actor.jobs.curJob.bill.CanFinishNow)
			{
				toil.actor.jobs.EndCurrentJob(JobCondition.Succeeded);
			}
		};
		return toil;
	}
}
