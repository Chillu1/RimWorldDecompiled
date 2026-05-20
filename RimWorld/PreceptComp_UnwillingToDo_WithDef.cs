using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PreceptComp_UnwillingToDo_WithDef : PreceptComp_UnwillingToDo
{
	public ThingDef buildingDef;

	public override bool MemberWillingToDo(HistoryEvent ev)
	{
		if (eventDef != null && ev.def != eventDef)
		{
			return true;
		}
		ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn arg);
		ev.args.TryGetArg(HistoryEventArgsNames.Building, out ThingDef arg2);
		if (arg2 == null)
		{
			return true;
		}
		if (nullifyingTraits != null && arg != null && arg.story != null)
		{
			if (!preceptDef.enabledForNPCFactions && !arg.Faction.IsPlayer)
			{
				return true;
			}
			for (int i = 0; i < nullifyingTraits.Count; i++)
			{
				if (nullifyingTraits[i].HasTrait(arg))
				{
					return true;
				}
			}
		}
		return arg2 != buildingDef;
	}

	public override IEnumerable<string> GetDescriptions()
	{
		yield return "UnwillingToDoIdeoAction".Translate() + ": " + "BuildThing".Translate(buildingDef);
	}
}
