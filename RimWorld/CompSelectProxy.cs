using Verse;

namespace RimWorld;

public class CompSelectProxy : ThingComp
{
	public Thing thingToSelect;

	public override void PostExposeData()
	{
		Scribe_References.Look(ref thingToSelect, "thingToSelect");
	}
}
