using Verse;

namespace RimWorld;

public class Blueprint_StorageWithRoomHighlight : Blueprint_Storage
{
	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (Find.Selector.SingleSelectedThing == this)
		{
			Room room = this.GetRoom();
			if (room != null && room.ProperRoom && !room.PsychologicallyOutdoors)
			{
				room.DrawFieldEdges();
			}
		}
	}
}
