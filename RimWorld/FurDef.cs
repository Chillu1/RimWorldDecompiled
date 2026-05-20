using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class FurDef : StyleItemDef
{
	public List<BodyTypeGraphicData> bodyTypeGraphicPaths;

	public override Graphic GraphicFor(Pawn pawn, Color color)
	{
		if (noGraphic)
		{
			return null;
		}
		return GraphicDatabase.Get<Graphic_Multi>(GetFurBodyGraphicPath(pawn), overrideShaderTypeDef?.Shader ?? ShaderDatabase.CutoutSkinOverlay, Vector2.one, color);
	}

	public string GetFurBodyGraphicPath(Pawn pawn)
	{
		for (int i = 0; i < bodyTypeGraphicPaths.Count; i++)
		{
			if (bodyTypeGraphicPaths[i].bodyType == pawn.story.bodyType)
			{
				return bodyTypeGraphicPaths[i].texturePath;
			}
		}
		return null;
	}
}
