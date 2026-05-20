using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_TakeToPen : WorkGiver_InteractAnimal
{
	protected bool targetRoamingAnimals;

	protected bool allowUnenclosedPens;

	protected RopingPriority ropingPriority;

	private readonly Dictionary<Map, AnimalPenBalanceCalculator> balanceCalculatorsCached = new Dictionary<Map, AnimalPenBalanceCalculator>();

	private int balanceCalculatorsCachedTick = -1;

	private Pawn balanceCalculatorsCachedForPawn;

	public WorkGiver_TakeToPen()
	{
		canInteractWhileSleeping = true;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn { IsAnimal: not false } pawn2))
		{
			return null;
		}
		bool flag = pawn2.MentalStateDef == MentalStateDefOf.Roaming;
		if (targetRoamingAnimals && !flag)
		{
			return null;
		}
		if (!targetRoamingAnimals && !flag && pawn2.MentalStateDef != null)
		{
			JobFailReason.Is("CantRopeAnimalMentalState".Translate(pawn2, pawn2.MentalStateDef.label));
			return null;
		}
		if (pawn2.Position.IsForbidden(pawn))
		{
			JobFailReason.Is(string.Format("{0} ({1})", "ForbiddenOutsideAllowedAreaLower".Translate().CapitalizeFirst(), pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.Label));
			return null;
		}
		if (t.Map.designationManager.DesignationOn(t, DesignationDefOf.ReleaseAnimalToWild) != null)
		{
			return null;
		}
		Map map = pawn2.Map;
		AnimalPenBalanceCalculator animalPenBalanceCalculator = null;
		if (balanceCalculatorsCachedTick != Find.TickManager.TicksGame || balanceCalculatorsCachedForPawn != pawn)
		{
			foreach (KeyValuePair<Map, AnimalPenBalanceCalculator> item in balanceCalculatorsCached)
			{
				item.Value.MarkDirty();
			}
			balanceCalculatorsCachedTick = Find.TickManager.TicksGame;
			balanceCalculatorsCachedForPawn = pawn;
		}
		if (balanceCalculatorsCached.ContainsKey(map))
		{
			animalPenBalanceCalculator = balanceCalculatorsCached[map];
		}
		else
		{
			animalPenBalanceCalculator = new AnimalPenBalanceCalculator(map, considerInProgressMovement: true);
			balanceCalculatorsCached.Add(map, animalPenBalanceCalculator);
		}
		string jobFailReason = null;
		CompAnimalPenMarker penAnimalShouldBeTakenTo = AnimalPenUtility.GetPenAnimalShouldBeTakenTo(pawn, pawn2, out jobFailReason, forced, canInteractWhileSleeping, allowUnenclosedPens: false, ignoreSkillRequirements: true, ropingPriority, animalPenBalanceCalculator);
		if (penAnimalShouldBeTakenTo != null)
		{
			Job job = MakeJob(pawn, pawn2, penAnimalShouldBeTakenTo, allowUnenclosedPens: false, ropingPriority, out jobFailReason);
			if (job != null)
			{
				return job;
			}
		}
		string jobFailReason2 = null;
		Building building = null;
		building = AnimalPenUtility.GetHitchingPostAnimalShouldBeTakenTo(pawn, pawn2, out jobFailReason2, forced);
		if (building != null)
		{
			return JobMaker.MakeJob(JobDefOf.RopeRoamerToHitchingPost, pawn2, building);
		}
		if (allowUnenclosedPens)
		{
			penAnimalShouldBeTakenTo = AnimalPenUtility.GetPenAnimalShouldBeTakenTo(pawn, pawn2, out jobFailReason, forced, canInteractWhileSleeping, allowUnenclosedPens: true, ignoreSkillRequirements: true, ropingPriority, animalPenBalanceCalculator);
			if (penAnimalShouldBeTakenTo != null)
			{
				Job job2 = MakeJob(pawn, pawn2, penAnimalShouldBeTakenTo, allowUnenclosedPens: true, ropingPriority, out jobFailReason);
				if (job2 != null)
				{
					return job2;
				}
			}
		}
		if (penAnimalShouldBeTakenTo == null && building == null && AnimalPenUtility.IsUnnecessarilyRoped(pawn2))
		{
			Job job3 = MakeUnropeJob(pawn, pawn2, forced, out jobFailReason);
			if (job3 != null)
			{
				return job3;
			}
		}
		if (jobFailReason != null)
		{
			JobFailReason.Is(jobFailReason);
		}
		else if (jobFailReason2 != null)
		{
			JobFailReason.Is(jobFailReason2);
		}
		return null;
	}

	private Job MakeUnropeJob(Pawn roper, Pawn animal, bool forced, out string jobFailReason)
	{
		jobFailReason = null;
		if (AnimalPenUtility.RopeAttachmentInteractionCell(roper, animal) == IntVec3.Invalid)
		{
			jobFailReason = "CantRopeAnimalCantTouch".Translate();
			return null;
		}
		if (!forced && !roper.CanReserve(animal))
		{
			return null;
		}
		if (!roper.CanReach(animal, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.Unrope, animal);
	}

	public static Job MakeJob(Pawn pawn, Pawn animal, CompAnimalPenMarker targetPenMarker, bool allowUnenclosedPens, RopingPriority ropingPriority, out string jobFailReason)
	{
		jobFailReason = null;
		IntVec3 intVec = AnimalPenUtility.FindPlaceInPenToStand(targetPenMarker, pawn);
		if (!intVec.IsValid)
		{
			jobFailReason = "CantRopeAnimalNoSpace".Translate();
			return null;
		}
		Job job = JobMaker.MakeJob(targetPenMarker.PenState.Enclosed ? JobDefOf.RopeToPen : JobDefOf.RopeRoamerToUnenclosedPen, animal, intVec, targetPenMarker.parent);
		job.ropingPriority = ropingPriority;
		job.ropeToUnenclosedPens = allowUnenclosedPens;
		return job;
	}
}
