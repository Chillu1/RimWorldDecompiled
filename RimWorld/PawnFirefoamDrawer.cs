using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnFirefoamDrawer : PawnOverlayDrawer
{
	public bool coveredInFoam;

	public const float TextureScaleFactor = 2.8f;

	public const float TextureTiles = 1.4f;

	public const float TextureOffsetVecMagnitude = 2f;

	private static readonly string[] FoamTexturePaths = new string[4] { "Things/Pawn/Overlays/Firefoam/FireFoamOverlayA", "Things/Pawn/Overlays/Firefoam/FireFoamOverlayB", "Things/Pawn/Overlays/Firefoam/FireFoamOverlayC", "Things/Pawn/Overlays/Firefoam/FireFoamOverlayD" };

	public PawnFirefoamDrawer(Pawn pawn)
		: base(pawn)
	{
	}

	protected override void WriteCache(CacheKey key, PawnDrawParms parms, List<DrawCall> writeTarget)
	{
		Rot4 pawnRot = key.pawnRot;
		Mesh bodyMesh = key.bodyMesh;
		OverlayLayer layer = key.layer;
		Graphic graphic = ((layer == OverlayLayer.Body) ? pawn.Drawer.renderer.BodyGraphic : pawn.Drawer.renderer.HeadGraphic);
		Rand.PushState(pawn.thingIDNumber * (int)(layer + 1));
		try
		{
			bool num = (graphic.EastFlipped && pawnRot == Rot4.East) || (graphic.WestFlipped && pawnRot == Rot4.West);
			int num2 = (Rand.Range(0, FoamTexturePaths.Length) + pawnRot.AsInt) % FoamTexturePaths.Length;
			Material material = MaterialPool.MatFrom(FoamTexturePaths[num2], ShaderDatabase.FirefoamOverlay, Color.white);
			Mesh mesh = (num ? MeshPool.GridPlaneFlip(Vector2.one * 0.25f) : MeshPool.GridPlane(Vector2.one * 0.25f));
			Vector3 size = bodyMesh.bounds.size;
			float num3 = size.magnitude * 2.8f;
			material = MaterialPool.MatFrom(new MaterialRequest
			{
				maskTex = (Texture2D)graphic.MatAt(pawnRot).mainTexture,
				mainTex = material.mainTexture,
				color = material.color,
				shader = material.shader
			});
			Vector3 vector = Rand.InsideUnitCircleVec3 * 2f;
			Vector3 vector2 = mesh.bounds.size * num3;
			Vector4 value = new Vector4(vector2.x / size.x, vector2.z / size.z);
			Vector4 value2 = new Vector4(vector.x, vector.z);
			Vector4 value3 = new Vector4(1.4f, 1.4f, 1f, 1f);
			writeTarget.Add(new DrawCall
			{
				overlayMat = material,
				matrix = Matrix4x4.Scale(Vector3.one * num3),
				overlayMesh = mesh,
				displayOverApparel = true,
				maskTexScale = value,
				mainTexScale = value3,
				mainTexOffset = value2
			});
		}
		finally
		{
			Rand.PopState();
		}
	}
}
