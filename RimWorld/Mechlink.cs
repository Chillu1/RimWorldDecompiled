using Verse;

namespace RimWorld;

public class Mechlink : ThingWithComps
{
	public bool sentMechsToPlayer;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref sentMechsToPlayer, "sentMechsToPlayer", defaultValue: false);
	}
}
