using Verse;

namespace RimWorld;

public class VirtualPawnRelation : IExposable
{
	public PawnRelationDef def;

	public RelationshipRecord record;

	public int startTicks;

	public VirtualPawnRelation()
	{
	}

	public VirtualPawnRelation(PawnRelationDef def, RelationshipRecord record, int startTicks)
	{
		this.def = def;
		this.record = record;
		this.startTicks = startTicks;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref record, "record");
		Scribe_Values.Look(ref startTicks, "startTicks", 0);
	}
}
