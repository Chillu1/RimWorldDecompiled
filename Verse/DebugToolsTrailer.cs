using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse.AI;

namespace Verse;

public static class DebugToolsTrailer
{
	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000, requiresOdyssey = true)]
	private static void SwimAt(Pawn pawn)
	{
		DebugTools.curTool = new DebugTool("Select destination...", delegate
		{
			if (SwimPathFinder.TryFindSwimPath(pawn, UI.MouseCell(), out var result))
			{
				pawn.needs.joy.CurLevelPercentage = 0.3f;
				Job job = JobMaker.MakeJob(JobDefOf.GoSwimming, result[0]);
				job.locomotionUrgency = LocomotionUrgency.Walk;
				job.targetQueueA = new List<LocalTargetInfo>();
				for (int i = 1; i < result.Count; i++)
				{
					job.targetQueueA.Add(result[i]);
				}
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				DebugTools.curTool = null;
			}
		});
	}

	[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
	private static void MeleeAttackTarget(Pawn p)
	{
		DebugTools.curTool = new DebugTool("Select target...", delegate
		{
			Pawn pawn = PawnAt(UI.MouseCell());
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee);
			job.targetA = pawn;
			p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		});
		static Pawn PawnAt(IntVec3 c)
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(c))
			{
				if (item is Pawn result)
				{
					return result;
				}
			}
			return null;
		}
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000, requiresOdyssey = true)]
	private static void VolcanicDebris()
	{
		SkyfallerMaker.SpawnSkyfaller(ThingDefOf.LavaRockIncoming, UI.MouseCell(), Find.CurrentMap);
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000, requiresOdyssey = true)]
	private static List<DebugActionNode> VolcanicDebrisDelayed()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		for (int i = 0; i < 41; i++)
		{
			int delay = i * 30;
			list.Add(new DebugActionNode(((float)delay / 60f).ToString("F1") + " seconds", DebugActionType.ToolMap, delegate
			{
				SkyfallerMaker.SpawnSkyfaller(ThingDefOf.LavaRockIncoming, UI.MouseCell(), Find.CurrentMap).ticksToImpact += delay;
			}));
		}
		return list;
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000, requiresOdyssey = true)]
	private static void NuzzlePawn(Pawn animal)
	{
		if (!animal.IsAnimal)
		{
			Messages.Message("Must select an animal", MessageTypeDefOf.RejectInput);
			return;
		}
		DebugTools.curTool = new DebugTool("Select target...", delegate
		{
			Pawn pawn = animal.Map.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
			if (pawn != null && pawn.RaceProps.Humanlike)
			{
				pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Wait, 3000), JobTag.Misc);
				Job job = JobMaker.MakeJob(JobDefOf.Nuzzle, pawn);
				job.locomotionUrgency = LocomotionUrgency.Walk;
				job.expiryInterval = 3000;
				animal.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				DebugTools.curTool = null;
			}
		});
	}
}
