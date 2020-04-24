using UnityEngine;

namespace Verse
{
	public class SubcameraDef : Def
	{
		[NoTranslate]
		public string layer;

		public int depth;

		public RenderTextureFormat format;

		[Unsaved(false)]
		private int layerCached = -1;

		public int LayerId
		{
			get
			{
				if (layerCached == -1)
				{
					layerCached = LayerMask.NameToLayer(layer);
				}
				return layerCached;
			}
		}

		public RenderTextureFormat BestFormat
		{
			get
			{
				if (SystemInfo.SupportsRenderTextureFormat(format))
				{
					return format;
				}
				if (format == RenderTextureFormat.R8 && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RG16))
				{
					return RenderTextureFormat.RG16;
				}
				if ((format == RenderTextureFormat.R8 || format == RenderTextureFormat.RG16) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
				{
					return RenderTextureFormat.ARGB32;
				}
				if ((format == RenderTextureFormat.R8 || format == RenderTextureFormat.RHalf || format == RenderTextureFormat.RFloat) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat))
				{
					return RenderTextureFormat.RGFloat;
				}
				if ((format == RenderTextureFormat.R8 || format == RenderTextureFormat.RHalf || format == RenderTextureFormat.RFloat || format == RenderTextureFormat.RGFloat) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
				{
					return RenderTextureFormat.ARGBFloat;
				}
				return format;
			}
		}
	}
}
