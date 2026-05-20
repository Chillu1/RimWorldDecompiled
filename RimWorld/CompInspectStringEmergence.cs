using Verse;

namespace RimWorld;

public class CompInspectStringEmergence : CompInspectString
{
	public Pawn sourcePawn;

	public override string CompInspectStringExtra()
	{
		if (sourcePawn != null)
		{
			return base.Props.inspectString.Formatted(sourcePawn.Named("SOURCEPAWN")).Resolve();
		}
		return null;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_References.Look(ref sourcePawn, "sourcePawn");
	}
}
