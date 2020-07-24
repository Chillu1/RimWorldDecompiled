using UnityEngine;

namespace Verse
{
	public class Graphic_MotePulse : Graphic_Mote
	{
		protected override bool ForcePropertyBlock => true;

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Mote mote = (Mote)thing;
			Graphic_Mote.propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecs, mote.AgeSecs);
			DrawMoteInternal(loc, rot, thingDef, thing, 0);
		}

		public override string ToString()
		{
			return string.Concat("Graphic_MotePulse(path=", path, ", shader=", base.Shader, ", color=", color, ", colorTwo=unsupported)");
		}
	}
}
