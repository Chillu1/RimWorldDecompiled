using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal class Building_SunLamp : Building
	{
		public IEnumerable<IntVec3> GrowableCells => GenRadial.RadialCellsAround(base.Position, def.specialDisplayRadius, useCenter: true);

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Growing>() != null)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.action = MakeMatchingGrowZone;
				command_Action.hotKey = KeyBindingDefOf.Misc2;
				command_Action.defaultDesc = "CommandSunLampMakeGrowingZoneDesc".Translate();
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing");
				command_Action.defaultLabel = "CommandSunLampMakeGrowingZoneLabel".Translate();
				yield return command_Action;
			}
		}

		private void MakeMatchingGrowZone()
		{
			Designator designator = DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Growing>();
			designator.DesignateMultiCell(GrowableCells.Where((IntVec3 tempCell) => designator.CanDesignateCell(tempCell).Accepted));
		}
	}
}
