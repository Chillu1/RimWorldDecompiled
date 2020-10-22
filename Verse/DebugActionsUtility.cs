using System.Collections.Generic;

namespace Verse
{
	public static class DebugActionsUtility
	{
		public static void DustPuffFrom(Thing t)
		{
			(t as Pawn)?.Drawer.Notify_DebugAffected();
		}

		public static IEnumerable<float> PointsOptions(bool extended)
		{
			if (!extended)
			{
				yield return 35f;
				yield return 70f;
				yield return 100f;
				yield return 150f;
				yield return 200f;
				yield return 350f;
				yield return 500f;
				yield return 700f;
				yield return 1000f;
				yield return 1200f;
				yield return 1500f;
				yield return 2000f;
				yield return 3000f;
				yield return 4000f;
				yield return 5000f;
			}
			else
			{
				for (int l = 20; l < 100; l += 10)
				{
					yield return l;
				}
				for (int l = 100; l < 500; l += 25)
				{
					yield return l;
				}
				for (int l = 500; l < 1500; l += 50)
				{
					yield return l;
				}
				for (int l = 1500; l <= 5000; l += 100)
				{
					yield return l;
				}
			}
			yield return 6000f;
			yield return 7000f;
			yield return 8000f;
			yield return 9000f;
			yield return 10000f;
		}

		public static IEnumerable<int> PopulationOptions()
		{
			for (int j = 1; j <= 20; j++)
			{
				yield return j;
			}
			for (int j = 30; j <= 50; j += 10)
			{
				yield return j;
			}
		}
	}
}
