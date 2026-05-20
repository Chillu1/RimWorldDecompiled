using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class CompStatOffsetBase : ThingComp
{
	protected Pawn lastUser;

	public CompProperties_StatOffsetBase Props => (CompProperties_StatOffsetBase)props;

	public virtual Pawn LastUser => lastUser;

	public abstract float GetStatOffset(Pawn pawn = null);

	public abstract IEnumerable<string> GetExplanation();

	public virtual void Used(Pawn pawn)
	{
		lastUser = pawn;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_References.Look(ref lastUser, "lastUser");
	}
}
