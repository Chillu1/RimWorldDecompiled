using Verse;

namespace RimWorld;

public class StatWorker_RoomReadingBonus : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (!base.ShouldShowFor(req))
		{
			return false;
		}
		if (!(req.Def is ThingDef thingDef))
		{
			return false;
		}
		if (typeof(Building_Bookcase).IsAssignableFrom(thingDef.thingClass))
		{
			return true;
		}
		return false;
	}
}
