using System;

namespace Verse
{
	[Flags]
	public enum AllowedGameStates
	{
		Invalid = 0x0,
		Entry = 0x1,
		Playing = 0x2,
		WorldRenderedNow = 0x4,
		IsCurrentlyOnMap = 0x8,
		HasGameCondition = 0x10,
		PlayingOnMap = 0xA,
		PlayingOnWorld = 0x6
	}
}
