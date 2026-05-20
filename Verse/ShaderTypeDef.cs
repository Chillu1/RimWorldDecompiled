using UnityEngine;

namespace Verse;

public class ShaderTypeDef : Def
{
	[NoTranslate]
	public string shaderPath;

	[NoTranslate]
	public string uiShaderPath;

	[Unsaved(false)]
	private Shader shaderInt;

	public Shader Shader
	{
		get
		{
			if ((object)shaderInt == null)
			{
				shaderInt = ShaderDatabase.LoadShader(this);
			}
			return shaderInt;
		}
	}
}
