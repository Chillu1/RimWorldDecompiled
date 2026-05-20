using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class PreceptComp
{
	[MustTranslate]
	public string description;

	public PreceptDef preceptDef;

	public virtual IEnumerable<TraitRequirement> TraitsAffecting => Enumerable.Empty<TraitRequirement>();

	public virtual void Notify_MemberTookAction(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts)
	{
	}

	public virtual void Notify_MemberWitnessedAction(HistoryEvent ev, Precept precept, Pawn member)
	{
	}

	public virtual void Notify_HistoryEvent(HistoryEvent ev, Precept precept)
	{
	}

	public virtual void Notify_MemberGenerated(Pawn pawn, Precept precept, bool newborn, bool ignoreApparel = false)
	{
	}

	public virtual void Notify_AddBedThoughts(Pawn pawn, Precept precept)
	{
	}

	public virtual bool MemberWillingToDo(HistoryEvent ev)
	{
		return true;
	}

	public virtual IEnumerable<string> GetDescriptions()
	{
		yield return description.CapitalizeFirst();
	}

	public virtual IEnumerable<string> ConfigErrors(PreceptDef parent)
	{
		return Enumerable.Empty<string>();
	}
}
