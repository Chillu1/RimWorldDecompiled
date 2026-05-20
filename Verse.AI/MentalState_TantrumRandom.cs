using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse.AI;

public abstract class MentalState_TantrumRandom : MentalState_Tantrum
{
	private int targetFoundTicks;

	private const int CheckChooseNewTargetIntervalTicks = 500;

	private const int MaxSameTargetAttackTicks = 1250;

	private static List<Thing> candidates = new List<Thing>();

	protected abstract void GetPotentialTargets(List<Thing> outThings);

	protected virtual Predicate<Thing> GetCustomValidator()
	{
		return null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref targetFoundTicks, "targetFoundTicks", 0);
	}

	public override void PostStart(string reason)
	{
		base.PostStart(reason);
		ChooseNextTarget();
	}

	public override void MentalStateTick(int delta)
	{
		if (target != null && (!target.Spawned || !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly) || (target is Pawn && ((Pawn)target).Downed)))
		{
			ChooseNextTarget();
		}
		if (pawn.IsHashIntervalTick(500, delta) && (target == null || hitTargetAtLeastOnce))
		{
			ChooseNextTarget();
		}
		base.MentalStateTick(delta);
	}

	private void ChooseNextTarget()
	{
		candidates.Clear();
		GetPotentialTargets(candidates);
		if (!candidates.Any())
		{
			target = null;
			hitTargetAtLeastOnce = false;
			targetFoundTicks = -1;
		}
		else
		{
			Thing thing = ((target == null || Find.TickManager.TicksGame - targetFoundTicks <= 1250 || !candidates.Any((Thing x) => x != target)) ? candidates.RandomElementByWeight((Thing x) => GetCandidateWeight(x)) : candidates.Where((Thing x) => x != target).RandomElementByWeight((Thing x) => GetCandidateWeight(x)));
			if (thing != target)
			{
				target = thing;
				hitTargetAtLeastOnce = false;
				targetFoundTicks = Find.TickManager.TicksGame;
			}
		}
		candidates.Clear();
	}

	private float GetCandidateWeight(Thing candidate)
	{
		float num = Mathf.Min(pawn.Position.DistanceTo(candidate.Position) / 40f, 1f);
		return (1f - num) * (1f - num) + 0.01f;
	}
}
