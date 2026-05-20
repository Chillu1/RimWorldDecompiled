using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class MoteBubble : MoteDualAttached
{
	public Material iconMat;

	public Pawn arrowTarget;

	public MaterialPropertyBlock iconMatPropertyBlock;

	private static readonly Material InteractionArrowTex = MaterialPool.MatFrom("Things/Mote/InteractionArrow");

	public void SetupMoteBubble(Texture2D icon, Pawn target, Color? iconColor = null)
	{
		iconMat = MaterialPool.MatFrom(icon, ShaderDatabase.TransparentPostLight, Color.white);
		iconMatPropertyBlock = new MaterialPropertyBlock();
		arrowTarget = target;
		if (iconColor.HasValue)
		{
			iconMatPropertyBlock.SetColor(ShaderPropertyIDs.Color, iconColor.Value);
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		if (Find.UIRoot.HideMotes)
		{
			return;
		}
		if (iconMat != null)
		{
			Vector3 drawPos = DrawPos;
			drawPos.y += 0.01f;
			float alpha = Alpha;
			if (alpha <= 0f)
			{
				return;
			}
			Color color = instanceColor;
			color.a *= alpha;
			Material material = iconMat;
			if (color != material.color)
			{
				material = MaterialPool.MatFrom((Texture2D)material.mainTexture, material.shader, color);
			}
			Vector3 s = new Vector3(def.graphicData.drawSize.x * 0.64f, 1f, def.graphicData.drawSize.y * 0.64f);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawPos, Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0, null, 0, iconMatPropertyBlock);
		}
		if (arrowTarget != null)
		{
			Quaternion quaternion = Quaternion.AngleAxis(((arrowTarget.Spawned ? arrowTarget.TrueCenter() : arrowTarget.PositionHeld.ToVector3Shifted()) - DrawPos).AngleFlat(), Vector3.up);
			Vector3 drawPos2 = DrawPos;
			drawPos2.y -= 0.01f;
			drawPos2 += 0.6f * (quaternion * Vector3.forward);
			Graphics.DrawMesh(MeshPool.plane05, drawPos2, quaternion, InteractionArrowTex, 0);
		}
	}
}
