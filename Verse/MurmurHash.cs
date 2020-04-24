namespace Verse
{
	public static class MurmurHash
	{
		private const uint Const1 = 3432918353u;

		private const uint Const2 = 461845907u;

		private const uint Const3 = 3864292196u;

		private const uint Const4Mix = 2246822507u;

		private const uint Const5Mix = 3266489909u;

		private const uint Const6StreamPosition = 2834544218u;

		public static int GetInt(uint seed, uint input)
		{
			uint num = input;
			num = (uint)((int)num * -862048943);
			num = ((num << 15) | (num >> 17));
			num *= 461845907;
			uint num2 = seed;
			num2 ^= num;
			num2 = ((num2 << 13) | (num2 >> 19));
			num2 = (uint)((int)(num2 * 5) + -430675100);
			num2 = (uint)((int)num2 ^ -1460423078);
			num2 ^= num2 >> 16;
			num2 = (uint)((int)num2 * -2048144789);
			num2 ^= num2 >> 13;
			num2 = (uint)((int)num2 * -1028477387);
			return (int)(num2 ^ (num2 >> 16));
		}
	}
}
