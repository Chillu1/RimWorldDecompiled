using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_EnterTransporter : ThinkNode_JobGiver
{
	private static List<CompTransporter> tmpTransporters = new List<CompTransporter>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		int transportersGroup = pawn.mindState.duty.transportersGroup;
		if (transportersGroup != -1)
		{
			IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i] != pawn && allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
				{
					CompTransporter transporter = ((JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
					if (transporter != null && transporter.groupID == transportersGroup)
					{
						return null;
					}
				}
			}
			TransporterUtility.GetTransportersInGroup(transportersGroup, pawn.Map, tmpTransporters);
			CompTransporter compTransporter = FindMyTransporter(tmpTransporters, pawn);
			tmpTransporters.Clear();
			if (compTransporter == null || !pawn.CanReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.EnterTransporter, compTransporter.parent);
		}
		Thing thing = pawn.mindState.duty.focus.Thing;
		if (thing == null || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, pawn.mindState.duty.focus.Thing);
		job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, LocomotionUrgency.Walk);
		return job;
	}

	public static CompTransporter FindMyTransporter(List<CompTransporter> transporters, Pawn me)
	{
		if (transporters == null)
		{
			return null;
		}
		for (int i = 0; i < transporters.Count; i++)
		{
			List<TransferableOneWay> leftToLoad = transporters[i].leftToLoad;
			if (leftToLoad == null)
			{
				continue;
			}
			for (int j = 0; j < leftToLoad.Count; j++)
			{
				if (!(leftToLoad[j].AnyThing is Pawn))
				{
					continue;
				}
				List<Thing> things = leftToLoad[j].things;
				for (int k = 0; k < things.Count; k++)
				{
					if (things[k] == me)
					{
						return transporters[i];
					}
				}
			}
		}
		return null;
	}
}
