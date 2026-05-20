using UnityEngine;

namespace Verse;

public class Graphic_Single : Graphic
{
	protected Material mat;

	public static readonly string MaskSuffix = "_m";

	public override Material MatSingle => mat;

	public override Material MatWest => mat;

	public override Material MatSouth => mat;

	public override Material MatEast => mat;

	public override Material MatNorth => mat;

	public override bool ShouldDrawRotated
	{
		get
		{
			if (data != null && !data.drawRotated)
			{
				return false;
			}
			return true;
		}
	}

	public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
	{
		Texture2D mask = null;
		if (mat.HasProperty(ShaderPropertyIDs.MaskTex))
		{
			mask = (Texture2D)mat.GetTexture(ShaderPropertyIDs.MaskTex);
		}
		GlobalTextureAtlasManager.TryInsertStatic(groupKey, (Texture2D)mat.mainTexture, mask);
	}

	public override void Init(GraphicRequest req)
	{
		data = req.graphicData;
		path = req.path;
		maskPath = req.maskPath;
		color = req.color;
		colorTwo = req.colorTwo;
		drawSize = req.drawSize;
		MaterialRequest materialRequest = new MaterialRequest(req.texture ?? ContentFinder<Texture2D>.Get(req.path), req.shader, color);
		materialRequest.colorTwo = colorTwo;
		materialRequest.renderQueue = req.renderQueue;
		materialRequest.shaderParameters = req.shaderParameters;
		MaterialRequest req2 = materialRequest;
		if (req.shader.SupportsMaskTex())
		{
			req2.maskTex = ContentFinder<Texture2D>.Get(maskPath.NullOrEmpty() ? (path + MaskSuffix) : maskPath, reportFailure: false);
		}
		mat = MaterialPool.MatFrom(req2);
	}

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		return GraphicDatabase.Get<Graphic_Single>(path, newShader, drawSize, newColor, newColorTwo, data);
	}

	public override Material MatAt(Rot4 rot, Thing thing = null)
	{
		return mat;
	}

	public override string ToString()
	{
		string[] obj = new string[7] { "Single(path=", path, ", color=", null, null, null, null };
		Color color = base.color;
		obj[3] = color.ToString();
		obj[4] = ", colorTwo=";
		color = colorTwo;
		obj[5] = color.ToString();
		obj[6] = ")";
		return string.Concat(obj);
	}
}
