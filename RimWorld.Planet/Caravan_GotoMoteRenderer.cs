using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class Caravan_GotoMoteRenderer
	{
		private int tile;

		private float lastOrderedToTileTime = -0.51f;

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		private static Material cachedMaterial;

		public static readonly Material FeedbackGoto = MaterialPool.MatFrom("Things/Mote/FeedbackGoto", ShaderDatabase.WorldOverlayTransparent, WorldMaterials.DynamicObjectRenderQueue);

		private const float Duration = 0.5f;

		private const float BaseSize = 0.8f;

		public void RenderMote()
		{
			float num = (Time.time - lastOrderedToTileTime) / 0.5f;
			if (!(num > 1f))
			{
				if (cachedMaterial == null)
				{
					cachedMaterial = MaterialPool.MatFrom((Texture2D)FeedbackGoto.mainTexture, FeedbackGoto.shader, Color.white, WorldMaterials.DynamicObjectRenderQueue);
				}
				WorldGrid worldGrid = Find.WorldGrid;
				Vector3 tileCenter = worldGrid.GetTileCenter(tile);
				Color value = new Color(1f, 1f, 1f, 1f - num);
				propertyBlock.SetColor(ShaderPropertyIDs.Color, value);
				WorldRendererUtility.DrawQuadTangentialToPlanet(tileCenter, 0.8f * worldGrid.averageTileSize, 0.018f, cachedMaterial, counterClockwise: false, useSkyboxLayer: false, propertyBlock);
			}
		}

		public void OrderedToTile(int tile)
		{
			this.tile = tile;
			lastOrderedToTileTime = Time.time;
		}
	}
}
