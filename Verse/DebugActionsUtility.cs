using System.Collections.Generic;

namespace Verse;

public static class DebugActionsUtility
{
	public static void DustPuffFrom(Thing t)
	{
		if (t is Pawn pawn)
		{
			pawn.Drawer.Notify_DebugAffected();
		}
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
			for (int i = 20; i < 100; i += 10)
			{
				yield return i;
			}
			for (int i = 100; i < 500; i += 25)
			{
				yield return i;
			}
			for (int i = 500; i < 1500; i += 50)
			{
				yield return i;
			}
			for (int i = 1500; i <= 5000; i += 100)
			{
				yield return i;
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
		for (int i = 1; i <= 20; i++)
		{
			yield return i;
		}
		for (int i = 30; i <= 50; i += 10)
		{
			yield return i;
		}
	}

	public static IEnumerable<int> RadiusOptions()
	{
		yield return 1;
		yield return 2;
		yield return 3;
		yield return 4;
		yield return 5;
		yield return 6;
		yield return 7;
		yield return 8;
		yield return 9;
		yield return 10;
		yield return 11;
		yield return 12;
		yield return 13;
		yield return 14;
		yield return 15;
		yield return 20;
		yield return 25;
		yield return 30;
	}
}
