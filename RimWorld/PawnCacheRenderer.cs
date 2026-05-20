using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnCacheRenderer : MonoBehaviour
{
	private Pawn pawn;

	private Rot4 rotation;

	private bool renderHead;

	private bool renderHeadgear;

	private bool renderClothes;

	private bool portrait;

	private float angle;

	private Vector3 positionOffset;

	private IReadOnlyDictionary<Apparel, Color> overrideApparelColor;

	private Color? overrideHairColor;

	private bool stylingStation;

	public void RenderPawn(Pawn pawn, RenderTexture renderTexture, Vector3 cameraOffset, float cameraZoom, float angle, Rot4 rotation, bool renderHead = true, bool renderHeadgear = true, bool renderClothes = true, bool portrait = false, Vector3 positionOffset = default(Vector3), IReadOnlyDictionary<Apparel, Color> overrideApparelColor = null, Color? overrideHairColor = null, bool stylingStation = false)
	{
		this.rotation = rotation;
		this.renderHead = renderHead;
		this.renderHeadgear = renderHeadgear;
		this.renderClothes = renderClothes;
		this.portrait = portrait;
		this.angle = angle;
		this.positionOffset = positionOffset;
		this.overrideApparelColor = overrideApparelColor;
		this.overrideHairColor = overrideHairColor;
		this.stylingStation = stylingStation;
		Camera pawnCacheCamera = Find.PawnCacheCamera;
		Vector3 position = pawnCacheCamera.transform.position;
		float orthographicSize = pawnCacheCamera.orthographicSize;
		pawnCacheCamera.transform.position += cameraOffset;
		pawnCacheCamera.orthographicSize = 1f / cameraZoom;
		this.pawn = pawn;
		pawnCacheCamera.SetTargetBuffers(renderTexture.colorBuffer, renderTexture.depthBuffer);
		pawnCacheCamera.Render();
		this.pawn = null;
		pawnCacheCamera.transform.position = position;
		pawnCacheCamera.orthographicSize = orthographicSize;
		pawnCacheCamera.targetTexture = null;
	}

	public void OnPostRender()
	{
		pawn.Drawer.renderer.RenderCache(rotation, angle, positionOffset, renderHead, portrait, renderHeadgear, renderClothes, overrideApparelColor, overrideHairColor, stylingStation);
	}
}
