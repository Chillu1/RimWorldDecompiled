using System.Collections.Generic;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_SkipAbduction : PsychicRitualDef_InvocationCircle
{
	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_SkipAbduction(InvokerRole));
		return list;
	}
}
