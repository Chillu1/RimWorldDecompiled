using System;

namespace Verse
{
	public static class AutomaticPauseModeExtension
	{
		public static string ToStringHuman(this AutomaticPauseMode mode)
		{
			switch (mode)
			{
			case AutomaticPauseMode.Never:
				return "AutomaticPauseMode_Never".Translate();
			case AutomaticPauseMode.MajorThreat:
				return "AutomaticPauseMode_MajorThreat".Translate();
			case AutomaticPauseMode.AnyThreat:
				return "AutomaticPauseMode_AnyThreat".Translate();
			case AutomaticPauseMode.AnyLetter:
				return "AutomaticPauseMode_AnyLetter".Translate();
			default:
				throw new NotImplementedException();
			}
		}
	}
}
