using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Graphic_MoteSplash : Graphic_Mote
	{
		protected override bool ForcePropertyBlock => true;

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			MoteSplash moteSplash = (MoteSplash)thing;
			float alpha = moteSplash.Alpha;
			if (!(alpha <= 0f))
			{
				Graphic_Mote.propertyBlock.SetColor(ShaderPropertyIDs.ShockwaveColor, new Color(1f, 1f, 1f, alpha));
				Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.ShockwaveSpan, moteSplash.CalculatedShockwaveSpan());
				DrawMoteInternal(loc, rot, thingDef, thing, SubcameraDefOf.WaterDepth.LayerId);
			}
		}

		public override string ToString()
		{
			return string.Concat("MoteSplash(path=", path, ", shader=", base.Shader, ", color=", color, ", colorTwo=unsupported)");
		}
	}
}
