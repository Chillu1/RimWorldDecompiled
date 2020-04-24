using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_LayDown
	{
		private const int TicksBetweenSleepZs = 100;

		private const int GetUpOrStartJobWhileInBedCheckInterval = 211;

		public static Toil LayDown(TargetIndex bedOrRestSpotIndex, bool hasBed, bool lookForOtherJobs, bool canSleep = true, bool gainRestAndHealth = true)
		{
			Toil layDown = new Toil();
			layDown.initAction = delegate
			{
				Pawn actor3 = layDown.actor;
				actor3.pather.StopDead();
				JobDriver curDriver3 = actor3.jobs.curDriver;
				if (hasBed)
				{
					if (!((Building_Bed)actor3.CurJob.GetTarget(bedOrRestSpotIndex).Thing).OccupiedRect().Contains(actor3.Position))
					{
						Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor3);
						actor3.jobs.EndCurrentJob(JobCondition.Errored);
						return;
					}
					actor3.jobs.posture = PawnPosture.LayingInBed;
				}
				else
				{
					actor3.jobs.posture = PawnPosture.LayingOnGroundNormal;
				}
				curDriver3.asleep = false;
				if (actor3.mindState.applyBedThoughtsTick == 0)
				{
					actor3.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
					actor3.mindState.applyBedThoughtsOnLeave = false;
				}
				if (actor3.ownership != null && actor3.CurrentBed() != actor3.ownership.OwnedBed)
				{
					ThoughtUtility.RemovePositiveBedroomThoughts(actor3);
				}
				actor3.GetComp<CompCanBeDormant>()?.ToSleep();
			};
			layDown.tickAction = delegate
			{
				Pawn actor2 = layDown.actor;
				Job curJob = actor2.CurJob;
				JobDriver curDriver2 = actor2.jobs.curDriver;
				Building_Bed building_Bed = (Building_Bed)curJob.GetTarget(bedOrRestSpotIndex).Thing;
				actor2.GainComfortFromCellIfPossible();
				if (!curDriver2.asleep)
				{
					if (canSleep && ((actor2.needs.rest != null && actor2.needs.rest.CurLevel < RestUtility.FallAsleepMaxLevel(actor2)) || curJob.forceSleep))
					{
						curDriver2.asleep = true;
					}
				}
				else if (!canSleep)
				{
					curDriver2.asleep = false;
				}
				else if ((actor2.needs.rest == null || actor2.needs.rest.CurLevel >= RestUtility.WakeThreshold(actor2)) && !curJob.forceSleep)
				{
					curDriver2.asleep = false;
				}
				if (curDriver2.asleep && gainRestAndHealth && actor2.needs.rest != null)
				{
					float restEffectiveness = (building_Bed == null || !building_Bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness)) ? StatDefOf.BedRestEffectiveness.valueIfMissing : building_Bed.GetStatValue(StatDefOf.BedRestEffectiveness);
					actor2.needs.rest.TickResting(restEffectiveness);
				}
				if (actor2.mindState.applyBedThoughtsTick != 0 && actor2.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
				{
					ApplyBedThoughts(actor2);
					actor2.mindState.applyBedThoughtsTick += 60000;
					actor2.mindState.applyBedThoughtsOnLeave = true;
				}
				if (actor2.IsHashIntervalTick(100) && !actor2.Position.Fogged(actor2.Map))
				{
					if (curDriver2.asleep)
					{
						MoteMaker.ThrowMetaIcon(actor2.Position, actor2.Map, ThingDefOf.Mote_SleepZ);
					}
					if (gainRestAndHealth && actor2.health.hediffSet.GetNaturallyHealingInjuredParts().Any())
					{
						MoteMaker.ThrowMetaIcon(actor2.Position, actor2.Map, ThingDefOf.Mote_HealingCross);
					}
				}
				if (actor2.ownership != null && building_Bed != null && !building_Bed.Medical && !building_Bed.OwnersForReading.Contains(actor2))
				{
					if (actor2.Downed)
					{
						actor2.Position = CellFinder.RandomClosewalkCellNear(actor2.Position, actor2.Map, 1);
					}
					actor2.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else if (lookForOtherJobs && actor2.IsHashIntervalTick(211))
				{
					actor2.jobs.CheckForJobOverride();
				}
			};
			layDown.defaultCompleteMode = ToilCompleteMode.Never;
			if (hasBed)
			{
				layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
			}
			layDown.AddFinishAction(delegate
			{
				Pawn actor = layDown.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				if (actor.mindState.applyBedThoughtsOnLeave)
				{
					ApplyBedThoughts(actor);
				}
				curDriver.asleep = false;
			});
			return layDown;
		}

		private static void ApplyBedThoughts(Pawn actor)
		{
			if (actor.needs.mood == null)
			{
				return;
			}
			Building_Bed building_Bed = actor.CurrentBed();
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
			if (actor.GetRoom().PsychologicallyOutdoors)
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside);
			}
			if (building_Bed == null || building_Bed.CostListAdjusted().Count == 0)
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround);
			}
			if (actor.AmbientTemperature < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold);
			}
			if (actor.AmbientTemperature > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat);
			}
			if (building_Bed == null || building_Bed != actor.ownership.OwnedBed || building_Bed.ForPrisoners || actor.story.traits.HasTrait(TraitDefOf.Ascetic))
			{
				return;
			}
			ThoughtDef thoughtDef = null;
			if (building_Bed.GetRoom().Role == RoomRoleDefOf.Bedroom)
			{
				thoughtDef = ThoughtDefOf.SleptInBedroom;
			}
			else if (building_Bed.GetRoom().Role == RoomRoleDefOf.Barracks)
			{
				thoughtDef = ThoughtDefOf.SleptInBarracks;
			}
			if (thoughtDef != null)
			{
				int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
				if (thoughtDef.stages[scoreStageIndex] != null)
				{
					actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
				}
			}
		}
	}
}
