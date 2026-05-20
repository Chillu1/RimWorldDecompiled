using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Work : ThinkNode
{
	public bool emergency;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_Work obj = (JobGiver_Work)base.DeepCopy(resolve);
		obj.emergency = emergency;
		return obj;
	}

	public override float GetPriority(Pawn pawn)
	{
		if (pawn.workSettings == null || !pawn.workSettings.EverWork)
		{
			return 0f;
		}
		TimeAssignmentDef timeAssignmentDef = ((pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment);
		if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
		{
			return 5.5f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Work)
		{
			return 9f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
		{
			return 3f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
		{
			return 2f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Meditate)
		{
			return 2f;
		}
		throw new NotImplementedException();
	}

	public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
	{
		if (pawn.RaceProps.Humanlike && pawn.health.hediffSet.InLabor())
		{
			return ThinkResult.NoJob;
		}
		if (emergency && pawn.mindState.priorityWork.IsPrioritized)
		{
			List<WorkGiverDef> workGiversByPriority = pawn.mindState.priorityWork.WorkGiver.workType.workGiversByPriority;
			for (int i = 0; i < workGiversByPriority.Count; i++)
			{
				WorkGiver worker = workGiversByPriority[i].Worker;
				if (!WorkGiversRelated(pawn.mindState.priorityWork.WorkGiver, worker.def))
				{
					continue;
				}
				Job job = GiverTryGiveJobPrioritized(pawn, worker, pawn.mindState.priorityWork.Cell);
				if (job != null)
				{
					job.playerForced = true;
					if (pawn.jobs.debugLog)
					{
						pawn.jobs.DebugLogEvent($"JobGiver_Work produced emergency Job {job.ToStringSafe()} from {worker}");
					}
					return new ThinkResult(job, this, workGiversByPriority[i].tagToGive);
				}
			}
			pawn.mindState.priorityWork.Clear();
		}
		List<WorkGiver> list = ((!emergency) ? pawn.workSettings.WorkGiversInOrderNormal : pawn.workSettings.WorkGiversInOrderEmergency);
		int num = -999;
		TargetInfo bestTargetOfLastPriority = TargetInfo.Invalid;
		WorkGiver_Scanner scannerWhoProvidedTarget = null;
		for (int j = 0; j < list.Count; j++)
		{
			WorkGiver workGiver = list[j];
			if (workGiver.def.priorityInType != num && bestTargetOfLastPriority.IsValid)
			{
				break;
			}
			if (!PawnCanUseWorkGiver(pawn, workGiver))
			{
				continue;
			}
			try
			{
				Job job2 = workGiver.NonScanJob(pawn);
				if (job2 != null)
				{
					if (pawn.jobs.debugLog)
					{
						pawn.jobs.DebugLogEvent($"JobGiver_Work produced non-scan Job {job2.ToStringSafe()} from {workGiver}");
					}
					return new ThinkResult(job2, this, list[j].def.tagToGive);
				}
				WorkGiver_Scanner scanner = workGiver as WorkGiver_Scanner;
				IntVec3 pawnPosition;
				float closestDistSquared;
				float bestPriority;
				bool prioritized;
				bool allowUnreachable;
				Danger maxPathDanger;
				if (scanner != null)
				{
					if (scanner.def.scanThings)
					{
						IEnumerable<Thing> enumerable = scanner.PotentialWorkThingsGlobal(pawn);
						bool flag = pawn.carryTracker?.CarriedThing != null && scanner.PotentialWorkThingRequest.Accepts(pawn.carryTracker.CarriedThing) && Validator(pawn.carryTracker.CarriedThing);
						Thing thing;
						if (scanner.Prioritized)
						{
							IEnumerable<Thing> searchSet = enumerable ?? pawn.Map.listerThings.ThingsMatching(scanner.PotentialWorkThingRequest);
							thing = ((!scanner.AllowUnreachable) ? GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchSet, scanner.PathEndMode, TraverseParms.For(pawn, scanner.MaxPathDanger(pawn)), 9999f, Validator, (Thing x) => scanner.GetPriority(pawn, x)) : GenClosest.ClosestThing_Global(pawn.Position, searchSet, 99999f, Validator, (Thing x) => scanner.GetPriority(pawn, x)));
							if (flag)
							{
								if (thing != null)
								{
									float num2 = scanner.GetPriority(pawn, pawn.carryTracker.CarriedThing);
									float num3 = scanner.GetPriority(pawn, thing);
									if (num2 >= num3)
									{
										thing = pawn.carryTracker.CarriedThing;
									}
								}
								else
								{
									thing = pawn.carryTracker.CarriedThing;
								}
							}
						}
						else if (flag)
						{
							thing = pawn.carryTracker.CarriedThing;
						}
						else if (scanner.AllowUnreachable)
						{
							IEnumerable<Thing> searchSet2 = enumerable ?? pawn.Map.listerThings.ThingsMatching(scanner.PotentialWorkThingRequest);
							thing = GenClosest.ClosestThing_Global(pawn.Position, searchSet2, 99999f, Validator);
						}
						else
						{
							thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest, scanner.PathEndMode, TraverseParms.For(pawn, scanner.MaxPathDanger(pawn)), 9999f, Validator, enumerable, 0, scanner.MaxRegionsToScanBeforeGlobalSearch, enumerable != null);
						}
						if (thing != null)
						{
							bestTargetOfLastPriority = thing;
							scannerWhoProvidedTarget = scanner;
						}
					}
					if (scanner.def.scanCells)
					{
						pawnPosition = pawn.Position;
						closestDistSquared = 99999f;
						bestPriority = float.MinValue;
						prioritized = scanner.Prioritized;
						allowUnreachable = scanner.AllowUnreachable;
						maxPathDanger = scanner.MaxPathDanger(pawn);
						IEnumerable<IntVec3> enumerable2 = scanner.PotentialWorkCellsGlobal(pawn);
						if (enumerable2 is IList<IntVec3> list2)
						{
							for (int num4 = 0; num4 < list2.Count; num4++)
							{
								ProcessCell(list2[num4]);
							}
						}
						else
						{
							foreach (IntVec3 item in enumerable2)
							{
								ProcessCell(item);
							}
						}
					}
				}
				void ProcessCell(IntVec3 c)
				{
					bool flag2 = false;
					float num5 = (c - pawnPosition).LengthHorizontalSquared;
					float num6 = 0f;
					if (prioritized)
					{
						if (!c.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, c))
						{
							if (!allowUnreachable && !pawn.CanReach(c, scanner.PathEndMode, maxPathDanger))
							{
								return;
							}
							num6 = scanner.GetPriority(pawn, c);
							if (num6 > bestPriority || (num6 == bestPriority && num5 < closestDistSquared))
							{
								flag2 = true;
							}
						}
					}
					else if (num5 < closestDistSquared && !c.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, c))
					{
						if (!allowUnreachable && !pawn.CanReach(c, scanner.PathEndMode, maxPathDanger))
						{
							return;
						}
						flag2 = true;
					}
					if (flag2)
					{
						bestTargetOfLastPriority = new TargetInfo(c, pawn.Map);
						scannerWhoProvidedTarget = scanner;
						closestDistSquared = num5;
						bestPriority = num6;
					}
				}
				bool Validator(Thing t)
				{
					if (!t.IsForbidden(pawn))
					{
						return scanner.HasJobOnThing(pawn, t);
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error(pawn?.ToString() + " threw exception in WorkGiver " + workGiver.def.defName + ": " + ex.ToString());
			}
			finally
			{
			}
			if (bestTargetOfLastPriority.IsValid)
			{
				Job job3 = ((!bestTargetOfLastPriority.HasThing) ? scannerWhoProvidedTarget.JobOnCell(pawn, bestTargetOfLastPriority.Cell) : scannerWhoProvidedTarget.JobOnThing(pawn, bestTargetOfLastPriority.Thing));
				if (job3 != null)
				{
					job3.workGiverDef = scannerWhoProvidedTarget.def;
					if (pawn.jobs.debugLog)
					{
						pawn.jobs.DebugLogEvent($"JobGiver_Work produced scan Job {job3.ToStringSafe()} from {scannerWhoProvidedTarget}");
					}
					return new ThinkResult(job3, this, list[j].def.tagToGive);
				}
				string[] obj = new string[6]
				{
					scannerWhoProvidedTarget?.ToString(),
					" provided target ",
					null,
					null,
					null,
					null
				};
				TargetInfo targetInfo = bestTargetOfLastPriority;
				obj[2] = targetInfo.ToString();
				obj[3] = " but yielded no actual job for pawn ";
				obj[4] = pawn?.ToString();
				obj[5] = ". The CanGiveJob and JobOnX methods may not be synchronized.";
				Log.ErrorOnce(string.Concat(obj), 6112651);
			}
			num = workGiver.def.priorityInType;
		}
		return ThinkResult.NoJob;
	}

	private bool PawnCanUseWorkGiver(Pawn pawn, WorkGiver giver)
	{
		if (!giver.def.nonColonistsCanDo && !pawn.IsColonist && !pawn.IsColonyMech && !pawn.IsColonySubhuman)
		{
			return false;
		}
		if (pawn.WorkTagIsDisabled(giver.def.workTags))
		{
			return false;
		}
		if (giver.def.workType != null && pawn.WorkTypeIsDisabled(giver.def.workType))
		{
			return false;
		}
		if (giver.ShouldSkip(pawn))
		{
			return false;
		}
		if (giver.MissingRequiredCapacity(pawn) != null)
		{
			return false;
		}
		if (pawn.RaceProps.IsMechanoid && !giver.def.canBeDoneByMechs)
		{
			return false;
		}
		return true;
	}

	private bool WorkGiversRelated(WorkGiverDef current, WorkGiverDef next)
	{
		if (next != WorkGiverDefOf.Repair || current == WorkGiverDefOf.Repair)
		{
			return next.doesSmoothing == current.doesSmoothing;
		}
		return false;
	}

	private Job GiverTryGiveJobPrioritized(Pawn pawn, WorkGiver giver, IntVec3 cell)
	{
		if (!PawnCanUseWorkGiver(pawn, giver))
		{
			return null;
		}
		try
		{
			Job job = giver.NonScanJob(pawn);
			if (job != null)
			{
				return job;
			}
			WorkGiver_Scanner scanner = giver as WorkGiver_Scanner;
			if (scanner != null)
			{
				if (giver.def.scanThings)
				{
					Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) && scanner.HasJobOnThing(pawn, t);
					List<Thing> thingList = cell.GetThingList(pawn.Map);
					for (int num = 0; num < thingList.Count; num++)
					{
						Thing thing = thingList[num];
						if (scanner.PotentialWorkThingRequest.Accepts(thing) && predicate(thing))
						{
							Job job2 = scanner.JobOnThing(pawn, thing);
							if (job2 != null)
							{
								job2.workGiverDef = giver.def;
							}
							return job2;
						}
					}
				}
				if (giver.def.scanCells && !cell.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, cell))
				{
					Job job3 = scanner.JobOnCell(pawn, cell);
					if (job3 != null)
					{
						job3.workGiverDef = giver.def;
					}
					return job3;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(pawn?.ToString() + " threw exception in GiverTryGiveJobTargeted on WorkGiver " + giver.def.defName + ": " + ex.ToString());
		}
		return null;
	}
}
