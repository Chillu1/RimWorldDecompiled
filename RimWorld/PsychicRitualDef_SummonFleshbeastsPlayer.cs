using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_SummonFleshbeastsPlayer : PsychicRitualDef_InvocationCircle
{
	public SimpleCurve fleshbeastCombatPointsFromQualityCurve;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
		list.Add(new PsychicRitualToil_SummonFleshbeastsPlayer(InvokerRole));
		return list;
	}
}
