using System;

namespace RimWorld;

public abstract class BookOutcomeProperties : ReadingOutcomeProperties
{
	public BookOutcomeProperties()
	{
	}

	public BookOutcomeProperties(Type doerClass)
	{
		base.doerClass = doerClass;
	}
}
