using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_MechGestatorTop : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			GhostUtility.GhostGraphicFor(GraphicDatabase.Get<Graphic_Multi>(def.building.mechGestatorCylinderGraphic.texPath, ShaderDatabase.Cutout, def.building.mechGestatorCylinderGraphic.drawSize, Color.white), def, ghostCol).DrawFromDef(GenThing.TrueCenter(loc, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()), rot, def);
			GhostUtility.GhostGraphicFor(GraphicDatabase.Get<Graphic_Multi>(def.building.mechGestatorTopGraphic.texPath, ShaderDatabase.Cutout, def.building.mechGestatorTopGraphic.drawSize, Color.white), def, ghostCol).DrawFromDef(GenThing.TrueCenter(loc, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()), rot, def);
		}
	}
}
