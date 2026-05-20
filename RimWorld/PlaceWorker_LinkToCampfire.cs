using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_LinkToCampfire : PlaceWorker
	{
		public float range = 11.9f;

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			GenDraw.DrawRadiusRing(center, range);
			List<Thing> forCell = Find.CurrentMap.listerBuldingOfDefInProximity.GetForCell(center, range, ThingDefOf.Campfire);
			for (int i = 0; i < forCell.Count; i++)
			{
				GenDraw.DrawLineBetween(GenThing.TrueCenter(center, Rot4.North, def.size, def.Altitude), forCell[i].TrueCenter(), SimpleColor.Green);
			}
		}
	}
}
