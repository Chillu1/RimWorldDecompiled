using System;

namespace RimWorld;

[Flags]
public enum PawnPosture : byte
{
	Standing = 0,
	LayingOnGroundFaceUp = 3,
	LayingOnGroundNormal = 1,
	LayingInBed = 5,
	LayingInBedFaceUp = 7,
	LayingMask = 1,
	FaceUpMask = 2,
	InBedMask = 4
}
