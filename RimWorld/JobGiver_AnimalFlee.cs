using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AnimalFlee : ThinkNode_JobGiver
{
	private const int FleeDistance = 24;

	private const int DistToDangerToFlee = 18;

	private const int DistToFireToFlee = 10;

	private const int MinFiresNearbyToFlee = 60;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse)
		{
			return null;
		}
		if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn))
		{
			return null;
		}
		if (pawn.Faction == null)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
			for (int i = 0; i < list.Count; i++)
			{
				if (pawn.Position.InHorDistOf(list[i].Position, 18f) && FleeUtility.ShouldFleeFrom(list[i], pawn, checkDistance: false, checkLOS: false))
				{
					Job job = FleeUtility.FleeJob(pawn, list[i], 24);
					if (job != null)
					{
						return job;
					}
				}
			}
			Job job2 = FleeUtility.FleeLargeFireJob(pawn, 60, 10, 24);
			if (job2 != null)
			{
				return job2;
			}
		}
		else if (FleeUtility.ShouldAnimalFleeDanger(pawn))
		{
			List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
			for (int j = 0; j < potentialTargetsFor.Count; j++)
			{
				Thing thing = potentialTargetsFor[j].Thing;
				if (pawn.Position.InHorDistOf(thing.Position, 18f) && FleeUtility.ShouldFleeFrom(thing, pawn, checkDistance: false, checkLOS: true) && (!(thing is Pawn pawn2) || !pawn2.AnimalOrWildMan() || pawn2.Faction != null))
				{
					Job job3 = FleeUtility.FleeJob(pawn, thing, 24);
					if (job3 != null)
					{
						return job3;
					}
				}
			}
		}
		return null;
	}
}
