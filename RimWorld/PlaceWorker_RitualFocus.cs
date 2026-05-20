using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PlaceWorker_RitualFocus : PlaceWorker
	{
		private static List<Thing> tmpConnectedRitualSeats = new List<Thing>();

		protected virtual bool UseArrow => true;

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return;
			}
			PlaceWorker_SpectatorPreview.DrawSpectatorPreview(def, center, rot, ghostCol, UseArrow, out var rect, thing);
			List<Thing> forCell = Find.CurrentMap.listerBuldingOfDefInProximity.GetForCell(center, 35f, ThingDefOf.Lectern);
			try
			{
				foreach (Thing item in forCell)
				{
					PlaceWorker_RitualPosition placeWorker_RitualPosition = (PlaceWorker_RitualPosition)item.def.PlaceWorkers.FirstOrDefault((PlaceWorker w) => w is PlaceWorker_RitualPosition);
					if (placeWorker_RitualPosition != null)
					{
						placeWorker_RitualPosition.DrawGhost(item.def, item.Position, item.Rotation, ghostCol, item);
						if (rect.ClosestCellTo(item.Position).InHorDistOf(item.Position, 4.9f))
						{
							tmpConnectedRitualSeats.AddRange(PlaceWorker_RitualPosition.RitualSeatsInRange(item.def, item.Position, item.Rotation, item));
							GenDraw.DrawLineBetween(GenThing.TrueCenter(center, Rot4.North, def.size, def.Altitude), item.TrueCenter(), SimpleColor.Green);
						}
					}
				}
				PlaceWorker_RitualPosition.DrawRitualSeatConnections(def, center, rot, thing, tmpConnectedRitualSeats);
			}
			finally
			{
				tmpConnectedRitualSeats.Clear();
			}
		}
	}
}
