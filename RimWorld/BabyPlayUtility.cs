using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class BabyPlayUtility
{
	private const float PlayPerTick = 0.0002f;

	private const float EndJobMaxPlay = 0.99f;

	private const float MaxBabyDecorationDistance = 9.9f;

	public static bool PlayTickCheckEnd(Pawn baby, Pawn adult, float playFactor, int delta, Thing playSource = null)
	{
		if (baby.needs.play == null)
		{
			return true;
		}
		float num = 0.0002f * (float)delta;
		if (playSource != null)
		{
			num *= playSource.GetStatValue(StatDefOf.BabyPlayGainFactor);
		}
		num *= playFactor;
		baby.needs.play.Play(num);
		float num2 = 0.000144f * (float)delta;
		if (playSource != null)
		{
			num2 *= playSource.GetStatValue(StatDefOf.JoyGainFactor);
		}
		adult.needs.joy?.GainJoy(num2, JoyKindDefOf.Social);
		return baby.needs.play.CurLevel >= 0.99f;
	}

	public static float GetRoomPlayGainFactors(Pawn baby)
	{
		float num = 1f;
		Room room = baby.PositionHeld.GetRoom(baby.MapHeld);
		if (room == null || room.IsHuge)
		{
			return num;
		}
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i].def == ThingDefOf.BabyDecoration && containedAndAdjacentThings[i].Position.DistanceTo(baby.Position) < 9.9f)
			{
				num *= containedAndAdjacentThings[i].GetStatValue(StatDefOf.BabyPlayGainFactor);
				break;
			}
		}
		return num;
	}
}
