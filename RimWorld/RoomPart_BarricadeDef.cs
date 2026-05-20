using Verse;

namespace RimWorld;

public class RoomPart_BarricadeDef : RoomPartDef
{
	public ThingDef wallDef;

	public ThingDef stuffDef;

	public int steps = 2;

	public int offset = 2;

	public float chancePerDoor = 0.75f;

	public RoomPart_BarricadeDef()
	{
		workerClass = typeof(RoomPart_Barricades);
	}
}
