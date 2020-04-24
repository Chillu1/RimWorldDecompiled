using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompShipPart : ThingComp
	{
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			Command_Action command_Action = new Command_Action();
			command_Action.action = ShowReport;
			command_Action.defaultLabel = "CommandShipLaunchReport".Translate();
			command_Action.defaultDesc = "CommandShipLaunchReportDesc".Translate();
			command_Action.hotKey = KeyBindingDefOf.Misc4;
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport");
			yield return command_Action;
		}

		public void ShowReport()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!ShipUtility.LaunchFailReasons((Building)parent).Any())
			{
				stringBuilder.AppendLine("ShipReportCanLaunch".Translate());
			}
			else
			{
				stringBuilder.AppendLine("ShipReportCannotLaunch".Translate());
				foreach (string item in ShipUtility.LaunchFailReasons((Building)parent))
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(item);
				}
			}
			Dialog_MessageBox window = new Dialog_MessageBox(stringBuilder.ToString());
			Find.WindowStack.Add(window);
		}
	}
}
