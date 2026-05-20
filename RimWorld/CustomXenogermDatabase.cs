using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class CustomXenogermDatabase : IExposable
	{
		private List<CustomXenogerm> customXenogerms = new List<CustomXenogerm>();

		public List<CustomXenogerm> CustomXenogermsForReading => customXenogerms;

		public void Add(CustomXenogerm customXenogerm)
		{
			if (ModLister.CheckBiotech("Custom xenogerm database"))
			{
				customXenogerms.RemoveAll((CustomXenogerm c) => c.name == customXenogerm.name);
				customXenogerms.Add(customXenogerm);
			}
		}

		public bool Remove(CustomXenogerm customXenogerm)
		{
			return customXenogerms.Remove(customXenogerm);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref customXenogerms, "customXenogerms", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && customXenogerms == null)
			{
				customXenogerms = new List<CustomXenogerm>();
			}
		}
	}
}
