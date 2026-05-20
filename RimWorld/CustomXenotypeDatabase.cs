using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class CustomXenotypeDatabase : IExposable
{
	public List<CustomXenotype> customXenotypes = new List<CustomXenotype>();

	public void ExposeData()
	{
		Scribe_Collections.Look(ref customXenotypes, "customXenotypes", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && customXenotypes == null)
		{
			customXenotypes = new List<CustomXenotype>();
		}
	}
}
