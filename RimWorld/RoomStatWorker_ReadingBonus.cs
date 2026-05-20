using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomStatWorker_ReadingBonus : RoomStatWorker
{
	private const float MaxEnhancement = 0.2f;

	private static readonly List<float> CellFilledFactor = new List<float> { 0.04f, 0.02f, 0.01f, 0.005f };

	public override float GetScore(Room room)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (Building_Bookcase item in room.ContainedThings<Building_Bookcase>())
		{
			foreach (float item2 in item.CellsFilledPercentage)
			{
				num2 += item2;
			}
		}
		int num3 = 0;
		while (num2 > 0f && num < 0.2f)
		{
			float num4 = ((num2 >= 1f) ? 1f : num2);
			num2 -= num4;
			num += num4 * CellFilledFactor[Mathf.Min(num3++, CellFilledFactor.Count - 1)];
		}
		return 1f + Mathf.Min(num, 0.2f);
	}
}
