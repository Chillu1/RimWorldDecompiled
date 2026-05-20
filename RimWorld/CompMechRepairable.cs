using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompMechRepairable : ThingComp
	{
		public bool autoRepair;

		public CompProperties_MechRepairable Props => (CompProperties_MechRepairable)props;

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Faction == Faction.OfPlayer)
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.defaultLabel = "CommandAutoRepair".Translate();
				command_Toggle.defaultDesc = "CommandAutoRepairDesc".Translate();
				command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/AutoRepair");
				command_Toggle.isActive = () => autoRepair;
				command_Toggle.toggleAction = (Action)Delegate.Combine(command_Toggle.toggleAction, (Action)delegate
				{
					autoRepair = !autoRepair;
				});
				yield return command_Toggle;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref autoRepair, "autoRepair", defaultValue: false);
		}
	}
}
