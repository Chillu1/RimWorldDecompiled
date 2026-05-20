namespace Verse;

public abstract class RoomRoleWorker
{
	public virtual string PostProcessedLabel(string baseLabel, Room room)
	{
		return baseLabel;
	}

	public abstract float GetScore(Room room);

	public virtual float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		return 0f;
	}
}
