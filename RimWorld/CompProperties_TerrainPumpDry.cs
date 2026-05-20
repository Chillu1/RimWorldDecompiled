using Verse;

namespace RimWorld;

public class CompProperties_TerrainPumpDry : CompProperties_TerrainPump
{
	public SoundDef soundWorking;

	public CompProperties_TerrainPumpDry()
	{
		compClass = typeof(CompTerrainPumpDry);
	}
}
