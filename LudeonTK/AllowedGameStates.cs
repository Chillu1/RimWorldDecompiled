using System;

namespace LudeonTK;

[Flags]
public enum AllowedGameStates
{
	Invalid = 0,
	Entry = 1,
	Playing = 2,
	WorldRenderedNow = 4,
	IsCurrentlyOnMap = 8,
	HasGameCondition = 0x10,
	PlayingOnMap = 0xA,
	PlayingOnWorld = 6
}
