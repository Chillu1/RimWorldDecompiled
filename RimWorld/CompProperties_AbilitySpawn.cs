using Verse;

namespace RimWorld;

public class CompProperties_AbilitySpawn : CompProperties_AbilityEffect
{
	public ThingDef thingDef;

	public bool allowOnBuildings = true;

	public bool sendSkipSignal = true;

	public CompProperties_AbilitySpawn()
	{
		compClass = typeof(CompAbilityEffect_Spawn);
	}
}
