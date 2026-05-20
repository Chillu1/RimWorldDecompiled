using System;
using System.Text;
using Unity.Collections;

namespace Verse;

public static class DataExposeUtility
{
	private const int NewlineInterval = 100;

	public static void LookByteArray(ref byte[] arr, string label)
	{
		if (Scribe.mode == LoadSaveMode.Saving && arr != null)
		{
			byte[] array = CompressUtility.Compress(arr);
			if (array.Length < arr.Length)
			{
				string value = Convert.ToBase64String(array).AddLineBreaksToLongString();
				Scribe_Values.Look(ref value, label + "Deflate");
			}
			else
			{
				string value2 = Convert.ToBase64String(arr).AddLineBreaksToLongString();
				Scribe_Values.Look(ref value2, label);
			}
		}
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			return;
		}
		string value3 = null;
		Scribe_Values.Look(ref value3, label + "Deflate");
		if (value3 != null)
		{
			arr = CompressUtility.Decompress(Convert.FromBase64String(value3.RemoveLineBreaks()));
			return;
		}
		Scribe_Values.Look(ref value3, label);
		if (value3 != null)
		{
			arr = Convert.FromBase64String(value3.RemoveLineBreaks());
		}
		else
		{
			arr = null;
		}
	}

	public static void LookBoolArray(ref bool[] arr, int elements, string label)
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			if (arr.Length != elements)
			{
				Log.ErrorOnce("Bool array length mismatch for " + label, 74135877);
			}
			elements = arr.Length;
		}
		int numBytes = (elements + 7) / 8;
		byte[] arr2 = BytesToBits(arr, elements, numBytes);
		LookByteArray(ref arr2, label);
		BitsToBytes(ref arr, elements, arr2, numBytes);
	}

	public static void LookBoolArray(ref NativeArray<bool> arr, int elements, string label)
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			if (arr.Length != elements)
			{
				Log.ErrorOnce("Native bool array length mismatch for " + label, 74135822);
			}
			elements = arr.Length;
		}
		bool[] arr2 = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			arr2 = arr.ToArray();
		}
		LookBoolArray(ref arr2, elements, label);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (!arr.IsCreated)
			{
				arr = new NativeArray<bool>(elements, Allocator.Persistent);
			}
			arr.CopyFrom(arr2);
		}
	}

	public static void LookBitArray(ref NativeBitArray arr, int elements, string label)
	{
		bool[] arr2 = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			arr2 = new bool[elements];
			for (int i = 0; i < arr.Length; i++)
			{
				arr2[i] = arr.IsSet(i);
			}
		}
		LookBoolArray(ref arr2, elements, label);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (!arr.IsCreated)
			{
				arr = new NativeBitArray(elements, Allocator.Persistent);
			}
			for (int j = 0; j < arr2.Length; j++)
			{
				arr.Set(j, arr2[j]);
			}
		}
	}

	private static byte[] BytesToBits(bool[] arr, int elements, int numBytes)
	{
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			return null;
		}
		byte[] array = new byte[numBytes];
		int num = 0;
		byte b = 1;
		for (int i = 0; i < elements; i++)
		{
			if (arr[i])
			{
				array[num] |= b;
			}
			b *= 2;
			if (b == 0)
			{
				b = 1;
				num++;
			}
		}
		return array;
	}

	private static void BitsToBytes(ref bool[] arr, int elements, byte[] arrBytes, int numBytes)
	{
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			return;
		}
		if (arr == null)
		{
			arr = new bool[elements];
		}
		if (arrBytes == null || arrBytes.Length == 0)
		{
			return;
		}
		if (arrBytes.Length != numBytes)
		{
			int num = 0;
			byte b = 1;
			for (int i = 0; i < elements; i++)
			{
				arr[i] = (arrBytes[num] & b) != 0;
				b *= 2;
				if (b > 32)
				{
					b = 1;
					num++;
				}
			}
			return;
		}
		int num2 = 0;
		byte b2 = 1;
		for (int j = 0; j < elements; j++)
		{
			arr[j] = (arrBytes[num2] & b2) != 0;
			b2 *= 2;
			if (b2 == 0)
			{
				b2 = 1;
				num2++;
			}
		}
	}

	public static string AddLineBreaksToLongString(this string str)
	{
		StringBuilder stringBuilder = new StringBuilder(str.Length + (str.Length / 100 + 3) * 2 + 1);
		stringBuilder.AppendLine();
		for (int i = 0; i < str.Length; i++)
		{
			if (i % 100 == 0 && i != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(str[i]);
		}
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	public static string RemoveLineBreaks(this string str)
	{
		return new StringBuilder(str).Replace("\n", "").Replace("\r", "").ToString();
	}
}
