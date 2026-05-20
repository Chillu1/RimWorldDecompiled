namespace Verse;

public struct DdsHeader
{
	public const uint RequiredMagic = 542327876u;

	public const uint RequiredSize = 124u;

	public uint Magic;

	public uint Size;

	public DdsHeaderFlags Flags;

	public uint Height;

	public uint Width;

	public uint PitchOrLinearSize;

	public uint Depth;

	public uint MipMapCount;

	public unsafe fixed uint Reserved[11];

	public DdsPixelFormat PixelFormat;

	public uint Caps;

	public uint Caps2;

	public uint Caps3;

	public uint Caps4;

	public uint Reserved2;
}
