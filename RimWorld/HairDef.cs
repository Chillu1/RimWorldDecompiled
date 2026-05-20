using UnityEngine;
using Verse;

namespace RimWorld;

public class HairDef : StyleItemDef
{
	public override Graphic GraphicFor(Pawn pawn, Color color)
	{
		if (noGraphic)
		{
			return null;
		}
		return GraphicDatabase.Get<Graphic_Multi>(texPath, overrideShaderTypeDef?.Shader ?? ShaderDatabase.CutoutHair, Vector2.one, color);
	}
}
