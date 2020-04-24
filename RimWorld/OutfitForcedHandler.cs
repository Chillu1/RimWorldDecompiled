using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class OutfitForcedHandler : IExposable
	{
		private List<Apparel> forcedAps = new List<Apparel>();

		public bool SomethingIsForced => forcedAps.Count > 0;

		public List<Apparel> ForcedApparel => forcedAps;

		public void Reset()
		{
			forcedAps.Clear();
		}

		public bool AllowedToAutomaticallyDrop(Apparel ap)
		{
			return !forcedAps.Contains(ap);
		}

		public void SetForced(Apparel ap, bool forced)
		{
			if (forced)
			{
				if (!forcedAps.Contains(ap))
				{
					forcedAps.Add(ap);
				}
			}
			else if (forcedAps.Contains(ap))
			{
				forcedAps.Remove(ap);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref forcedAps, "forcedAps", LookMode.Reference);
		}

		public bool IsForced(Apparel ap)
		{
			if (ap.Destroyed)
			{
				Log.Error("Apparel was forced while Destroyed: " + ap);
				if (forcedAps.Contains(ap))
				{
					forcedAps.Remove(ap);
				}
				return false;
			}
			return forcedAps.Contains(ap);
		}
	}
}
