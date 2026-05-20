using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class GlobalTextureAtlasManager
{
	public static bool rebakeAtlas = false;

	private static List<PawnTextureAtlas> pawnTextureAtlases = new List<PawnTextureAtlas>();

	private static List<StaticTextureAtlas> staticTextureAtlases = new List<StaticTextureAtlas>();

	private static Dictionary<TextureAtlasGroupKey, (List<Texture2D>, HashSet<Texture2D>)> buildQueue = new Dictionary<TextureAtlasGroupKey, (List<Texture2D>, HashSet<Texture2D>)>();

	private static Dictionary<Texture2D, Texture2D> buildQueueMasks = new Dictionary<Texture2D, Texture2D>();

	public static void ClearStaticAtlasBuildQueue()
	{
		buildQueue.Clear();
	}

	public static bool TryInsertStatic(TextureAtlasGroup group, Texture2D texture, Texture2D mask = null)
	{
		if (texture.width >= 512 || texture.height >= 512)
		{
			return false;
		}
		if (mask != null && (texture.width != mask.width || texture.height != mask.height))
		{
			Log.Warning("Texture " + texture.name + " has dimensions of " + texture.width + " x " + texture.height + ", but its mask has " + mask.width + " x " + mask.height + ". This is not supported, texture will be excluded from atlas");
			return false;
		}
		TextureAtlasGroupKey key = new TextureAtlasGroupKey
		{
			group = group,
			hasMask = (mask != null)
		};
		if (!buildQueue.TryGetValue(key, out var value))
		{
			value = (new List<Texture2D>(), new HashSet<Texture2D>());
			buildQueue.Add(key, value);
		}
		if (value.Item2.Add(texture))
		{
			value.Item1.Add(texture);
		}
		if (mask != null)
		{
			if (buildQueueMasks.ContainsKey(texture))
			{
				if (buildQueueMasks[texture] != mask)
				{
					Log.Error("Same texture with 2 different masks inserted into texture atlas manager (" + texture.name + ") - " + mask.name + " | " + buildQueueMasks[texture].name + "!");
				}
			}
			else
			{
				buildQueueMasks.Add(texture, mask);
			}
		}
		return true;
	}

	public static void BakeStaticAtlases()
	{
		BuildingsDamageSectionLayerUtility.TryInsertIntoAtlas();
		MinifiedThing.TryInsertIntoAtlas();
		int pixels = 0;
		List<Texture2D> currentBatch = new List<Texture2D>();
		foreach (KeyValuePair<TextureAtlasGroupKey, (List<Texture2D>, HashSet<Texture2D>)> item in buildQueue)
		{
			foreach (Texture2D item2 in item.Value.Item1)
			{
				int num = item2.width * item2.height;
				if (num + pixels > StaticTextureAtlas.MaxPixelsPerAtlas)
				{
					FlushBatch(item.Key);
				}
				pixels += num;
				currentBatch.Add(item2);
			}
			FlushBatch(item.Key);
		}
		void FlushBatch(TextureAtlasGroupKey groupKey)
		{
			StaticTextureAtlas staticTextureAtlas = new StaticTextureAtlas(groupKey);
			foreach (Texture2D item3 in currentBatch)
			{
				if (!groupKey.hasMask || !buildQueueMasks.TryGetValue(item3, out var value))
				{
					value = null;
				}
				staticTextureAtlas.Insert(item3, value);
			}
			staticTextureAtlas.Bake();
			staticTextureAtlases.Add(staticTextureAtlas);
			pixels = 0;
			currentBatch.Clear();
		}
	}

	public static bool TryGetStaticTile(TextureAtlasGroup group, Texture2D texture, out StaticTextureAtlasTile tile, bool ignoreFoundInOtherAtlas = false)
	{
		foreach (StaticTextureAtlas staticTextureAtlase in staticTextureAtlases)
		{
			if (staticTextureAtlase.groupKey.group == group && staticTextureAtlase.TryGetTile(texture, out tile))
			{
				return true;
			}
		}
		foreach (StaticTextureAtlas staticTextureAtlase2 in staticTextureAtlases)
		{
			if (staticTextureAtlase2.TryGetTile(texture, out tile))
			{
				if (!ignoreFoundInOtherAtlas)
				{
					Log.Warning("Found texture " + texture.name + " in another atlas group than requested (found in " + staticTextureAtlase2.groupKey.ToString() + ", requested in " + group.ToString() + ")!");
				}
				return true;
			}
		}
		tile = null;
		return false;
	}

	public static bool TryGetPawnFrameSet(Pawn pawn, out PawnTextureAtlasFrameSet frameSet, out bool createdNew, bool allowCreatingNew = true)
	{
		foreach (PawnTextureAtlas pawnTextureAtlase in pawnTextureAtlases)
		{
			if (pawnTextureAtlase.TryGetFrameSet(pawn, out frameSet, out createdNew))
			{
				return true;
			}
		}
		if (allowCreatingNew)
		{
			PawnTextureAtlas pawnTextureAtlas = new PawnTextureAtlas();
			pawnTextureAtlases.Add(pawnTextureAtlas);
			return pawnTextureAtlas.TryGetFrameSet(pawn, out frameSet, out createdNew);
		}
		createdNew = false;
		frameSet = null;
		return false;
	}

	public static bool TryMarkPawnFrameSetDirty(Pawn pawn)
	{
		if (!TryGetPawnFrameSet(pawn, out var frameSet, out var _, allowCreatingNew: false))
		{
			return false;
		}
		for (int i = 0; i < frameSet.isDirty.Length; i++)
		{
			frameSet.isDirty[i] = true;
		}
		return true;
	}

	public static void GlobalTextureAtlasManagerUpdate()
	{
		if (rebakeAtlas)
		{
			FreeAllRuntimeAtlases();
			PortraitsCache.Clear();
			rebakeAtlas = false;
		}
		foreach (PawnTextureAtlas pawnTextureAtlase in pawnTextureAtlases)
		{
			pawnTextureAtlase.GC();
		}
	}

	public static void FreeAllRuntimeAtlases()
	{
		foreach (PawnTextureAtlas pawnTextureAtlase in pawnTextureAtlases)
		{
			pawnTextureAtlase.Destroy();
		}
		pawnTextureAtlases.Clear();
	}

	public static void DumpPawnAtlases(string folder)
	{
		foreach (PawnTextureAtlas pawnTextureAtlase in pawnTextureAtlases)
		{
			TextureAtlasHelper.WriteDebugPNG(pawnTextureAtlase.RawTexture, folder + "\\dump_" + pawnTextureAtlase.RawTexture.GetInstanceID() + ".png");
		}
		Log.Message("Atlases have been dumped to " + folder);
	}

	public static void DumpStaticAtlases(string folder)
	{
		foreach (StaticTextureAtlas staticTextureAtlase in staticTextureAtlases)
		{
			TextureAtlasHelper.WriteDebugPNG(staticTextureAtlase.ColorTexture, folder + "\\" + staticTextureAtlase.ColorTexture.name + ".png");
			if (staticTextureAtlase.MaskTexture != null)
			{
				TextureAtlasHelper.WriteDebugPNG(staticTextureAtlase.MaskTexture, folder + "\\" + staticTextureAtlase.MaskTexture.name + ".png");
			}
		}
		Log.Message("Atlases have been dumped to " + folder);
	}
}
