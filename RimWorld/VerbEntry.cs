using System.Collections.Generic;
using Verse;

namespace RimWorld;

public struct VerbEntry
{
	public Verb verb;

	private float cachedSelectionWeight;

	public bool IsMeleeAttack => verb.IsMeleeAttack;

	public VerbEntry(Verb verb, Pawn pawn, List<Verb> allVerbs, float highestSelWeight)
	{
		this.verb = verb;
		cachedSelectionWeight = VerbUtility.FinalSelectionWeight(verb, pawn, allVerbs, highestSelWeight);
	}

	public float GetSelectionWeight(Thing target)
	{
		if (!verb.IsUsableOn(target))
		{
			return 0f;
		}
		float num = cachedSelectionWeight;
		if (target?.def?.IsEdifice() == true)
		{
			num *= verb.verbProps.commonalityVsEdificeFactor;
		}
		return num;
	}

	public override string ToString()
	{
		return verb?.ToString() + " - " + cachedSelectionWeight;
	}
}
