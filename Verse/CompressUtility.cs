using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;

namespace Verse;

public static class CompressUtility
{
	public static byte[] Compress(byte[] input)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			deflateStream.Write(input, 0, input.Length);
		}
		return memoryStream.ToArray();
	}

	public static byte[] Compress(ReadOnlySpan<byte> input)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			deflateStream.Write(input);
		}
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] input)
	{
		using MemoryStream stream = new MemoryStream(input);
		using DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
		List<byte[]> list = null;
		byte[] array;
		int num;
		while (true)
		{
			array = new byte[65536];
			num = deflateStream.Read(array, 0, array.Length);
			if (num < array.Length && list == null)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, array2, num);
				return array2;
			}
			if (num < array.Length)
			{
				break;
			}
			if (list == null)
			{
				list = new List<byte[]>();
			}
			list.Add(array);
		}
		byte[] array3 = new byte[num + list.Count * array.Length];
		for (int i = 0; i < list.Count; i++)
		{
			Array.Copy(list[i], 0, array3, i * array.Length, array.Length);
		}
		Array.Copy(array, 0, array3, list.Count * array.Length, num);
		return array3;
	}
}
