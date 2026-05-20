using UnityEngine;
using Verse;

namespace RimWorld;

public class TattooDef : StyleItemDef
{
	public TattooType tattooType;

	public bool visibleNorth = true;

	public override Graphic GraphicFor(Pawn pawn, Color color)
	{
		if (noGraphic)
		{
			return null;
		}
		string maskPath = ((tattooType == TattooType.Body) ? pawn.story.bodyType.bodyNakedGraphicPath : pawn.story.headType.graphicPath);
		return GraphicDatabase.Get<Graphic_Multi>(texPath, overrideShaderTypeDef?.Shader ?? ShaderDatabase.CutoutSkinOverlay, Vector2.one, color, Color.white, null, maskPath);
	}
}
