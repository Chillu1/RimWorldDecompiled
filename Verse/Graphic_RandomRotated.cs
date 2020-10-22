using UnityEngine;

namespace Verse
{
	public class Graphic_RandomRotated : Graphic
	{
		private Graphic subGraphic;

		private float maxAngle;

		public override Material MatSingle => subGraphic.MatSingle;

		public Graphic_RandomRotated(Graphic subGraphic, float maxAngle)
		{
			this.subGraphic = subGraphic;
			this.maxAngle = maxAngle;
			drawSize = subGraphic.drawSize;
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Mesh mesh = MeshAt(rot);
			float num = 0f;
			if (thing != null)
			{
				num = 0f - maxAngle + (float)(thing.thingIDNumber * 542) % (maxAngle * 2f);
			}
			num += extraRotation;
			Graphics.DrawMesh(material: subGraphic.MatSingle, mesh: mesh, position: loc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0, camera: null, submeshIndex: 0);
		}

		public override string ToString()
		{
			return "RandomRotated(subGraphic=" + subGraphic.ToString() + ")";
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return new Graphic_RandomRotated(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo), maxAngle)
			{
				data = data,
				drawSize = drawSize
			};
		}
	}
}
