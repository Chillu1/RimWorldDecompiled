using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_WindTurbine : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			GenDraw.DrawFieldEdges(WindTurbineUtility.CalculateWindCells(center, rot, def.size).ToList());
		}
	}
}
