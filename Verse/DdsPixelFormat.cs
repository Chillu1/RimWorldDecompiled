using System;
using UnityEngine;

namespace Verse;

public struct DdsPixelFormat
{
	public const uint RequiredSize = 32u;

	public uint Size;

	public DdsPixelFormatFlags Flags;

	public uint FourCC;

	public uint RGBBitCount;

	public uint RBitMask;

	public uint GBitMask;

	public uint BBitMask;

	public uint ABitMask;

	public bool IsRgb888
	{
		get
		{
			if (RBitMask == 255 && GBitMask == 65280)
			{
				return BBitMask == 16711680;
			}
			return false;
		}
	}

	public bool IsBgr888
	{
		get
		{
			if (RBitMask == 16711680 && GBitMask == 65280)
			{
				return BBitMask == 255;
			}
			return false;
		}
	}

	public bool IsRgb565
	{
		get
		{
			if (RBitMask == 63488 && GBitMask == 2016)
			{
				return BBitMask == 31;
			}
			return false;
		}
	}

	public bool IsArgb4444
	{
		get
		{
			if (ABitMask == 61440 && RBitMask == 3840 && GBitMask == 240)
			{
				return BBitMask == 15;
			}
			return false;
		}
	}

	public bool IsRgba4444
	{
		get
		{
			if (RBitMask == 61440 && GBitMask == 3840 && BBitMask == 240)
			{
				return ABitMask == 15;
			}
			return false;
		}
	}

	public bool IsCompressed
	{
		get
		{
			if ((Flags & DdsPixelFormatFlags.FourCC) != DdsPixelFormatFlags.None)
			{
				return FourCC != 0;
			}
			return false;
		}
	}

	public bool IsDxt1
	{
		get
		{
			if (IsCompressed)
			{
				return FourCC == 827611204;
			}
			return false;
		}
	}

	public bool IsDxt5
	{
		get
		{
			if (IsCompressed)
			{
				return FourCC == 894720068;
			}
			return false;
		}
	}

	public bool IsBc7
	{
		get
		{
			if (IsCompressed)
			{
				return FourCC == 808540228;
			}
			return false;
		}
	}

	public bool IsUnsupportedCompressedFormat
	{
		get
		{
			if (IsCompressed && !IsDxt1 && !IsDxt5)
			{
				return !IsBc7;
			}
			return false;
		}
	}

	public TextureFormat ToTextureFormat()
	{
		if (IsCompressed)
		{
			if (IsDxt1)
			{
				return TextureFormat.DXT1;
			}
			if (IsDxt5)
			{
				return TextureFormat.DXT5;
			}
			if (IsBc7)
			{
				return TextureFormat.BC7;
			}
			throw new NotSupportedException($"Unsupported compressed DDS texture format: {FourCC:X8}");
		}
		if ((Flags & DdsPixelFormatFlags.RGB) != DdsPixelFormatFlags.None)
		{
			bool flag = (Flags & DdsPixelFormatFlags.AlphaPixels) != 0;
			if (IsRgb888 || IsBgr888)
			{
				if (!flag)
				{
					return TextureFormat.RGB24;
				}
				return TextureFormat.RGBA32;
			}
			if (IsRgb565)
			{
				return TextureFormat.RGB565;
			}
			if (IsRgba4444)
			{
				if (!flag)
				{
					throw new NotSupportedException("RGBA4444 format without alpha is not supported.");
				}
				return TextureFormat.ARGB4444;
			}
			throw new NotSupportedException($"Unsupported DDS RGB pixel format: {Flags} with FourCC {FourCC:X8}");
		}
		if ((Flags & DdsPixelFormatFlags.Luminance) != DdsPixelFormatFlags.None || (Flags & DdsPixelFormatFlags.Alpha) != DdsPixelFormatFlags.None)
		{
			return TextureFormat.Alpha8;
		}
		throw new NotSupportedException($"Unsupported DDS pixel format: {Flags} with FourCC {FourCC:X8}");
	}
}
