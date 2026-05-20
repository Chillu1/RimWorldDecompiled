using Verse;

namespace RimWorld;

public class CompProperties_LootSpawn : CompProperties
{
	public ThingSetMakerDef contents;

	public CompProperties_LootSpawn()
	{
		compClass = typeof(CompLootSpawn);
	}
}
