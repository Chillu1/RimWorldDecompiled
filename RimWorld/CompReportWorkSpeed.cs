using Verse;

namespace RimWorld
{
	public class CompReportWorkSpeed : ThingComp
	{
		public override string CompInspectStringExtra()
		{
			if (parent.def.statBases == null)
			{
				return null;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			foreach (StatDef item in DefDatabase<StatDef>.AllDefsListForReading)
			{
				if (item == null || item.parts == null || item.Worker.IsDisabledFor(parent))
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
				}
			}
			bool flag4 = flag && StatPart_WorkTableOutdoors.Applies(parent.def, parent.Map, parent.Position);
			bool flag5 = flag2 && StatPart_WorkTableTemperature.Applies(parent);
			bool flag6 = flag3 && StatPart_WorkTableUnpowered.Applies(parent);
			if (flag4 || flag5 || flag6)
			{
				string str = "WorkSpeedPenalty".Translate() + ": ";
				string text = "";
				if (flag4)
				{
					text += "Outdoors".Translate().ToLower();
				}
				if (flag5)
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += "BadTemperature".Translate().ToLower();
				}
				if (flag6)
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += "NoPower".Translate().ToLower();
				}
				return str + text.CapitalizeFirst();
			}
			return null;
		}
	}
}
