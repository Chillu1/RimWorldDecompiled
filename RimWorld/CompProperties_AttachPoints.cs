using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_AttachPoints : CompProperties
{
	public List<AttachPoint> points;

	public CompProperties_AttachPoints()
	{
		compClass = typeof(CompAttachPoints);
	}
}
