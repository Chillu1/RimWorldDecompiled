using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_ShipComputerCore : Building
	{
		private bool CanLaunchNow => !ShipUtility.LaunchFailReasons(this).Any();

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Gizmo item in ShipUtility.ShipStartupGizmos(this))
			{
				yield return item;
			}
			Command_Action command_Action = new Command_Action();
			command_Action.action = TryLaunch;
			command_Action.defaultLabel = "CommandShipLaunch".Translate();
			command_Action.defaultDesc = "CommandShipLaunchDesc".Translate();
			if (!CanLaunchNow)
			{
				command_Action.Disable(ShipUtility.LaunchFailReasons(this).First());
			}
			if (ShipCountdown.CountingDown)
			{
				command_Action.Disable();
			}
			command_Action.hotKey = KeyBindingDefOf.Misc1;
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");
			yield return command_Action;
		}

		public void ForceLaunch()
		{
			ShipCountdown.InitiateCountdown(this);
			if (base.Spawned)
			{
				QuestUtility.SendQuestTargetSignals(base.Map.Parent.questTags, "LaunchedShip");
			}
		}

		private void TryLaunch()
		{
			if (CanLaunchNow)
			{
				ForceLaunch();
			}
		}
	}
}
