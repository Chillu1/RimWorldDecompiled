using System.Collections.Generic;

namespace RimWorld;

public class ComplexLayoutDef : StructureLayoutDef
{
	public List<ComplexThreat> threats;

	public float roomRewardCrateFactor = 0.5f;

	public float fixedHostileFactionChance = 0.25f;

	public ThingSetMakerDef rewardThingSetMakerDef;
}
