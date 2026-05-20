using System;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_TakeCombatEnhancingDrug : ThinkNode_JobGiver
{
	private bool onlyIfInDanger;

	private const int TakeEveryTicks = 20000;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_TakeCombatEnhancingDrug obj = (JobGiver_TakeCombatEnhancingDrug)base.DeepCopy(resolve);
		obj.onlyIfInDanger = onlyIfInDanger;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!pawn.RaceProps.Humanlike)
		{
			return null;
		}
		if (pawn.IsTeetotaler())
		{
			return null;
		}
		if (Find.TickManager.TicksGame - pawn.mindState.lastTakeCombatEnhancingDrugTick < 20000)
		{
			return null;
		}
		Thing thing = pawn.inventory.FindCombatEnhancingDrug();
		if (thing == null)
		{
			return null;
		}
		if (onlyIfInDanger)
		{
			Lord lord = pawn.GetLord();
			if (lord == null)
			{
				if (!HarmedRecently(pawn))
				{
					return null;
				}
			}
			else
			{
				int num = 0;
				int num2 = Mathf.Clamp(lord.ownedPawns.Count / 2, 1, 4);
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					if (HarmedRecently(lord.ownedPawns[i]))
					{
						num++;
						if (num >= num2)
						{
							break;
						}
					}
				}
				if (num < num2)
				{
					return null;
				}
			}
		}
		Job job = JobMaker.MakeJob(JobDefOf.Ingest, thing);
		job.count = 1;
		return job;
	}

	private bool HarmedRecently(Pawn pawn)
	{
		return Find.TickManager.TicksGame - pawn.mindState.lastHarmTick < 2500;
	}

	[Obsolete("Will be removed in a future update, use pawn.inventory.FindCombatEnhancingDrug()")]
	private Thing FindCombatEnhancingDrug(Pawn pawn)
	{
		return pawn.inventory.FindCombatEnhancingDrug();
	}
}
