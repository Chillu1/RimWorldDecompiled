using UnityEngine;

namespace Verse;

public class CachedMaterial
{
	private string texPath;

	private Material cachedMaterial;

	private Shader shader;

	private Color color;

	private int validationIndex;

	private static int curValidationIndex;

	public Material Material
	{
		get
		{
			if (cachedMaterial == null || validationIndex != curValidationIndex)
			{
				if (texPath.NullOrEmpty())
				{
					cachedMaterial = BaseContent.BadMat;
				}
				else
				{
					cachedMaterial = MaterialPool.MatFrom(texPath, shader, color);
				}
				validationIndex = curValidationIndex;
			}
			return cachedMaterial;
		}
	}

	public string TexturePath => texPath;

	public Color Color => color;

	public CachedMaterial(string texPath, Shader shader, Color color)
	{
		this.texPath = texPath;
		this.shader = shader;
		this.color = color;
		cachedMaterial = null;
		validationIndex = -1;
	}

	public CachedMaterial(string texPath, Shader shader)
	{
		this.texPath = texPath;
		this.shader = shader;
		color = Color.white;
		cachedMaterial = null;
		validationIndex = -1;
	}

	public static void ResetStaticData()
	{
		curValidationIndex++;
	}
}
