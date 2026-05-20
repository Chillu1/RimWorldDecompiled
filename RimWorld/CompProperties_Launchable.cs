using Verse;

namespace RimWorld;

public class CompProperties_Launchable : CompProperties
{
	public int fixedLaunchDistanceMax = -1;

	public float fuelPerTile = 2.25f;

	public float minFuelCost = -1f;

	public ThingDef skyfallerLeaving;

	public ThingDef activeTransporterDef;

	public WorldObjectDef worldObjectDef;

	public int cooldownTicks;

	[MustTranslate]
	public string cooldownEndedMessage;

	public CompProperties_Launchable()
	{
		compClass = typeof(CompLaunchable);
	}
}
