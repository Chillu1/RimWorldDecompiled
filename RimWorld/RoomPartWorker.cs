using Verse;

namespace RimWorld;

public abstract class RoomPartWorker
{
	public readonly RoomPartDef def;

	public virtual bool FillOnPost => false;

	public RoomPartWorker(RoomPartDef def)
	{
		this.def = def;
	}

	public abstract void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints);
}
