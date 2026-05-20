using System;

namespace RimWorld;

public class BookOutcomeProperties_JoyFactorModifier : BookOutcomeProperties
{
	public override Type DoerClass => typeof(ReadingOutcomeDoerJoyFactorModifier);
}
