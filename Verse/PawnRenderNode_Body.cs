using UnityEngine;

namespace Verse;

public class PawnRenderNode_Body : PawnRenderNode
{
	public PawnRenderNode_Body(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		Shader shader = ShaderFor(pawn);
		if (shader == null)
		{
			return null;
		}
		if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
		{
			return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, shader);
		}
		if (pawn.IsMutant && !pawn.mutant.Def.bodyTypeGraphicPaths.NullOrEmpty())
		{
			string bodyGraphicPath = pawn.mutant.Def.GetBodyGraphicPath(pawn);
			if (bodyGraphicPath != null)
			{
				return GraphicDatabase.Get<Graphic_Multi>(bodyGraphicPath, shader, Vector2.one, ColorFor(pawn));
			}
		}
		if (ModsConfig.AnomalyActive && pawn.IsCreepJoiner && pawn.story.bodyType != null && !pawn.creepjoiner.form.bodyTypeGraphicPaths.NullOrEmpty())
		{
			return GraphicDatabase.Get<Graphic_Multi>(pawn.creepjoiner.form.GetBodyGraphicPath(pawn), shader, Vector2.one, ColorFor(pawn));
		}
		if (pawn.story?.bodyType?.bodyNakedGraphicPath == null)
		{
			return null;
		}
		return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, shader, Vector2.one, ColorFor(pawn));
	}
}
