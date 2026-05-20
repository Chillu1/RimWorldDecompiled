using Verse;

namespace RimWorld.Planet;

public class GravshipLaunch : WorldObject
{
	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (base.Faction != Faction.OfPlayer)
		{
			return text;
		}
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		text += "LaunchDate".Translate() + ": " + GenDate.DateShortStringAt(GenDate.TickGameToAbs(creationGameTicks), Find.WorldGrid.LongLatOf(base.Tile));
		text += " (";
		text += "TimeAgo".Translate((Find.TickManager.TicksGame - creationGameTicks).ToStringTicksToPeriodVague());
		return text + ")";
	}
}
