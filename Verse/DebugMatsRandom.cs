using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class DebugMatsRandom
	{
		private static readonly Material[] mats;

		public const int MaterialCount = 100;

		private const float Opacity = 0.25f;

		static DebugMatsRandom()
		{
			mats = new Material[100];
			for (int i = 0; i < 100; i++)
			{
				mats[i] = SolidColorMaterials.SimpleSolidColorMaterial(new Color(Rand.Value, Rand.Value, Rand.Value, 0.25f));
			}
		}

		public static Material Mat(int ind)
		{
			ind %= 100;
			if (ind < 0)
			{
				ind *= -1;
			}
			return mats[ind];
		}
	}
}
