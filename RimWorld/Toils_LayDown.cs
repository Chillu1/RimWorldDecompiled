using Verse;
using Verse.AI;

namespace RimWorld;

public static class Toils_LayDown
{
	private const int TicksBetweenSleepZs = 100;

	private const int GetUpOrStartJobWhileInBedCheckInterval = 211;

	private const int SlavesInSleepingRoomCheckInterval = 2500;

	private const float VelocityForAdults = 0.42f;

	private const float VelocityForBabies = 0.25f;

	private const float VelocityForChildren = 0.33f;

	public static Toil LayDown(TargetIndex bedOrRestSpotIndex, bool hasBed, bool lookForOtherJobs, bool canSleep = true, bool gainRestAndHealth = true, PawnPosture noBedLayingPosture = PawnPosture.LayingOnGroundNormal, bool deathrest = false)
	{
		Toil layDown = ToilMaker.MakeToil("LayDown");
		layDown.initAction = delegate
		{
			Pawn actor = layDown.actor;
			actor.pather?.StopDead();
			if (hasBed)
			{
				Building_Bed building_Bed = (Building_Bed)actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing;
				if (!building_Bed.OccupiedRect().Contains(actor.Position))
				{
					Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor);
					actor.jobs.EndCurrentJob(JobCondition.Errored);
					return;
				}
				actor.jobs.posture = PawnPosture.LayingInBed;
				actor.mindState.lastBedDefSleptIn = building_Bed.def;
				PortraitsCache.SetDirty(actor);
			}
			else
			{
				actor.jobs.posture = noBedLayingPosture;
				actor.mindState.lastBedDefSleptIn = null;
			}
			if (actor.mindState.applyBedThoughtsTick == 0)
			{
				actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
				actor.mindState.applyBedThoughtsOnLeave = false;
			}
			if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed && !deathrest)
			{
				ThoughtUtility.RemovePositiveBedroomThoughts(actor);
			}
		};
		layDown.tickAction = delegate
		{
			Pawn actor = layDown.actor;
			Job curJob = actor.CurJob;
			JobDriver curDriver = actor.jobs.curDriver;
			Building_Bed bed = curJob.GetTarget(bedOrRestSpotIndex).Thing as Building_Bed;
			if (!curDriver.asleep)
			{
				if (canSleep && (RestUtility.CanFallAsleep(actor) || curJob.forceSleep) && (actor.ageTracker.CurLifeStage.canVoluntarilySleep || curJob.startInvoluntarySleep))
				{
					curDriver.asleep = true;
					curJob.startInvoluntarySleep = false;
				}
			}
			else if (!canSleep || (RestUtility.ShouldWakeUp(actor) && !curJob.forceSleep))
			{
				curDriver.asleep = false;
			}
			ApplyBedRelatedEffects(actor, bed, curDriver.asleep, gainRestAndHealth, 1);
			if (lookForOtherJobs && actor.IsHashIntervalTick(211))
			{
				actor.jobs.CheckForJobOverride();
			}
		};
		layDown.defaultCompleteMode = ToilCompleteMode.Never;
		if (hasBed)
		{
			layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
		}
		layDown.AddFinishAction(delegate
		{
			FinalizeLayingJob(layDown.actor, hasBed ? ((Building_Bed)layDown.actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing) : null, deathrest);
		});
		return layDown;
	}

	private static void ApplyBedRelatedEffects(Pawn p, Building_Bed bed, bool asleep, bool gainRest, int delta)
	{
		p.GainComfortFromCellIfPossible(delta);
		if (asleep && gainRest && p.needs.rest != null)
		{
			float restEffectiveness = ((bed == null || !bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness)) ? StatDefOf.BedRestEffectiveness.valueIfMissing : bed.GetStatValue(StatDefOf.BedRestEffectiveness, applyPostProcess: true, 15));
			p.needs.rest.TickResting(restEffectiveness);
		}
		if (p.IsHashIntervalTick(100, delta))
		{
			Thing spawnedParentOrMe = p.SpawnedParentOrMe;
			if (spawnedParentOrMe != null && !spawnedParentOrMe.Position.Fogged(spawnedParentOrMe.Map))
			{
				if (asleep && !p.RaceProps.IsMechanoid)
				{
					FleckDef fleckDef = FleckDefOf.SleepZ;
					float velocitySpeed = 0.42f;
					if (p.ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Baby || p.ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Newborn)
					{
						fleckDef = FleckDefOf.SleepZ_Tiny;
						velocitySpeed = 0.25f;
					}
					else if (p.ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Child)
					{
						fleckDef = FleckDefOf.SleepZ_Small;
						velocitySpeed = 0.33f;
					}
					FleckMaker.ThrowMetaIcon(spawnedParentOrMe.Position, spawnedParentOrMe.Map, fleckDef, velocitySpeed);
				}
				if (gainRest && p.health.hediffSet.GetNaturallyHealingInjuredParts().Any())
				{
					FleckMaker.ThrowMetaIcon(spawnedParentOrMe.Position, spawnedParentOrMe.Map, FleckDefOf.HealingCross);
				}
			}
		}
		if (p.mindState.applyBedThoughtsTick != 0 && p.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
		{
			ApplyBedThoughts(p, bed);
			p.mindState.applyBedThoughtsTick += 60000;
			p.mindState.applyBedThoughtsOnLeave = true;
		}
		if (!ModsConfig.IdeologyActive || bed == null || !p.IsHashIntervalTick(2500, delta) || p.Awake() || (!p.IsFreeColonist && !p.IsPrisonerOfColony) || p.IsSlaveOfColony)
		{
			return;
		}
		Room room = bed.GetRoom();
		if (room.PsychologicallyOutdoors)
		{
			return;
		}
		bool flag = false;
		foreach (Building_Bed containedBed in room.ContainedBeds)
		{
			foreach (Pawn curOccupant in containedBed.CurOccupants)
			{
				if (curOccupant != p && !curOccupant.Awake() && curOccupant.IsSlave && !LovePartnerRelationUtility.LovePartnerRelationExists(p, curOccupant))
				{
					p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInRoomWithSlave);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	private static void FinalizeLayingJob(Pawn pawn, Building_Bed bed, bool deathrest)
	{
		if (pawn.mindState.applyBedThoughtsOnLeave)
		{
			ApplyBedThoughts(pawn, bed);
		}
		if (deathrest)
		{
			UpdateDeathrestThoughtIndex(pawn);
		}
	}

	private static void ApplyBedThoughts(Pawn actor, Building_Bed bed)
	{
		if (actor.needs.mood == null)
		{
			return;
		}
		actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
		actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
		actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
		actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
		actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
		actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
		float ambientTemperature = actor.AmbientTemperature;
		float num = actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin);
		float num2 = actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax);
		if (ModsConfig.BiotechActive && actor.genes != null)
		{
			foreach (Gene item in actor.genes.GenesListForReading)
			{
				if (item.Active)
				{
					num += item.def.statOffsets.GetStatOffsetFromList(StatDefOf.ComfyTemperatureMin);
					num2 += item.def.statOffsets.GetStatOffsetFromList(StatDefOf.ComfyTemperatureMax);
				}
			}
		}
		if (ambientTemperature < num)
		{
			actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold);
		}
		if (ambientTemperature > num2)
		{
			actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat);
		}
		if (!actor.IsWildMan())
		{
			if (actor.GetRoom().PsychologicallyOutdoors)
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside);
			}
			if (bed == null || bed.CostListAdjusted().Count == 0)
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround);
			}
		}
		if (bed != null && bed == actor.ownership.OwnedBed && !bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
		{
			Room room = bed.GetRoom();
			if (room != null)
			{
				ThoughtDef thoughtDef = null;
				if (room.Role == RoomRoleDefOf.Bedroom)
				{
					thoughtDef = ThoughtDefOf.SleptInBedroom;
				}
				else if (room.Role == RoomRoleDefOf.Barracks)
				{
					thoughtDef = ThoughtDefOf.SleptInBarracks;
				}
				if (thoughtDef != null)
				{
					int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
					if (thoughtDef.stages[scoreStageIndex] != null)
					{
						actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
					}
				}
			}
		}
		actor.Notify_AddBedThoughts();
	}

	private static void UpdateDeathrestThoughtIndex(Pawn actor)
	{
		if (actor.needs.mood == null || !ModsConfig.BiotechActive)
		{
			return;
		}
		Gene_Deathrest gene_Deathrest = actor.genes?.GetFirstGeneOfType<Gene_Deathrest>();
		if (gene_Deathrest != null)
		{
			Room room = actor.GetRoom();
			if (room == null)
			{
				gene_Deathrest.chamberThoughtIndex = -1;
			}
			else if (!actor.IsWildMan() && room.PsychologicallyOutdoors)
			{
				gene_Deathrest.chamberThoughtIndex = 0;
			}
			else if (actor.story.traits.HasTrait(TraitDefOf.Ascetic))
			{
				gene_Deathrest.chamberThoughtIndex = -1;
			}
			else
			{
				gene_Deathrest.chamberThoughtIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness)) + 1;
			}
		}
	}

	public static Toil SelfShutdown()
	{
		Toil layDown = ToilMaker.MakeToil("SelfShutdown");
		layDown.initAction = delegate
		{
			Pawn actor = layDown.actor;
			actor.pather?.StopDead();
			JobDriver curDriver = actor.jobs.curDriver;
			actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
			actor.mindState.lastBedDefSleptIn = null;
			curDriver.asleep = true;
		};
		layDown.defaultCompleteMode = ToilCompleteMode.Never;
		layDown.AddFinishAction(delegate
		{
			layDown.actor.jobs.curDriver.asleep = false;
		});
		return layDown;
	}

	public static Toil ActivityDormant()
	{
		Toil toil = ToilMaker.MakeToil("ActivityDormant");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			actor.pather?.StopDead();
			JobDriver curDriver = actor.jobs.curDriver;
			actor.jobs.posture = PawnPosture.Standing;
			actor.GetComp<CompCanBeDormant>()?.ToSleep();
			curDriver.asleep = true;
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.AddFinishAction(delegate
		{
			toil.actor.jobs.curDriver.asleep = false;
		});
		toil.AddEndCondition(() => (!toil.actor.GetComp<CompActivity>().IsActive) ? JobCondition.Ongoing : JobCondition.InterruptForced);
		return toil;
	}
}
