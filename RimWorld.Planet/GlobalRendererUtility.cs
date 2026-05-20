using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class GlobalRendererUtility
{
	public static void UpdateGlobalShadersParams()
	{
		Shader.SetGlobalFloat(ShaderPropertyIDs.GameTime, ((float?)Find.TickManager?.TicksGame / 60f).GetValueOrDefault());
	}
}
