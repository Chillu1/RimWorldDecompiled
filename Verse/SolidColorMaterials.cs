using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class SolidColorMaterials
	{
		private static Dictionary<Color, Material> simpleColorMats = new Dictionary<Color, Material>();

		private static Dictionary<Color, Material> simpleColorAndVertexColorMats = new Dictionary<Color, Material>();

		public static int SimpleColorMatCount => simpleColorMats.Count + simpleColorAndVertexColorMats.Count;

		public static Material SimpleSolidColorMaterial(Color col, bool careAboutVertexColors = false)
		{
			col = (Color32)col;
			Material value;
			if (careAboutVertexColors)
			{
				if (!simpleColorAndVertexColorMats.TryGetValue(col, out value))
				{
					value = NewSolidColorMaterial(col, ShaderDatabase.VertexColor);
					simpleColorAndVertexColorMats.Add(col, value);
				}
			}
			else if (!simpleColorMats.TryGetValue(col, out value))
			{
				value = NewSolidColorMaterial(col, ShaderDatabase.SolidColor);
				simpleColorMats.Add(col, value);
			}
			return value;
		}

		public static Material NewSolidColorMaterial(Color col, Shader shader)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to create a material from a different thread.");
				return null;
			}
			Material material = MaterialAllocator.Create(shader);
			material.color = col;
			material.name = "SolidColorMat-" + shader.name + "-" + col;
			return material;
		}

		public static Texture2D NewSolidColorTexture(float r, float g, float b, float a)
		{
			return NewSolidColorTexture(new Color(r, g, b, a));
		}

		public static Texture2D NewSolidColorTexture(Color color)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to create a texture from a different thread.");
				return null;
			}
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.name = "SolidColorTex-" + color;
			texture2D.SetPixel(0, 0, color);
			texture2D.Apply();
			return texture2D;
		}
	}
}
