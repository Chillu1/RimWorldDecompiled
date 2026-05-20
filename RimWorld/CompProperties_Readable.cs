using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class CompProperties_Readable : CompProperties
{
	public MentalBreakIntensity mentalBreakIntensity = MentalBreakIntensity.Major;

	public DevelopmentalStage developmentalStageFilter = DevelopmentalStage.Child | DevelopmentalStage.Adult;

	public List<ReadingOutcomeProperties> doers;

	public CompProperties_Readable()
	{
		compClass = typeof(CompReadable);
	}
}
