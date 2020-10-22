using System;

namespace Verse
{
	public static class DataSerializeUtility
	{
		public static byte[] SerializeByte(int elements, Func<int, byte> reader)
		{
			byte[] array = new byte[elements];
			for (int i = 0; i < elements; i++)
			{
				array[i] = reader(i);
			}
			return array;
		}

		public static byte[] SerializeByte(byte[] data)
		{
			return data;
		}

		public static byte[] DeserializeByte(byte[] data)
		{
			return data;
		}

		public static void LoadByte(byte[] arr, int elements, Action<int, byte> writer)
		{
			if (arr != null && arr.Length != 0)
			{
				for (int i = 0; i < elements; i++)
				{
					writer(i, arr[i]);
				}
			}
		}

		public static byte[] SerializeUshort(int elements, Func<int, ushort> reader)
		{
			byte[] array = new byte[elements * 2];
			for (int i = 0; i < elements; i++)
			{
				ushort num = reader(i);
				array[i * 2] = (byte)(num & 0xFFu);
				array[i * 2 + 1] = (byte)((uint)(num >> 8) & 0xFFu);
			}
			return array;
		}

		public static byte[] SerializeUshort(ushort[] data)
		{
			return SerializeUshort(data.Length, (int i) => data[i]);
		}

		public static ushort[] DeserializeUshort(byte[] data)
		{
			ushort[] result = new ushort[data.Length / 2];
			LoadUshort(data, result.Length, delegate(int i, ushort dat)
			{
				result[i] = dat;
			});
			return result;
		}

		public static void LoadUshort(byte[] arr, int elements, Action<int, ushort> writer)
		{
			if (arr != null && arr.Length != 0)
			{
				for (int i = 0; i < elements; i++)
				{
					writer(i, (ushort)(arr[i * 2] | (arr[i * 2 + 1] << 8)));
				}
			}
		}

		public static byte[] SerializeInt(int elements, Func<int, int> reader)
		{
			byte[] array = new byte[elements * 4];
			for (int i = 0; i < elements; i++)
			{
				int num = reader(i);
				array[i * 4] = (byte)((uint)num & 0xFFu);
				array[i * 4 + 1] = (byte)((uint)(num >> 8) & 0xFFu);
				array[i * 4 + 2] = (byte)((uint)(num >> 16) & 0xFFu);
				array[i * 4 + 3] = (byte)((uint)(num >> 24) & 0xFFu);
			}
			return array;
		}

		public static byte[] SerializeInt(int[] data)
		{
			return SerializeInt(data.Length, (int i) => data[i]);
		}

		public static int[] DeserializeInt(byte[] data)
		{
			int[] result = new int[data.Length / 4];
			LoadInt(data, result.Length, delegate(int i, int dat)
			{
				result[i] = dat;
			});
			return result;
		}

		public static void LoadInt(byte[] arr, int elements, Action<int, int> writer)
		{
			if (arr != null && arr.Length != 0)
			{
				for (int i = 0; i < elements; i++)
				{
					writer(i, arr[i * 4] | (arr[i * 4 + 1] << 8) | (arr[i * 4 + 2] << 16) | (arr[i * 4 + 3] << 24));
				}
			}
		}
	}
}
