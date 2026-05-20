using Verse;

namespace RimWorld;

public class PlaceWorker_ReportWorkSpeedPenalties : PlaceWorker
{
	public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
	{
		if (!(def is ThingDef thingDef))
		{
			return;
		}
		bool flag = StatPart_WorkTableOutdoors.Applies(thingDef, map, loc);
		bool flag2 = StatPart_WorkTableTemperature.Applies(thingDef, map, loc);
		bool flag3 = StatPart_WorkTableRoomRole.WouldApplyToBuildingIfPlaced(thingDef, map, loc);
		if (!(flag || flag2 || flag3))
		{
			return;
		}
		string text = "WillGetWorkSpeedPenalty".Translate(def.label).CapitalizeFirst() + ": ";
		string text2 = "";
		if (flag)
		{
			text2 += "Outdoors".Translate().CapitalizeFirst();
		}
		if (flag2)
		{
			if (!text2.NullOrEmpty())
			{
				text2 += ", ";
			}
			text2 += "BadTemperature".Translate();
		}
		if (flag3)
		{
			if (!text2.NullOrEmpty())
			{
				text2 += ", ";
			}
			text2 += "NotInRoomRole".Translate(thingDef.building.workTableRoomRole.label);
		}
		Messages.Message(string.Concat(text + text2.CapitalizeFirst(), "."), new TargetInfo(loc, map), MessageTypeDefOf.CautionInput, historical: false);
	}
}
