using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class TerrorUtility
{
	private const int MaxTerror = 100;

	private const int TerrorThoughtsToTake = 3;

	public static SimpleCurve SuppressionFallRateOverTerror = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(25f, -15f),
		new CurvePoint(50f, -25f),
		new CurvePoint(100f, -45f)
	};

	public static IEnumerable<Thought_MemoryObservationTerror> GetTerrorThoughts(Pawn pawn)
	{
		List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
		for (int i = 0; i < memories.Count; i++)
		{
			if (memories[i] is Thought_MemoryObservationTerror thought_MemoryObservationTerror)
			{
				yield return thought_MemoryObservationTerror;
			}
		}
	}

	public static IEnumerable<Thought_MemoryObservationTerror> TakeTopTerrorThoughts(IEnumerable<Thought_MemoryObservationTerror> thoughts)
	{
		return thoughts.OrderByDescending((Thought_MemoryObservationTerror t) => t.intensity).Take(3);
	}

	public static float GetTerrorLevel(this Pawn pawn)
	{
		int num = 0;
		if (!pawn.IsSlave)
		{
			return num;
		}
		foreach (Thought_MemoryObservationTerror terrorThought in GetTerrorThoughts(pawn))
		{
			num += terrorThought.intensity;
		}
		return (float)Mathf.Min(num, 100) / 100f;
	}

	public static void RemoveAllTerrorThoughts(Pawn pawn)
	{
		foreach (Thought_MemoryObservationTerror terrorThought in GetTerrorThoughts(pawn))
		{
			pawn.needs.mood.thoughts.memories.RemoveMemory(terrorThought);
		}
	}

	public static bool TryCreateTerrorThought(Thing thing, out Thought_MemoryObservationTerror thought)
	{
		thought = null;
		if (!ModsConfig.IdeologyActive)
		{
			return false;
		}
		float statValue = thing.GetStatValue(StatDefOf.TerrorSource);
		if (statValue <= 0f)
		{
			return false;
		}
		thought = (Thought_MemoryObservationTerror)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedTerror);
		thought.Target = thing;
		thought.intensity = (int)statValue;
		return true;
	}
}
