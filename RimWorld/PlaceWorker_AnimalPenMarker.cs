using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_AnimalPenMarker : PlaceWorker
	{
		private static readonly AnimalPenGUI.PenPainter tmpPainter = new AnimalPenGUI.PenPainter();

		private static readonly AnimalPenGUI.PenBlueprintPainter tmpPlacingBlueprintPainter = new AnimalPenGUI.PenBlueprintPainter();

		private static readonly AnimalPenGUI.PenBlueprintPainter tmpPlacedBlueprintPainter = new AnimalPenGUI.PenBlueprintPainter();

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (thing == null)
			{
				tmpPlacingBlueprintPainter.Paint(center, Find.CurrentMap);
			}
			else if (thing is Blueprint || thing is Frame)
			{
				tmpPlacedBlueprintPainter.Paint(center, Find.CurrentMap);
			}
			else
			{
				tmpPainter.Paint(center, Find.CurrentMap);
			}
		}

		public override void DrawMouseAttachments(BuildableDef def)
		{
			Find.CurrentMap.animalPenManager.DrawPlacingMouseAttachments(def);
		}
	}
}
