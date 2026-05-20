using System;
using System.Buffers;
using System.IO;
using RimWorld.IO;
using UnityEngine;

namespace Verse;

public static class ModDdsLoader
{
	public static Texture2D TryLoadDds(VirtualFile file)
	{
		using MemoryMappedFileSpanWrapper memoryMappedFileSpanWrapper = new MemoryMappedFileSpanWrapper(new FileInfo(((file as FilesystemFile) ?? throw new NotSupportedException("ModDdsLoader only supports FilesystemFile types.")).FullPath), suppressExistsCheck: true);
		DdsHeader header = memoryMappedFileSpanWrapper.Read<DdsHeader>(0L);
		if (header.Magic != 542327876)
		{
			throw new InvalidDataException($"Invalid DDS magic number: {header.Magic:X8}. Expected: {542327876u:X8}");
		}
		if (header.Size != 124)
		{
			throw new InvalidDataException($"Invalid DDS header size: {header.Size}. Expected: {124u}");
		}
		if (header.PixelFormat.Size != 32)
		{
			throw new InvalidDataException($"Invalid DDS pixel format size: {header.PixelFormat.Size}. Expected: {32u}");
		}
		TextureFormat textureFormat = header.PixelFormat.ToTextureFormat();
		int num = ((textureFormat == TextureFormat.BC7) ? 148 : 128);
		if (header.PixelFormat.IsBgr888 && !header.PixelFormat.IsCompressed)
		{
			Span<byte> span = memoryMappedFileSpanWrapper.GetSpan(num);
			byte[] array = ArrayPool<byte>.Shared.Rent(span.Length);
			Span<byte> span2 = new Span<byte>(array, 0, span.Length);
			span.CopyTo(span2);
			int num2 = (int)header.PixelFormat.RGBBitCount / 8;
			for (int i = 0; i < span.Length; i += num2)
			{
				ref byte reference = ref span2[i];
				ref byte reference2 = ref span2[i + 2];
				byte b = span2[i + 2];
				byte b2 = span2[i];
				reference = b;
				reference2 = b2;
			}
			Texture2D result = CreateTexture(file, header, textureFormat, span2);
			ArrayPool<byte>.Shared.Return(array);
			return result;
		}
		return CreateTexture(file, header, textureFormat, memoryMappedFileSpanWrapper.GetSpan(num));
	}

	private unsafe static Texture2D CreateTexture(VirtualFile file, DdsHeader header, TextureFormat format, Span<byte> data)
	{
		bool flag = (header.Flags & DdsHeaderFlags.MipMapCount) != 0 && header.MipMapCount > 1;
		int mipCount = (int)((!flag) ? 1 : header.MipMapCount);
		Texture2D texture2D = new Texture2D((int)header.Width, (int)header.Height, format, mipCount, linear: false, createUninitialized: true);
		fixed (byte* ptr = data)
		{
			texture2D.LoadRawTextureData((IntPtr)ptr, data.Length);
		}
		texture2D.name = Path.GetFileNameWithoutExtension(file.Name);
		texture2D.filterMode = FilterMode.Trilinear;
		texture2D.anisoLevel = 0;
		texture2D.Apply(!flag, makeNoLongerReadable: true);
		return texture2D;
	}
}
