using Verse;

namespace RimWorld.Planet;

public class RoomTemperatureVacuum : IExposable
{
	public struct Data
	{
		public IntVec3 local;

		public float temperature;

		public float vacuum;
	}

	public IntVec3 roomCell;

	public float temperature;

	public float vacuum;

	public void ExposeData()
	{
		Scribe_Values.Look(ref roomCell, "roomCell");
		Scribe_Values.Look(ref temperature, "temperature", 0f);
		Scribe_Values.Look(ref vacuum, "vacuum", 0f);
	}
}
