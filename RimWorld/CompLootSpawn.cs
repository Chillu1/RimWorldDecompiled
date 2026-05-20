using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompLootSpawn : ThingComp
{
	private CompProperties_LootSpawn Props => (CompProperties_LootSpawn)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (respawningAfterLoad)
		{
			return;
		}
		Building_Crate building_Crate = (Building_Crate)parent;
		if (Props.contents == null)
		{
			building_Crate.Open();
			return;
		}
		List<Thing> list = Props.contents.root.Generate(default(ThingSetMakerParams));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Thing thing = list[num];
			if (!building_Crate.TryAcceptThing(thing, allowSpecialEffects: false))
			{
				thing.Destroy();
			}
		}
	}
}
