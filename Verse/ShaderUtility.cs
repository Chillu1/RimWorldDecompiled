using UnityEngine;

namespace Verse;

public static class ShaderUtility
{
	public static bool SupportsMaskTex(this Shader shader)
	{
		int propertyCount = shader.GetPropertyCount();
		for (int i = 0; i < propertyCount; i++)
		{
			if (shader.GetPropertyNameId(i) == ShaderPropertyIDs.MaskTex)
			{
				return true;
			}
		}
		return false;
	}

	public static Shader GetSkinShader(Pawn pawn)
	{
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			if (hediff.def.skinShader != null)
			{
				return hediff.def.skinShader.Shader;
			}
		}
		bool dead = pawn.Dead || (pawn.IsMutant && pawn.mutant.HasTurned && pawn.mutant.Def.useCorpseGraphics);
		return GetSkinShaderAbstract(pawn.story != null && pawn.story.SkinColorOverriden, dead);
	}

	public static Shader GetSkinShaderAbstract(bool skinColorOverriden, bool dead)
	{
		if (skinColorOverriden)
		{
			return ShaderDatabase.CutoutSkinColorOverride;
		}
		if (dead)
		{
			return ShaderDatabase.Cutout;
		}
		return ShaderDatabase.CutoutSkin;
	}
}
