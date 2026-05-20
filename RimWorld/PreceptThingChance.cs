using Verse;

namespace RimWorld;

public struct PreceptThingChance
{
	public ThingDef def;

	public float chance;

	public static implicit operator PreceptThingChance(PreceptThingChanceClass c)
	{
		return new PreceptThingChance
		{
			chance = c.chance,
			def = c.def
		};
	}
}
