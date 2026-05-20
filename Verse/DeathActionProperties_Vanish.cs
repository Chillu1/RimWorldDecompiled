using RimWorld;

namespace Verse;

public class DeathActionProperties_Vanish : DeathActionProperties
{
	public FleckDef fleck;

	public ThingDef filth;

	public IntRange filthCountRange = IntRange.One;

	public FleshbeastUtility.MeatExplosionSize? meatExplosionSize;

	public DeathActionProperties_Vanish()
	{
		workerClass = typeof(DeathActionWorker_Vanish);
	}
}
