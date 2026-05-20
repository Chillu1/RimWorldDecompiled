using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class RitualAttachableOutcomeEffectWorker
{
	public RitualAttachableOutcomeEffectDef def;

	public abstract void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets);

	public virtual AcceptanceReport CanApplyNow(Precept_Ritual ritual, Map map)
	{
		return true;
	}
}
