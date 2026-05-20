using Verse;

namespace RimWorld;

public class ThoughtWorker_HediffThoughtRandom : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		foreach (Hediff hediff in p.health.hediffSet.hediffs)
		{
			if (!(hediff is HediffWithComps hediffWithComps))
			{
				continue;
			}
			foreach (HediffComp comp in hediffWithComps.comps)
			{
				if (comp is HediffComp_GiveRandomSituationalThought hediffComp_GiveRandomSituationalThought && hediffComp_GiveRandomSituationalThought.selectedThought == def)
				{
					return true;
				}
			}
		}
		return false;
	}
}
