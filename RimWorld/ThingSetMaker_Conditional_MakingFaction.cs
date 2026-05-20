using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThingSetMaker_Conditional_MakingFaction : ThingSetMaker_Conditional
{
	public FactionDef makingFaction;

	public List<string> makingFactionCategories;

	public bool requireNonNull;

	protected override bool Condition(ThingSetMakerParams parms)
	{
		if (requireNonNull && parms.makingFaction == null)
		{
			return false;
		}
		if (makingFaction != null && (parms.makingFaction == null || parms.makingFaction.def != makingFaction))
		{
			return false;
		}
		if (!makingFactionCategories.NullOrEmpty() && (parms.makingFaction == null || !makingFactionCategories.Contains(parms.makingFaction.def.categoryTag)))
		{
			return false;
		}
		return true;
	}
}
