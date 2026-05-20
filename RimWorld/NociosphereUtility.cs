using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class NociosphereUtility
{
	public const float NociosphereLOS = 24.9f;

	public static readonly int NociosphereLOSSqr = Mathf.FloorToInt(620.01f);

	private static readonly List<Thing> targetsTmp = new List<Thing>();

	public static Thing FindTarget(Pawn pawn)
	{
		List<Thing> source = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
		CheckForTargets(pawn, source, targetsTmp, IsPawnTarget);
		if (targetsTmp.Empty())
		{
			List<Thing> source2 = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
			CheckForTargets(pawn, source2, targetsTmp, IsBuildingTarget);
		}
		Thing result = null;
		if (targetsTmp.Any())
		{
			result = targetsTmp.RandomElement();
		}
		targetsTmp.Clear();
		return result;
	}

	public static void SkipTo(Pawn pawn, IntVec3 cell)
	{
		Ability ability = pawn.abilities.GetAbility(AbilityDefOf.EntitySkip);
		ability.ResetCooldown();
		if (pawn.IsOnHoldingPlatform)
		{
			Building_HoldingPlatform building_HoldingPlatform = (Building_HoldingPlatform)pawn.ParentHolder;
			building_HoldingPlatform.innerContainer.TryDrop(pawn, building_HoldingPlatform.Position, building_HoldingPlatform.Map, ThingPlaceMode.Direct, 1, out var _);
			CompHoldingPlatformTarget compHoldingPlatformTarget = pawn.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget != null)
			{
				compHoldingPlatformTarget.targetHolder = null;
			}
		}
		Job job = ability.GetJob(pawn, cell);
		pawn.jobs.StartJob(job, JobCondition.InterruptForced);
	}

	private static void CheckForTargets(Pawn pawn, List<Thing> source, List<Thing> output, Func<Thing, bool> validator)
	{
		output.Clear();
		for (int i = 0; i < source.Count; i++)
		{
			if (pawn.Position.DistanceToSquared(source[i].Position) <= NociosphereLOSSqr && validator(source[i]) && GenSight.LineOfSightToThing(pawn.Position, source[i], pawn.Map, skipFirstCell: true))
			{
				output.Add(source[i]);
			}
		}
	}

	private static bool IsPawnTarget(Thing thing)
	{
		if (thing is Pawn { Downed: false } pawn)
		{
			return pawn.kindDef != PawnKindDefOf.Nociosphere;
		}
		return false;
	}

	private static bool IsBuildingTarget(Thing thing)
	{
		if (thing is Building building)
		{
			return building.def.building.IsTurret;
		}
		return false;
	}
}
