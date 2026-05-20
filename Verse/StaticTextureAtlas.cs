using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Verse;

public class StaticTextureAtlas
{
	public readonly TextureAtlasGroupKey groupKey;

	private List<Texture2D> textures = new List<Texture2D>();

	private Dictionary<Texture2D, Texture2D> masks = new Dictionary<Texture2D, Texture2D>();

	private Dictionary<Texture, StaticTextureAtlasTile> tiles = new Dictionary<Texture, StaticTextureAtlasTile>();

	private Texture2D colorTexture;

	private Texture2D maskTexture;

	public const int MaxTextureSizeForTiles = 512;

	public const int TexturePadding = 8;

	public Texture2D ColorTexture => colorTexture;

	public Texture2D MaskTexture => maskTexture;

	public static int MaxPixelsPerAtlas => MaxAtlasSize / 2 * (MaxAtlasSize / 2);

	public static int MaxAtlasSize => SystemInfo.maxTextureSize;

	public StaticTextureAtlas(TextureAtlasGroupKey groupKey)
	{
		this.groupKey = groupKey;
		colorTexture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
	}

	public void Insert(Texture2D texture, Texture2D mask = null)
	{
		if (groupKey.hasMask && mask == null)
		{
			Log.Error("Tried to insert a mask-less texture into a static atlas which does have a mask atlas");
		}
		if (!groupKey.hasMask && mask != null)
		{
			Log.Error("Tried to insert a mask texture into a static atlas which does not have a mask atlas");
		}
		textures.Add(texture);
		if (mask != null && groupKey.hasMask)
		{
			masks.Add(texture, mask);
		}
	}

	public void Bake(bool rebake = false)
	{
		using (new DeepProfilerScope("StaticTextureAtlas.Bake()"))
		{
			if (rebake)
			{
				foreach (KeyValuePair<Texture, StaticTextureAtlasTile> tile in tiles)
				{
					UnityEngine.Object.Destroy(tile.Value.mesh);
				}
				tiles.Clear();
			}
			Rect[] array = CalcRectsForAtlasNew();
			if (array.Length != textures.Count)
			{
				Log.Error("Texture packing failed! Clearing out atlas...");
				textures.Clear();
				return;
			}
			bool flag = !UnityData.ComputeShadersSupported;
			BlitTexturesToColorAtlas(array, flag);
			if (groupKey.hasMask)
			{
				BuildMaskAtlas(array, flag);
			}
			BuildMeshesForUvs(array);
			if (Prefs.TextureCompression)
			{
				ApplyTextureCompression(flag);
			}
			if (flag)
			{
				DeepProfiler.Start("Final Texture2D.Apply() for atlas textures");
				if (colorTexture != null)
				{
					colorTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				}
				if (maskTexture != null)
				{
					maskTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				}
				DeepProfiler.End();
			}
		}
	}

	private void ApplyTextureCompression(bool noGpuCompressionSupport)
	{
		DeepProfiler.Start("Compress atlas textures");
		if (colorTexture != null)
		{
			if (noGpuCompressionSupport)
			{
				colorTexture.Compress(highQuality: true);
			}
			else
			{
				string name = colorTexture.name;
				colorTexture = FastCompressDXT(colorTexture, deleteOriginal: true);
				colorTexture.name = name;
			}
		}
		if (maskTexture != null)
		{
			if (noGpuCompressionSupport)
			{
				maskTexture.Compress(highQuality: true);
			}
			else
			{
				string name2 = maskTexture.name;
				maskTexture = FastCompressDXT(maskTexture, deleteOriginal: true);
				maskTexture.name = name2;
			}
		}
		DeepProfiler.End();
	}

	private void BuildMeshesForUvs(Rect[] uvRects)
	{
		for (int i = 0; i < textures.Count; i++)
		{
			Mesh mesh = TextureAtlasHelper.CreateMeshForUV(uvRects[i], 0.5f);
			mesh.name = "TextureAtlasMesh_" + groupKey.ToString() + "_" + mesh.GetInstanceID();
			tiles.Add(textures[i], new StaticTextureAtlasTile
			{
				atlas = this,
				mesh = mesh,
				uvRect = uvRects[i]
			});
		}
	}

	private Rect[] CalcRectsForAtlasNew(int divisor = 1, bool useIncreasedShortAxis = false)
	{
		DeepProfiler.Start("StaticTextureAtlas.CalcRectsForAtlasNew()");
		List<Texture2D> list = textures.OrderByDescending((Texture2D t) => t.height).ToList();
		int num = list.Sum((Texture2D t) => t.width * t.height);
		int a = Mathf.NextPowerOfTwo((int)((float)Mathf.CeilToInt(Mathf.Sqrt(num)) * 1.05f));
		a = Mathf.Min(a, MaxAtlasSize);
		int num2 = Mathf.NextPowerOfTwo(Mathf.CeilToInt((float)num / (float)a));
		if (useIncreasedShortAxis)
		{
			num2 *= 2;
		}
		num2 = Mathf.Min(num2, MaxAtlasSize);
		if (divisor > 1)
		{
			num2 /= divisor;
			int num3 = num2 * a;
			int num4 = num / (divisor * divisor);
			if ((float)num3 < (float)num4 * 1.5f)
			{
				num2 *= divisor;
			}
		}
		List<Vector2> list2 = list.Select((Texture2D t) => new Vector2(Mathf.RoundToInt((float)t.width / (float)divisor), Mathf.RoundToInt((float)t.height / (float)divisor))).ToList();
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		Rect[] array = new Rect[list.Count];
		for (int num8 = 0; num8 < list.Count; num8++)
		{
			Texture2D texture2D = list[num8];
			Vector2 vector = list2[num8];
			if ((float)num5 + vector.x > (float)a)
			{
				num5 = 0;
				num6 += num7;
				num7 = 0;
			}
			if ((float)num6 + vector.y > (float)num2)
			{
				if (divisor == 1 && !useIncreasedShortAxis && num2 < MaxAtlasSize)
				{
					return CalcRectsForAtlasNew(divisor, useIncreasedShortAxis: true);
				}
				if (divisor < 4)
				{
					Log.Warning($"StaticTextureAtlas: Texture {texture2D.name} does not fit in the atlas of size {a}x{num2} (trying to place at y {num6} with height of {vector.y}). Retrying with divisor of {divisor * 2}.");
					return CalcRectsForAtlasNew(divisor * 2);
				}
				return CalcRectsForAtlas();
			}
			array[num8] = new Rect(num5, num6, vector.x, vector.y);
			num5 += Mathf.CeilToInt(vector.x);
			if (vector.y > (float)num7)
			{
				num7 = Mathf.CeilToInt(vector.y);
			}
		}
		Rect[] array2 = new Rect[list.Count];
		for (int num9 = 0; num9 < textures.Count; num9++)
		{
			Texture2D texture2D2 = textures[num9];
			int num10 = list.IndexOf(texture2D2);
			if (num10 < 0)
			{
				Log.Error("Texture " + texture2D2.name + " not found in height sorted list, cannot calculate UV rect.");
				continue;
			}
			Rect rect = array[num10];
			array2[num9] = new Rect(rect.x / (float)a, rect.y / (float)num2, rect.width / (float)a, rect.height / (float)num2);
		}
		Texture2D obj = colorTexture;
		int mipCount = Mathf.FloorToInt(Mathf.Log((float)a / 512f, 2f)) + 1;
		colorTexture = new Texture2D(a, num2, GraphicsFormat.R8G8B8A8_SRGB, mipCount, TextureCreationFlags.MipChain | TextureCreationFlags.DontInitializePixels);
		UnityEngine.Object.DestroyImmediate(obj);
		DeepProfiler.End();
		return array2;
	}

	private Rect[] CalcRectsForAtlas()
	{
		DeepProfiler.Start("Create dummy textures for atlas packing");
		Texture2D[] array = textures.Select(delegate(Texture2D t)
		{
			int width = t.width;
			int height = t.height;
			bool flag = width % 4 == 0 && height % 4 == 0;
			return new Texture2D(width, height, (!flag) ? TextureFormat.Alpha8 : TextureFormat.DXT1, 1, linear: false, createUninitialized: true)
			{
				name = t.name + "_DummyForAtlas",
				filterMode = t.filterMode,
				wrapMode = t.wrapMode,
				anisoLevel = t.anisoLevel,
				mipMapBias = t.mipMapBias,
				minimumMipmapLevel = t.minimumMipmapLevel
			};
		}).ToArray();
		DeepProfiler.End();
		DeepProfiler.Start("PackTextures() with dummy textures");
		Rect[] result = colorTexture.PackTextures(array, 8, MaxAtlasSize, makeNoLongerReadable: false);
		Texture2D texture2D = colorTexture;
		Texture2D[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			UnityEngine.Object.DestroyImmediate(array2[num]);
		}
		int mipCount = Mathf.FloorToInt(Mathf.Log((float)Mathf.Max(texture2D.width, texture2D.height) / 512f, 2f)) + 1;
		colorTexture = new Texture2D(texture2D.width, texture2D.height, GraphicsFormat.R8G8B8A8_SRGB, mipCount, TextureCreationFlags.MipChain | TextureCreationFlags.DontInitializePixels);
		UnityEngine.Object.DestroyImmediate(texture2D);
		DeepProfiler.End();
		return result;
	}

	private void BlitTexturesToColorAtlas(Rect[] uvRects, bool noGpuCompressionSupport)
	{
		RenderTexture renderTexture = new RenderTexture(colorTexture.width, colorTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
		{
			name = "StaticTextureAtlas_Bake_Temp"
		};
		renderTexture.Create();
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(renderTexture);
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, renderTexture.width, renderTexture.height, 0f);
		DeepProfiler.Start("Render real textures to RT");
		Material material = new Material(Shader.Find("Custom/BlitExact"));
		try
		{
			for (int i = 0; i < textures.Count; i++)
			{
				Rect rect = uvRects[i];
				Texture2D texture = textures[i];
				int num = Mathf.RoundToInt(rect.x * (float)colorTexture.width);
				int num2 = Mathf.RoundToInt((1f - rect.y) * (float)colorTexture.height);
				int num3 = Mathf.RoundToInt(rect.width * (float)colorTexture.width);
				int num4 = Mathf.RoundToInt(rect.height * (float)colorTexture.height);
				num2 -= num4;
				Graphics.DrawTexture(new Rect(num, num2, num3, num4), texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, material, -1);
			}
		}
		finally
		{
			UnityEngine.Object.Destroy(material);
		}
		GL.PopMatrix();
		DeepProfiler.End();
		if (noGpuCompressionSupport)
		{
			DeepProfiler.Start("GPU Readback from RenderTexture to Texture2D");
			RenderTexture.active = renderTexture;
			colorTexture.ReadPixels(new Rect(0f, 0f, colorTexture.width, colorTexture.height), 0, 0, recalculateMipMaps: false);
			DeepProfiler.End();
			DeepProfiler.Start("colorTexture.Apply() to generate mipmaps");
			colorTexture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
			DeepProfiler.End();
		}
		else
		{
			DeepProfiler.Start("CopyTexture from RenderTexture to Texture2D");
			Graphics.CopyTexture(renderTexture, 0, 0, 0, 0, colorTexture.width, colorTexture.height, colorTexture, 0, 0, 0, 0);
			DeepProfiler.End();
			DeepProfiler.Start("Generate mipmaps with compute shader");
			GenerateMipmapsWithCompute(colorTexture);
			DeepProfiler.End();
		}
		RenderTexture.active = active;
		renderTexture.Release();
		UnityEngine.Object.DestroyImmediate(renderTexture);
		colorTexture.name = "TextureAtlas_" + groupKey.ToString() + "_" + colorTexture.GetInstanceID();
	}

	private void BuildMaskAtlas(Rect[] uvRects, bool noGpuCompressionSupport)
	{
		maskTexture = new Texture2D(colorTexture.width, colorTexture.height, TextureFormat.ARGB32, mipChain: false);
		RenderTexture renderTexture = new RenderTexture(maskTexture.width, maskTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
		{
			name = "StaticTextureAtlas_MaskBake_Temp"
		};
		renderTexture.Create();
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(renderTexture);
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, renderTexture.width, renderTexture.height, 0f);
		DeepProfiler.Start("Render mask textures to RT");
		Material material = new Material(Shader.Find("Custom/BlitExact"));
		try
		{
			for (int i = 0; i < textures.Count; i++)
			{
				Texture2D key = textures[i];
				if (masks.TryGetValue(key, out var value))
				{
					Rect rect = uvRects[i];
					int num = Mathf.RoundToInt(rect.x * (float)maskTexture.width);
					int num2 = Mathf.RoundToInt((1f - rect.y) * (float)maskTexture.height);
					int num3 = Mathf.RoundToInt(rect.width * (float)maskTexture.width);
					int num4 = Mathf.RoundToInt(rect.height * (float)maskTexture.height);
					num2 -= num4;
					Graphics.DrawTexture(new Rect(num, num2, num3, num4), value, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, material, -1);
				}
			}
		}
		finally
		{
			UnityEngine.Object.Destroy(material);
		}
		GL.PopMatrix();
		DeepProfiler.End();
		if (noGpuCompressionSupport)
		{
			DeepProfiler.Start("GPU Readback from RenderTexture to mask Texture2D");
			RenderTexture.active = renderTexture;
			maskTexture.ReadPixels(new Rect(0f, 0f, maskTexture.width, maskTexture.height), 0, 0, recalculateMipMaps: false);
			DeepProfiler.End();
			DeepProfiler.Start("maskTexture.Apply()");
			maskTexture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
			DeepProfiler.End();
		}
		else
		{
			DeepProfiler.Start("CopyTexture from RenderTexture to mask Texture2D");
			Graphics.CopyTexture(renderTexture, 0, 0, 0, 0, maskTexture.width, maskTexture.height, maskTexture, 0, 0, 0, 0);
			DeepProfiler.End();
		}
		RenderTexture.active = active;
		renderTexture.Release();
		UnityEngine.Object.DestroyImmediate(renderTexture);
		maskTexture.name = "Mask_" + colorTexture.name;
	}

	public bool TryGetTile(Texture texture, out StaticTextureAtlasTile tile)
	{
		return tiles.TryGetValue(texture, out tile);
	}

	private void GenerateMipmapsWithCompute(Texture2D baseTexture)
	{
		ComputeShader computeShader = Resources.Load<ComputeShader>("Materials/Misc/Mipmapper");
		int kernelIndex = computeShader.FindKernel("GenerateMip");
		int num = Mathf.FloorToInt(Mathf.Log((float)Mathf.Max(baseTexture.width, baseTexture.height) / 512f, 2f));
		if (num >= 1)
		{
			RenderTexture active = RenderTexture.active;
			RenderTexture renderTexture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.ARGB32);
			renderTexture.Create();
			Material material = new Material(Shader.Find("Custom/BlitExact"));
			Graphics.Blit(baseTexture, renderTexture, material);
			UnityEngine.Object.Destroy(material);
			RenderTexture[] array = new RenderTexture[num];
			RenderTexture renderTexture2 = renderTexture;
			for (int i = 0; i < num; i++)
			{
				int num2 = Mathf.Max(1, renderTexture2.width / 2);
				int num3 = Mathf.Max(1, renderTexture2.height / 2);
				array[i] = new RenderTexture(num2, num3, 0, RenderTextureFormat.ARGB32)
				{
					enableRandomWrite = true,
					name = $"GenerateMipmapsWithCompute_Mip{i + 1}"
				};
				array[i].Create();
				computeShader.SetTexture(kernelIndex, "InputTexture", renderTexture2);
				computeShader.SetTexture(kernelIndex, "OutputMip", array[i]);
				computeShader.SetInts("InputSize", renderTexture2.width, renderTexture2.height);
				computeShader.SetInts("OutputSize", num2, num3);
				computeShader.Dispatch(kernelIndex, Mathf.CeilToInt((float)num2 / 8f), Mathf.CeilToInt((float)num3 / 8f), 1);
				renderTexture2 = array[i];
			}
			for (int j = 0; j < num; j++)
			{
				Graphics.CopyTexture(array[j], 0, 0, baseTexture, 0, j + 1);
				array[j].Release();
				UnityEngine.Object.DestroyImmediate(array[j]);
			}
			RenderTexture.active = active;
			renderTexture.Release();
			UnityEngine.Object.DestroyImmediate(renderTexture);
		}
	}

	public static Texture2D FastCompressDXT(Texture2D texture, bool deleteOriginal = false)
	{
		int num = Mathf.Min(texture.width, texture.height);
		if (num <= 16)
		{
			return texture;
		}
		ComputeShader computeShader = Resources.Load<ComputeShader>("Materials/Misc/EncodeBCn");
		int kernelIndex = computeShader.FindKernel("EncodeBC3_AMD");
		int val = Mathf.FloorToInt(Mathf.Log((float)num / 16f, 2f)) + 1;
		val = Math.Min(val, CalculateMaxMipmapsForDxtSupport(texture));
		val = Mathf.Min(val, texture.mipmapCount);
		val = Mathf.Max(val, 1);
		Texture2D texture2D = new Texture2D(texture.width, texture.height, GraphicsFormat.RGBA_DXT5_UNorm, val, TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		computeShader.SetFloat("_Quality", 0.9f);
		for (int i = 0; i < val; i++)
		{
			int num2 = Mathf.Max(1, texture.width >> i);
			int num3 = Mathf.Max(1, texture.height >> i);
			RenderTexture renderTexture = new RenderTexture(num2 / 4, num3 / 4, 24)
			{
				graphicsFormat = GraphicsFormat.R32G32B32A32_SInt,
				enableRandomWrite = true,
				name = $"FastCompressDXT_OutputRT_Mip{i}"
			};
			renderTexture.Create();
			computeShader.SetTexture(kernelIndex, "_Target", renderTexture);
			computeShader.SetTexture(kernelIndex, "_Source", texture, i);
			computeShader.SetInt("_mipLevel", i);
			computeShader.Dispatch(kernelIndex, num2 / 8, num3 / 8, 1);
			Graphics.CopyTexture(renderTexture, 0, 0, 0, 0, renderTexture.width, renderTexture.height, texture2D, 0, i, 0, 0);
			renderTexture.Release();
			UnityEngine.Object.DestroyImmediate(renderTexture);
		}
		if (deleteOriginal)
		{
			UnityEngine.Object.DestroyImmediate(texture);
		}
		return texture2D;
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(colorTexture);
		UnityEngine.Object.Destroy(maskTexture);
		foreach (KeyValuePair<Texture, StaticTextureAtlasTile> tile in tiles)
		{
			UnityEngine.Object.Destroy(tile.Value.mesh);
		}
		textures.Clear();
		tiles.Clear();
	}

	public static int CalculateMaxMipmapsForDxtSupport(Texture2D tex)
	{
		int num = 0;
		int num2 = tex.width;
		int num3 = tex.height;
		while (num2 >= 4 && num3 >= 4 && num2 % 4 == 0 && num3 % 4 == 0)
		{
			num++;
			num2 >>= 1;
			num3 >>= 1;
		}
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}
}
