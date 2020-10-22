using System;
using System.Text;

namespace Verse
{
	public static class DataExposeUtility
	{
		private const int NewlineInterval = 100;

		public static void ByteArray(ref byte[] arr, string label)
		{
			if (Scribe.mode == LoadSaveMode.Saving && arr != null)
			{
				byte[] array = CompressUtility.Compress(arr);
				if (array.Length < arr.Length)
				{
					string value = AddLineBreaksToLongString(Convert.ToBase64String(array));
					Scribe_Values.Look(ref value, label + "Deflate");
				}
				else
				{
					string value2 = AddLineBreaksToLongString(Convert.ToBase64String(arr));
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
				arr = CompressUtility.Decompress(Convert.FromBase64String(RemoveLineBreaks(value3)));
				return;
			}
			Scribe_Values.Look(ref value3, label);
			if (value3 != null)
			{
				arr = Convert.FromBase64String(RemoveLineBreaks(value3));
			}
			else
			{
				arr = null;
			}
		}

		public static void BoolArray(ref bool[] arr, int elements, string label)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (arr.Length != elements)
				{
					Log.ErrorOnce($"Bool array length mismatch for {label}", 74135877);
				}
				elements = arr.Length;
			}
			int num = (elements + 7) / 8;
			byte[] arr2 = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				arr2 = new byte[num];
				int num2 = 0;
				byte b = 1;
				for (int i = 0; i < elements; i++)
				{
					if (arr[i])
					{
						arr2[num2] |= b;
					}
					b = (byte)(b * 2);
					if (b == 0)
					{
						b = 1;
						num2++;
					}
				}
			}
			ByteArray(ref arr2, label);
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				return;
			}
			if (arr == null)
			{
				arr = new bool[elements];
			}
			if (arr2 == null || arr2.Length == 0)
			{
				return;
			}
			if (arr2.Length != num)
			{
				int num3 = 0;
				byte b2 = 1;
				for (int j = 0; j < elements; j++)
				{
					arr[j] = (arr2[num3] & b2) != 0;
					b2 = (byte)(b2 * 2);
					if (b2 > 32)
					{
						b2 = 1;
						num3++;
					}
				}
				return;
			}
			int num4 = 0;
			byte b3 = 1;
			for (int k = 0; k < elements; k++)
			{
				arr[k] = (arr2[num4] & b3) != 0;
				b3 = (byte)(b3 * 2);
				if (b3 == 0)
				{
					b3 = 1;
					num4++;
				}
			}
		}

		public static string AddLineBreaksToLongString(string str)
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

		public static string RemoveLineBreaks(string str)
		{
			return str.Replace("\n", "").Replace("\r", "");
		}
	}
}
