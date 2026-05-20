using System;

namespace Verse;

[Flags]
public enum PawnRenderFlags : uint
{
	None = 0u,
	Portrait = 1u,
	HeadStump = 2u,
	Invisible = 4u,
	DrawNow = 8u,
	Cache = 0x10u,
	Headgear = 0x20u,
	Clothes = 0x40u,
	NeverAimWeapon = 0x80u,
	StylingStation = 0x100u,
	NoBody = 0x200u,
	Statue = 0x400u
}
