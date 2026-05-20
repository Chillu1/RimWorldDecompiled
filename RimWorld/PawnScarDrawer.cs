using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class PawnScarDrawer : PawnOverlayDrawer
{
	protected abstract string ScarTexturePath { get; }

	protected virtual float ScaleFactor => 1f;

	public PawnScarDrawer(Pawn pawn)
		: base(pawn)
	{
	}

	protected override void WriteCache(CacheKey key, PawnDrawParms parms, List<DrawCall> writeTarget)
	{
		Rot4 pawnRot = key.pawnRot;
		Mesh bodyMesh = key.bodyMesh;
		OverlayLayer layer = key.layer;
		Graphic graphic = ((layer == OverlayLayer.Body) ? pawn.Drawer.renderer.BodyGraphic : pawn.Drawer.renderer.HeadGraphic);
		if (graphic == null)
		{
			return;
		}
		Rand.PushState(pawn.thingIDNumber * (int)(layer + 1));
		try
		{
			Mesh mesh = (((graphic.EastFlipped && pawnRot == Rot4.East) || (graphic.WestFlipped && pawnRot == Rot4.West)) ? MeshPool.GridPlaneFlip(Vector2.one) : MeshPool.GridPlane(Vector2.one));
			Vector3 size = bodyMesh.bounds.size;
			float num = size.magnitude * ScaleFactor;
			Vector3 vector = mesh.bounds.size * num;
			Vector4 value = new Vector4(vector.x / size.x, vector.z / size.z);
			Material material = MaterialPool.MatFrom(ScarTexturePath, ShaderDatabase.Wound, Color.white);
			material = MaterialPool.MatFrom(new MaterialRequest
			{
				maskTex = (Texture2D)graphic.MatAt(pawnRot).mainTexture,
				mainTex = material.mainTexture,
				color = material.color,
				shader = material.shader
			});
			Vector3 vector2 = Rand.InsideUnitCircleVec3 / 2f;
			int rotation = Rand.Range(0, 360);
			writeTarget.Add(new DrawCall
			{
				overlayMat = material,
				matrix = Matrix4x4.Scale(size),
				overlayMesh = mesh,
				mainTexScale = value,
				mainTexOffset = new Vector4(vector2.x, vector2.z),
				rotation = rotation
			});
		}
		finally
		{
			Rand.PopState();
		}
	}
}
