using System;

namespace Verse;

[Flags]
public enum DdsPixelFormatFlags : uint
{
	None = 0u,
	AlphaPixels = 1u,
	Alpha = 2u,
	FourCC = 4u,
	RGB = 0x40u,
	YUV = 0x200u,
	Luminance = 0x20000u
}
