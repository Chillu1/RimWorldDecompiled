using Verse;

namespace RimWorld;

public class RoomPart_CrateDef : RoomPartDef
{
	public ThingDef crateDef;

	public ThingSetMakerDef thingSetMaker;

	public RotEnum rotations = RotEnum.All;

	public bool triggerThreatSignal = true;

	public RoomPart_CrateDef()
	{
		workerClass = typeof(RoomPart_Crate);
	}
}
