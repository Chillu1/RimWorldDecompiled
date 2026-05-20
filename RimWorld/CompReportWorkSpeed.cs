using Verse;

namespace RimWorld;

public class CompReportWorkSpeed : ThingComp
{
	public CompProperties_ReportWorkSpeed Props => (CompProperties_ReportWorkSpeed)props;

	public override string CompInspectStringExtra()
	{
		if (parent.def.statBases == null)
		{
			return null;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		foreach (StatDef item in DefDatabase<StatDef>.AllDefsListForReading)
		{
			if (item?.parts == null || item.Worker.IsDisabledFor(parent))
			{
				continue;
			}
			foreach (StatPart part in item.parts)
			{
				if (part is StatPart_WorkTableOutdoors || part is StatPart_Outdoors)
				{
					flag = true;
				}
				else if (part is StatPart_WorkTableTemperature)
				{
					flag2 = true;
				}
				else if (part is StatPart_WorkTableUnpowered)
				{
					flag3 = true;
				}
				else if (part is StatPart_WorkTableRoomRole)
				{
					flag4 = true;
				}
			}
		}
		StatDef statDef = Props.workSpeedStat ?? StatDefOf.WorkTableWorkSpeedFactor;
		float statValue = parent.GetStatValue(statDef);
		string text = $"{statDef.LabelCap}: {statValue.ToStringPercent()}";
		string text2 = string.Empty;
		bool num = flag && StatPart_WorkTableOutdoors.Applies(parent.def, parent.Map, parent.Position);
		bool flag5 = flag2 && StatPart_WorkTableTemperature.Applies(parent);
		bool flag6 = flag3 && StatPart_WorkTableUnpowered.Applies(parent);
		bool flag7 = flag4 && StatPart_WorkTableRoomRole.Applies(parent);
		if (num)
		{
			text2 += "Outdoors".Translate();
		}
		if (flag5)
		{
			string text3 = "BadTemperature".Translate();
			text2 = (text2.NullOrEmpty() ? (text2 + text3) : (text2 + ", " + text3));
		}
		if (flag6)
		{
			string text4 = "NoPower".Translate();
			text2 = (text2.NullOrEmpty() ? (text2 + text4) : (text2 + ", " + text4));
		}
		if (flag7)
		{
			string text5 = "NotInRoomRole".Translate(parent.def.building.workTableRoomRole.label);
			text2 = (text2.NullOrEmpty() ? (text2 + text5) : (text2 + ", " + text5));
		}
		CompAffectedByFacilities comp = parent.GetComp<CompAffectedByFacilities>();
		if (comp != null)
		{
			foreach (Thing item2 in comp.LinkedFacilitiesListForReading)
			{
				if (item2.def.GetCompProperties<CompProperties_Facility>().statOffsets.GetStatOffsetFromList(statDef) != 0f)
				{
					string label = item2.def.label;
					text2 = (text2.NullOrEmpty() ? (text2 + label) : (text2 + ", " + label));
				}
			}
		}
		if (!text2.NullOrEmpty())
		{
			text = text + " (" + text2 + ")";
		}
		return text;
	}
}
