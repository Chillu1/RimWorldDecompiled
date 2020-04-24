using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_TurretTop : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			GhostUtility.GhostGraphicFor(GraphicDatabase.Get<Graphic_Single>(def.building.turretGunDef.graphicData.texPath, ShaderDatabase.Cutout, new Vector2(def.building.turretTopDrawSize, def.building.turretTopDrawSize), Color.white), def, ghostCol).DrawFromDef(GenThing.TrueCenter(loc, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()), rot, def, TurretTop.ArtworkRotation);
		}
	}
}
