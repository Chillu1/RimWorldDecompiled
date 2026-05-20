using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_SummonFleshbeasts : PsychicRitualDef_InvocationCircle
{
	public SimpleCurve fleshbeastPointsFromThreatPointsCurve;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_SummonFleshbeastsCultist(InvokerRole));
		return list;
	}

	public override void CalculateMaxPower(PsychicRitualRoleAssignments assignments, List<QualityFactor> powerFactorsOut, out float power)
	{
		power = 0f;
		if (assignments.FirstAssignedPawn(InvokerRole)?.GetLord()?.LordJob is LordJob_PsychicRitual lordJob_PsychicRitual)
		{
			power = Mathf.Max(power, lordJob_PsychicRitual.points);
		}
	}
}
