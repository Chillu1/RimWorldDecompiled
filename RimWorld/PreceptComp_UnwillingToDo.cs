using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PreceptComp_UnwillingToDo : PreceptComp
{
	public HistoryEventDef eventDef;

	public List<TraitRequirement> nullifyingTraits;

	public List<HediffDef> nullifyingHediffs;

	public override IEnumerable<TraitRequirement> TraitsAffecting
	{
		get
		{
			if (nullifyingTraits != null)
			{
				for (int i = 0; i < nullifyingTraits.Count; i++)
				{
					yield return nullifyingTraits[i];
				}
			}
		}
	}

	public override bool MemberWillingToDo(HistoryEvent ev)
	{
		if (eventDef != null && ev.def != eventDef)
		{
			return true;
		}
		if (!ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn arg))
		{
			return false;
		}
		if (!preceptDef.enabledForNPCFactions && arg != null && !arg.Faction.IsPlayer)
		{
			return true;
		}
		if (nullifyingTraits != null && arg?.story != null)
		{
			for (int i = 0; i < nullifyingTraits.Count; i++)
			{
				if (nullifyingTraits[i].HasTrait(arg))
				{
					return true;
				}
			}
		}
		if (nullifyingHediffs != null && arg?.health?.hediffSet != null)
		{
			for (int j = 0; j < nullifyingHediffs.Count; j++)
			{
				if (!arg.health.hediffSet.HasHediff(nullifyingHediffs[j]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual string GetProhibitionText()
	{
		return description ?? ((string)eventDef.LabelCap);
	}

	public override IEnumerable<string> GetDescriptions()
	{
		yield return "UnwillingToDoIdeoAction".Translate() + ": " + eventDef.LabelCap;
	}
}
