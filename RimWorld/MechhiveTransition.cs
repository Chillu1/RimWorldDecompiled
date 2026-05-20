using Verse;

namespace RimWorld;

public class MechhiveTransition : OrbitalTransition
{
	public override bool IsTransitionSatisfied()
	{
		if (!base.IsTransitionSatisfied())
		{
			return false;
		}
		foreach (Map map in Find.Maps)
		{
			if (map.listerThings.AnyThingWithDef(ThingDefOf.CerebrexCore))
			{
				return true;
			}
		}
		return false;
	}
}
