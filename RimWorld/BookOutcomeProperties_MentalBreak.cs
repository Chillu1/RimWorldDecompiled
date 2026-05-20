using System;
using Verse;

namespace RimWorld;

public class BookOutcomeProperties_MentalBreak : BookOutcomeProperties
{
	public FloatRange chancePerHourRange = new FloatRange(0f, 0.2f);

	public FloatRange chanceMinMaxRange = new FloatRange(0.01f, 0.9f);

	public override Type DoerClass => typeof(ReadingOutcomeDoerMentalBreak);
}
