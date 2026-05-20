using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse.AI;

public class MentalState_InsultingSpreeAll : MentalState_InsultingSpree
{
	private int targetFoundTicks;

	private const int CheckChooseNewTargetIntervalTicks = 250;

	private const int MaxSameTargetChaseTicks = 1250;

	private static List<Pawn> candidates = new List<Pawn>();

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
		if (target != null && !InsultingSpreeMentalStateUtility.CanChaseAndInsult(pawn, target))
		{
			ChooseNextTarget();
		}
		if (pawn.IsHashIntervalTick(250, delta) && (target == null || insultedTargetAtLeastOnce))
		{
			ChooseNextTarget();
		}
		base.MentalStateTick(delta);
	}

	private void ChooseNextTarget()
	{
		InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(base.pawn, candidates);
		if (!candidates.Any())
		{
			target = null;
			insultedTargetAtLeastOnce = false;
			targetFoundTicks = -1;
			return;
		}
		Pawn pawn = ((target == null || Find.TickManager.TicksGame - targetFoundTicks <= 1250 || !candidates.Any((Pawn x) => x != target)) ? candidates.RandomElementByWeight((Pawn x) => GetCandidateWeight(x)) : candidates.Where((Pawn x) => x != target).RandomElementByWeight((Pawn x) => GetCandidateWeight(x)));
		if (pawn != target)
		{
			target = pawn;
			insultedTargetAtLeastOnce = false;
			targetFoundTicks = Find.TickManager.TicksGame;
		}
	}

	private float GetCandidateWeight(Pawn candidate)
	{
		float num = Mathf.Min(pawn.Position.DistanceTo(candidate.Position) / 40f, 1f);
		return 1f - num + 0.01f;
	}
}
