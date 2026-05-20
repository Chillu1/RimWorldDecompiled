using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class CachedTexture
{
	private string texPath;

	private Texture2D cachedTexture;

	private int validationIndex;

	private static int curValidationIndex = 0;

	private static readonly Dictionary<string, CachedTexture> cachedTextures = new Dictionary<string, CachedTexture>(32);

	public Texture2D Texture
	{
		get
		{
			if (cachedTexture == null || validationIndex != curValidationIndex)
			{
				if (texPath.NullOrEmpty())
				{
					cachedTexture = BaseContent.BadTex;
				}
				else
				{
					cachedTexture = ContentFinder<Texture2D>.Get(texPath) ?? BaseContent.BadTex;
				}
				validationIndex = curValidationIndex;
			}
			return cachedTexture;
		}
	}

	public CachedTexture(string texPath)
	{
		this.texPath = texPath;
		cachedTexture = null;
		validationIndex = -1;
	}

	public static void ResetStaticData()
	{
		curValidationIndex++;
	}

	public static Texture2D Get(string path)
	{
		if (!cachedTextures.TryGetValue(path, out var value))
		{
			value = (cachedTextures[path] = new CachedTexture(path));
		}
		return value.Texture;
	}
}
