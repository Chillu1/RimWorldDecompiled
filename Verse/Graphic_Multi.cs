using UnityEngine;

namespace Verse;

public class Graphic_Multi : Graphic
{
	private Material[] mats = new Material[4];

	private bool westFlipped;

	private bool eastFlipped;

	private float drawRotatedExtraAngleOffset;

	public const string NorthSuffix = "_north";

	public const string SouthSuffix = "_south";

	public const string EastSuffix = "_east";

	public const string WestSuffix = "_west";

	public string GraphicPath => path;

	public override Material MatSingle => MatSouth;

	public override Material MatWest => mats[3];

	public override Material MatSouth => mats[2];

	public override Material MatEast => mats[1];

	public override Material MatNorth => mats[0];

	public override bool WestFlipped => westFlipped;

	public override bool EastFlipped => eastFlipped;

	public override bool ShouldDrawRotated
	{
		get
		{
			if (data != null && !data.drawRotated)
			{
				return false;
			}
			if (!(MatEast == MatNorth))
			{
				return MatWest == MatNorth;
			}
			return true;
		}
	}

	public override float DrawRotatedExtraAngleOffset => drawRotatedExtraAngleOffset;

	public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
	{
		Material[] array = mats;
		foreach (Material material in array)
		{
			Texture2D mask = null;
			if (material.HasProperty(ShaderPropertyIDs.MaskTex))
			{
				mask = (Texture2D)material.GetTexture(ShaderPropertyIDs.MaskTex);
			}
			GlobalTextureAtlasManager.TryInsertStatic(groupKey, (Texture2D)material.mainTexture, mask);
		}
	}

	public override void Init(GraphicRequest req)
	{
		data = req.graphicData;
		path = req.path;
		maskPath = req.maskPath;
		color = req.color;
		colorTwo = req.colorTwo;
		drawSize = req.drawSize;
		Texture2D[] array = new Texture2D[mats.Length];
		array[0] = ContentFinder<Texture2D>.Get(req.path + "_north", reportFailure: false);
		array[1] = ContentFinder<Texture2D>.Get(req.path + "_east", reportFailure: false);
		array[2] = ContentFinder<Texture2D>.Get(req.path + "_south", reportFailure: false);
		array[3] = ContentFinder<Texture2D>.Get(req.path + "_west", reportFailure: false);
		if (array[0] == null)
		{
			if (array[2] != null)
			{
				array[0] = array[2];
				drawRotatedExtraAngleOffset = 180f;
			}
			else if (array[1] != null)
			{
				array[0] = array[1];
				drawRotatedExtraAngleOffset = -90f;
			}
			else if (array[3] != null)
			{
				array[0] = array[3];
				drawRotatedExtraAngleOffset = 90f;
			}
			else
			{
				array[0] = ContentFinder<Texture2D>.Get(req.path, reportFailure: false);
			}
		}
		if (array[0] == null)
		{
			Log.Error("Failed to find any textures at " + req.path + " while constructing " + this.ToStringSafe());
			mats[0] = (mats[1] = (mats[2] = (mats[3] = BaseContent.BadMat)));
			return;
		}
		if (array[2] == null)
		{
			array[2] = array[0];
		}
		if (array[1] == null)
		{
			if (array[3] != null)
			{
				array[1] = array[3];
				eastFlipped = base.DataAllowsFlip;
			}
			else
			{
				array[1] = array[0];
			}
		}
		if (array[3] == null)
		{
			if (array[1] != null)
			{
				array[3] = array[1];
				westFlipped = base.DataAllowsFlip;
			}
			else
			{
				array[3] = array[0];
			}
		}
		Texture2D[] array2 = new Texture2D[mats.Length];
		if (req.shader.SupportsMaskTex())
		{
			string text = (maskPath.NullOrEmpty() ? path : maskPath);
			string text2 = (maskPath.NullOrEmpty() ? "m" : string.Empty);
			array2[0] = ContentFinder<Texture2D>.Get(text + "_north" + text2, reportFailure: false);
			array2[1] = ContentFinder<Texture2D>.Get(text + "_east" + text2, reportFailure: false);
			array2[2] = ContentFinder<Texture2D>.Get(text + "_south" + text2, reportFailure: false);
			array2[3] = ContentFinder<Texture2D>.Get(text + "_west" + text2, reportFailure: false);
			if (array2[0] == null)
			{
				if (array2[2] != null)
				{
					array2[0] = array2[2];
				}
				else if (array2[1] != null)
				{
					array2[0] = array2[1];
				}
				else if (array2[3] != null)
				{
					array2[0] = array2[3];
				}
			}
			if (array2[2] == null)
			{
				array2[2] = array2[0];
			}
			if (array2[1] == null)
			{
				if (array2[3] != null)
				{
					array2[1] = array2[3];
				}
				else
				{
					array2[1] = array2[0];
				}
			}
			if (array2[3] == null)
			{
				if (array2[1] != null)
				{
					array2[3] = array2[1];
				}
				else
				{
					array2[3] = array2[0];
				}
			}
		}
		for (int i = 0; i < mats.Length; i++)
		{
			MaterialRequest req2 = new MaterialRequest
			{
				mainTex = array[i],
				shader = req.shader,
				color = color,
				colorTwo = colorTwo,
				maskTex = array2[i],
				shaderParameters = req.shaderParameters,
				renderQueue = req.renderQueue
			};
			mats[i] = MaterialPool.MatFrom(req2);
		}
	}

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		return GraphicDatabase.Get<Graphic_Multi>(path, newShader, drawSize, newColor, newColorTwo, data, maskPath);
	}

	public override string ToString()
	{
		string[] obj = new string[7] { "Multi(initPath=", path, ", color=", null, null, null, null };
		Color color = base.color;
		obj[3] = color.ToString();
		obj[4] = ", colorTwo=";
		color = colorTwo;
		obj[5] = color.ToString();
		obj[6] = ")";
		return string.Concat(obj);
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineStruct(Gen.HashCombineStruct(Gen.HashCombine(0, path), color), colorTwo);
	}
}
