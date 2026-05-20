using System;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class MoodThresholdExtensions
{
	private const float Alpha_Off = 0f;

	private const float Alpha_Subtle = 0.9f;

	private const float Alpha_Intense = 1f;

	public static Color GetColor(this MoodThreshold m)
	{
		Color c = Color.clear;
		float transparency = 0f;
		switch (m)
		{
		case MoodThreshold.Minor:
			c = ColorLibrary.Gold;
			transparency = 0.9f;
			break;
		case MoodThreshold.Major:
			c = ColorLibrary.Orange;
			transparency = 0.9f;
			break;
		case MoodThreshold.Extreme:
			c = ColorLibrary.RedReadable;
			transparency = 1f * Pulser.PulseBrightness(0.9f, 0.6f) + 0.1f;
			break;
		}
		return c.ToTransparent(transparency);
	}

	public static float EdgeExpansion(this MoodThreshold m)
	{
		return m switch
		{
			MoodThreshold.None => 0f, 
			MoodThreshold.Minor => 0f, 
			MoodThreshold.Major => 5f, 
			MoodThreshold.Extreme => 6f, 
			_ => throw new NotImplementedException(), 
		};
	}

	public static MoodThreshold CurrentMoodThresholdFor(Pawn pawn)
	{
		if (pawn.mindState == null)
		{
			return MoodThreshold.None;
		}
		if (pawn.mindState.mentalBreaker.CanDoRandomMentalBreaks)
		{
			if (pawn.mindState.mentalBreaker.BreakExtremeIsImminent)
			{
				return MoodThreshold.Extreme;
			}
			if (pawn.mindState.mentalBreaker.BreakMajorIsImminent)
			{
				return MoodThreshold.Major;
			}
			if (pawn.mindState.mentalBreaker.BreakMinorIsImminent)
			{
				return MoodThreshold.Minor;
			}
		}
		return MoodThreshold.None;
	}
}
