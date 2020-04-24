using System.Collections.Generic;

namespace Verse
{
	public static class CompressibilityDeciderUtility
	{
		public static bool IsSaveCompressible(this Thing t)
		{
			if (Scribe.saver.savingForDebug)
			{
				return false;
			}
			if (!t.def.saveCompressible)
			{
				return false;
			}
			if (t.def.useHitPoints && t.HitPoints != t.MaxHitPoints)
			{
				return false;
			}
			if (!t.Spawned)
			{
				return false;
			}
			bool flag = false;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].compressor != null)
				{
					flag = true;
					if (maps[i].compressor.compressibilityDecider.IsReferenced(t))
					{
						return false;
					}
				}
			}
			if (!flag)
			{
				Log.ErrorOnce("Called IsSaveCompressible but there are no maps with compressor != null. This should never happen. It probably means that we're not saving any map at the moment?", 1935111328);
			}
			return true;
		}
	}
}
