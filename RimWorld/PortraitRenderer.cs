using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PortraitRenderer : MonoBehaviour
	{
		private Pawn pawn;

		public void RenderPortrait(Pawn pawn, RenderTexture renderTexture, Vector3 cameraOffset, float cameraZoom)
		{
			Camera portraitCamera = Find.PortraitCamera;
			portraitCamera.targetTexture = renderTexture;
			Vector3 position = portraitCamera.transform.position;
			float orthographicSize = portraitCamera.orthographicSize;
			portraitCamera.transform.position += cameraOffset;
			portraitCamera.orthographicSize = 1f / cameraZoom;
			this.pawn = pawn;
			portraitCamera.Render();
			this.pawn = null;
			portraitCamera.transform.position = position;
			portraitCamera.orthographicSize = orthographicSize;
			portraitCamera.targetTexture = null;
		}

		public void OnPostRender()
		{
			pawn.Drawer.renderer.RenderPortrait();
		}
	}
}
