using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_PollutionPumpArtificialBuildings : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (!ModsConfig.BiotechActive)
			{
				return;
			}
			CompProperties_PollutionPump compProperties = def.GetCompProperties<CompProperties_PollutionPump>();
			if (compProperties == null)
			{
				return;
			}
			List<Thing> forCell = Find.CurrentMap.listerArtificialBuildingsForMeditation.GetForCell(center, compProperties.radius);
			GenDraw.DrawRadiusRing(center, compProperties.radius, Color.white);
			if (forCell.NullOrEmpty())
			{
				return;
			}
			int num = 0;
			foreach (Thing item in forCell)
			{
				if (num++ > 10)
				{
					break;
				}
				GenDraw.DrawLineBetween(GenThing.TrueCenter(center, Rot4.North, def.size, def.Altitude), item.TrueCenter(), SimpleColor.Red);
			}
		}
	}
}
