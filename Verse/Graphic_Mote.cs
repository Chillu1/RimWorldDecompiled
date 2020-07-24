using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Graphic_Mote : Graphic_Single
	{
		protected static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		protected virtual bool ForcePropertyBlock => false;

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			DrawMoteInternal(loc, rot, thingDef, thing, 0);
		}

		public void DrawMoteInternal(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, int layer)
		{
			Mote mote = (Mote)thing;
			float alpha = mote.Alpha;
			if (!(alpha <= 0f))
			{
				Color color = base.Color * mote.instanceColor;
				color.a *= alpha;
				Vector3 exactScale = mote.exactScale;
				exactScale.x *= data.drawSize.x;
				exactScale.z *= data.drawSize.y;
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(mote.DrawPos, Quaternion.AngleAxis(mote.exactRotation, Vector3.up), exactScale);
				Material matSingle = MatSingle;
				if (!ForcePropertyBlock && color.IndistinguishableFrom(matSingle.color))
				{
					Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, layer, null, 0);
					return;
				}
				propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
				Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, layer, null, 0, propertyBlock);
			}
		}

		public override string ToString()
		{
			return string.Concat("Mote(path=", path, ", shader=", base.Shader, ", color=", color, ", colorTwo=unsupported)");
		}
	}
}
