using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ionic.Crc;

[Guid("ebc25cf6-9120-4283-b972-0e5520d0000C")]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDispatch)]
public class CRC32
{
	private uint dwPolynomial;

	private long _TotalBytesRead;

	private bool reverseBits;

	private uint[] crc32Table;

	private const int BUFFER_SIZE = 8192;

	private uint _register = uint.MaxValue;

	public long TotalBytesRead => _TotalBytesRead;

	public int Crc32Result => (int)(~_register);

	public int GetCrc32(Stream input)
	{
		return GetCrc32AndCopy(input, null);
	}

	public int GetCrc32AndCopy(Stream input, Stream output)
	{
		if (input == null)
		{
			throw new Exception("The input stream must not be null.");
		}
		byte[] array = new byte[8192];
		int count = 8192;
		_TotalBytesRead = 0L;
		int num = input.Read(array, 0, count);
		output?.Write(array, 0, num);
		_TotalBytesRead += num;
		while (num > 0)
		{
			SlurpBlock(array, 0, num);
			num = input.Read(array, 0, count);
			output?.Write(array, 0, num);
			_TotalBytesRead += num;
		}
		return (int)(~_register);
	}

	public int ComputeCrc32(int W, byte B)
	{
		return _InternalComputeCrc32((uint)W, B);
	}

	internal int _InternalComputeCrc32(uint W, byte B)
	{
		return (int)(crc32Table[(W ^ B) & 0xFF] ^ (W >> 8));
	}

	public void SlurpBlock(byte[] block, int offset, int count)
	{
		if (block == null)
		{
			throw new Exception("The data buffer must not be null.");
		}
		for (int i = 0; i < count; i++)
		{
			int num = offset + i;
			byte b = block[num];
			if (reverseBits)
			{
				uint num2 = (_register >> 24) ^ b;
				_register = (_register << 8) ^ crc32Table[num2];
			}
			else
			{
				uint num3 = (_register & 0xFF) ^ b;
				_register = (_register >> 8) ^ crc32Table[num3];
			}
		}
		_TotalBytesRead += count;
	}

	public void UpdateCRC(byte b)
	{
		if (reverseBits)
		{
			uint num = (_register >> 24) ^ b;
			_register = (_register << 8) ^ crc32Table[num];
		}
		else
		{
			uint num2 = (_register & 0xFF) ^ b;
			_register = (_register >> 8) ^ crc32Table[num2];
		}
	}

	public void UpdateCRC(byte b, int n)
	{
		while (n-- > 0)
		{
			if (reverseBits)
			{
				uint num = (_register >> 24) ^ b;
				_register = (_register << 8) ^ crc32Table[(num >= 0) ? num : (num + 256)];
			}
			else
			{
				uint num2 = (_register & 0xFF) ^ b;
				_register = (_register >> 8) ^ crc32Table[(num2 >= 0) ? num2 : (num2 + 256)];
			}
		}
	}

	private static uint ReverseBits(uint data)
	{
		uint num = data;
		num = ((num & 0x55555555) << 1) | ((num >> 1) & 0x55555555);
		num = ((num & 0x33333333) << 2) | ((num >> 2) & 0x33333333);
		num = ((num & 0xF0F0F0F) << 4) | ((num >> 4) & 0xF0F0F0F);
		return (num << 24) | ((num & 0xFF00) << 8) | ((num >> 8) & 0xFF00) | (num >> 24);
	}

	private static byte ReverseBits(byte data)
	{
		int num = data * 131586;
		uint num2 = 17055760u;
		uint num3 = (uint)num & num2;
		uint num4 = (uint)(num << 2) & (num2 << 1);
		return (byte)(16781313 * (num3 + num4) >> 24);
	}

	private void GenerateLookupTable()
	{
		crc32Table = new uint[256];
		byte b = 0;
		do
		{
			uint num = b;
			for (byte b2 = 8; b2 > 0; b2--)
			{
				num = (((num & 1) != 1) ? (num >> 1) : ((num >> 1) ^ dwPolynomial));
			}
			if (reverseBits)
			{
				crc32Table[ReverseBits(b)] = ReverseBits(num);
			}
			else
			{
				crc32Table[b] = num;
			}
			b++;
		}
		while (b != 0);
	}

	private uint gf2_matrix_times(uint[] matrix, uint vec)
	{
		uint num = 0u;
		int num2 = 0;
		while (vec != 0)
		{
			if ((vec & 1) == 1)
			{
				num ^= matrix[num2];
			}
			vec >>= 1;
			num2++;
		}
		return num;
	}

	private void gf2_matrix_square(uint[] square, uint[] mat)
	{
		for (int i = 0; i < 32; i++)
		{
			square[i] = gf2_matrix_times(mat, mat[i]);
		}
	}

	public void Combine(int crc, int length)
	{
		uint[] array = new uint[32];
		uint[] array2 = new uint[32];
		if (length == 0)
		{
			return;
		}
		uint num = ~_register;
		array2[0] = dwPolynomial;
		uint num2 = 1u;
		for (int i = 1; i < 32; i++)
		{
			array2[i] = num2;
			num2 <<= 1;
		}
		gf2_matrix_square(array, array2);
		gf2_matrix_square(array2, array);
		uint num3 = (uint)length;
		do
		{
			gf2_matrix_square(array, array2);
			if ((num3 & 1) == 1)
			{
				num = gf2_matrix_times(array, num);
			}
			num3 >>= 1;
			if (num3 == 0)
			{
				break;
			}
			gf2_matrix_square(array2, array);
			if ((num3 & 1) == 1)
			{
				num = gf2_matrix_times(array2, num);
			}
			num3 >>= 1;
		}
		while (num3 != 0);
		num ^= (uint)crc;
		_register = ~num;
	}

	public CRC32()
		: this(reverseBits: false)
	{
	}

	public CRC32(bool reverseBits)
		: this(-306674912, reverseBits)
	{
	}

	public CRC32(int polynomial, bool reverseBits)
	{
		this.reverseBits = reverseBits;
		dwPolynomial = (uint)polynomial;
		GenerateLookupTable();
	}

	public void Reset()
	{
		_register = uint.MaxValue;
	}
}
