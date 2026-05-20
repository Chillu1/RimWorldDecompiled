using System;

namespace Verse;

public static class AutomaticPauseModeExtension
{
	public static string ToStringHuman(this AutomaticPauseMode mode)
	{
		return mode switch
		{
			AutomaticPauseMode.Never => "AutomaticPauseMode_Never".Translate(), 
			AutomaticPauseMode.MajorThreat => "AutomaticPauseMode_MajorThreat".Translate(), 
			AutomaticPauseMode.AnyThreat => "AutomaticPauseMode_AnyThreat".Translate(), 
			AutomaticPauseMode.AnyLetter => "AutomaticPauseMode_AnyLetter".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}
}
