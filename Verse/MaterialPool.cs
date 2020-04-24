using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MaterialPool
	{
		private static Dictionary<MaterialRequest, Material> matDictionary = new Dictionary<MaterialRequest, Material>();

		public static Material MatFrom(string texPath, bool reportFailure)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			return MatFrom(new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, reportFailure)));
		}

		public static Material MatFrom(string texPath)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			return MatFrom(new MaterialRequest(ContentFinder<Texture2D>.Get(texPath)));
		}

		public static Material MatFrom(Texture2D srcTex)
		{
			return MatFrom(new MaterialRequest(srcTex));
		}

		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color)
		{
			return MatFrom(new MaterialRequest(srcTex, shader, color));
		}

		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color, int renderQueue)
		{
			MaterialRequest req = new MaterialRequest(srcTex, shader, color);
			req.renderQueue = renderQueue;
			return MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader)
		{
			return MatFrom(new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader));
		}

		public static Material MatFrom(string texPath, Shader shader, int renderQueue)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader);
			req.renderQueue = renderQueue;
			return MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader, Color color)
		{
			return MatFrom(new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader, color));
		}

		public static Material MatFrom(string texPath, Shader shader, Color color, int renderQueue)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader, color);
			req.renderQueue = renderQueue;
			return MatFrom(req);
		}

		public static Material MatFrom(MaterialRequest req)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get a material from a different thread.");
				return null;
			}
			if (req.mainTex == null)
			{
				Log.Error("MatFrom with null sourceTex.");
				return BaseContent.BadMat;
			}
			if (req.shader == null)
			{
				Log.Warning("Matfrom with null shader.");
				return BaseContent.BadMat;
			}
			if (req.maskTex != null && !req.shader.SupportsMaskTex())
			{
				Log.Error("MaterialRequest has maskTex but shader does not support it. req=" + req.ToString());
				req.maskTex = null;
			}
			req.color = (Color32)req.color;
			req.colorTwo = (Color32)req.colorTwo;
			if (!matDictionary.TryGetValue(req, out Material value))
			{
				value = MaterialAllocator.Create(req.shader);
				value.name = req.shader.name + "_" + req.mainTex.name;
				value.mainTexture = req.mainTex;
				value.color = req.color;
				if (req.maskTex != null)
				{
					value.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
					value.SetColor(ShaderPropertyIDs.ColorTwo, req.colorTwo);
				}
				if (req.renderQueue != 0)
				{
					value.renderQueue = req.renderQueue;
				}
				if (!req.shaderParameters.NullOrEmpty())
				{
					for (int i = 0; i < req.shaderParameters.Count; i++)
					{
						req.shaderParameters[i].Apply(value);
					}
				}
				matDictionary.Add(req, value);
				if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
				{
					WindManager.Notify_PlantMaterialCreated(value);
				}
			}
			return value;
		}
	}
}
