using System.Collections.Generic;

namespace RimWorld;

public class BiomeFishTypes
{
	public List<FishChance> freshwater_Common = new List<FishChance>();

	public List<FishChance> saltwater_Common = new List<FishChance>();

	public List<FishChance> freshwater_Uncommon = new List<FishChance>();

	public List<FishChance> saltwater_Uncommon = new List<FishChance>();

	public ThingSetMakerDef rareCatchesSetMaker;
}
