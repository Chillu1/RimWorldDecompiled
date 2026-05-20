using System;
using Verse;

namespace RimWorld;

public abstract class ReadingOutcomeProperties
{
	[TranslationHandle]
	public Type doerClass = typeof(ReadingOutcomeDoer);

	public abstract Type DoerClass { get; }

	public ReadingOutcomeProperties()
	{
	}

	public ReadingOutcomeProperties(Type doerClass)
	{
		this.doerClass = doerClass;
	}
}
