using System.IO;
using UnityEngine;

namespace Verse
{
	public static class TextureAtlasHelper
	{
		public static Mesh CreateMeshForUV(Rect uv, float scale = 1f)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = new Vector3[4]
			{
				new Vector3(-1f * scale, 0f, -1f * scale),
				new Vector3(-1f * scale, 0f, 1f * scale),
				new Vector3(1f * scale, 0f, 1f * scale),
				new Vector3(1f * scale, 0f, -1f * scale)
			};
			mesh.normals = new Vector3[4]
			{
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up
			};
			mesh.uv = new Vector2[4]
			{
				uv.min,
				new Vector2(uv.xMin, uv.yMax),
				uv.max,
				new Vector2(uv.xMax, uv.yMin)
			};
			mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
			return mesh;
		}

		public static void WriteDebugPNG(RenderTexture atlas, string path)
		{
			Texture2D texture2D = new Texture2D(atlas.width, atlas.height, TextureFormat.ARGB32, mipChain: false);
			RenderTexture.active = atlas;
			texture2D.ReadPixels(new Rect(0f, 0f, atlas.width, atlas.height), 0, 0);
			RenderTexture.active = null;
			File.WriteAllBytes(path, texture2D.EncodeToPNG());
		}

		public static void WriteDebugPNG(Texture2D atlas, string path)
		{
			Texture2D texture2D = (atlas.isReadable ? atlas : MakeReadableTextureInstance(atlas));
			File.WriteAllBytes(path, texture2D.EncodeToPNG());
			if (texture2D != atlas)
			{
				Object.Destroy(texture2D);
			}
		}

		public static Texture2D MakeReadableTextureInstance(Texture2D source)
		{
			DeepProfiler.Start("MakeReadableTextureInstance");
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			temporary.name = "MakeReadableTexture_Temp";
			Graphics.Blit(source, temporary);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = temporary;
			Texture2D texture2D = new Texture2D(source.width, source.height);
			texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			DeepProfiler.End();
			return texture2D;
		}

		public static TextureAtlasGroup ToAtlasGroup(this ThingCategory category)
		{
			return category switch
			{
				ThingCategory.Building => TextureAtlasGroup.Building, 
				ThingCategory.Plant => TextureAtlasGroup.Plant, 
				ThingCategory.Item => TextureAtlasGroup.Item, 
				ThingCategory.Filth => TextureAtlasGroup.Filth, 
				_ => TextureAtlasGroup.Misc, 
			};
		}
	}
}
