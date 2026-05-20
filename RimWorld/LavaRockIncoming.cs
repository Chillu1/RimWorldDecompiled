using Verse;

namespace RimWorld;

public class LavaRockIncoming : Skyfaller
{
	private static readonly IntRange LavaRadiusRange = new IntRange(3, 5);

	private static readonly IntRange CoolTicksRange = new IntRange(48000, 72000);

	protected override void Impact()
	{
		int randomInRange = LavaRadiusRange.RandomInRange;
		IntVec3 position = base.Position;
		Map map = base.Map;
		base.Impact();
		LavaEmergenceImmediate obj = (LavaEmergenceImmediate)ThingMaker.MakeThing(ThingDefOf.LavaEmergenceImmediate);
		obj.forceCoolDelay = CoolTicksRange.RandomInRange;
		obj.forcePoolSize = randomInRange * randomInRange * 3;
		GenSpawn.Spawn(obj, position, map);
	}
}
