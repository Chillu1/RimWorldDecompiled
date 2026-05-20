using System;
using RimWorld.Planet;
using Verse.Sound;

namespace Verse;

public static class SoundDefHelper
{
	public static bool NullOrUndefined(this SoundDef def)
	{
		return def?.isUndefined ?? true;
	}

	public static bool CorrectContextNow(SoundDef def, Map sourceMap)
	{
		if (sourceMap != null && (Find.CurrentMap != sourceMap || WorldRendererUtility.WorldSelected))
		{
			return false;
		}
		switch (def.context)
		{
		case SoundContext.Any:
			return true;
		case SoundContext.MapOnly:
			if (Current.ProgramState == ProgramState.Playing)
			{
				return WorldRendererUtility.DrawingMap;
			}
			return false;
		case SoundContext.WorldOnly:
			return WorldRendererUtility.WorldSelected;
		default:
			throw new NotImplementedException();
		}
	}
}
