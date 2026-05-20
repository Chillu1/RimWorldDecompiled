using System;

namespace RimWorld;

public class BookOutcomeProperties_GiveQuest : BookOutcomeProperties
{
	public float questChance;

	public override Type DoerClass => typeof(BookOutcomeDoer_GiveQuest);
}
