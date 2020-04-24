using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MoteBubble : MoteDualAttached
	{
		public Material iconMat;

		public Pawn arrowTarget;

		private static readonly Material InteractionArrowTex = MaterialPool.MatFrom("Things/Mote/InteractionArrow");

		public void SetupMoteBubble(Texture2D icon, Pawn target)
		{
			iconMat = MaterialPool.MatFrom(icon, ShaderDatabase.TransparentPostLight, Color.white);
			arrowTarget = target;
		}

		public override void Draw()
		{
			base.Draw();
			if (iconMat != null)
			{
				Vector3 drawPos = DrawPos;
				drawPos.y += 0.01f;
				float alpha = Alpha;
				if (alpha <= 0f)
				{
					return;
				}
				Color instanceColor = base.instanceColor;
				instanceColor.a *= alpha;
				Material material = iconMat;
				if (instanceColor != material.color)
				{
					material = MaterialPool.MatFrom((Texture2D)material.mainTexture, material.shader, instanceColor);
				}
				Vector3 s = new Vector3(def.graphicData.drawSize.x * 0.64f, 1f, def.graphicData.drawSize.y * 0.64f);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawPos, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
			}
			if (arrowTarget != null)
			{
				Quaternion rotation = Quaternion.AngleAxis((arrowTarget.TrueCenter() - DrawPos).AngleFlat(), Vector3.up);
				Vector3 drawPos2 = DrawPos;
				drawPos2.y -= 0.01f;
				drawPos2 += 0.6f * (rotation * Vector3.forward);
				Graphics.DrawMesh(MeshPool.plane05, drawPos2, rotation, InteractionArrowTex, 0);
			}
		}
	}
}
