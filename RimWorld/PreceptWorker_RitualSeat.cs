using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PreceptWorker_RitualSeat : PreceptWorker
	{
		private static List<ThingDef> tmpRitualSeats = new List<ThingDef>();

		private static List<ThingDef> tmpRitualSeatsMerge = new List<ThingDef>();

		public override AcceptanceReport CanUse(ThingDef def, Ideo ideo, FactionDef generatingFor)
		{
			tmpRitualSeats.Clear();
			List<ThingDef> list = null;
			foreach (MemeDef meme in ideo.memes)
			{
				if (meme.requireAnyRitualSeat.NullOrEmpty())
				{
					continue;
				}
				if (list == null)
				{
					tmpRitualSeats.AddRange(meme.requireAnyRitualSeat);
					list = tmpRitualSeats;
					continue;
				}
				tmpRitualSeatsMerge.Clear();
				foreach (ThingDef item in meme.requireAnyRitualSeat)
				{
					if (list.Contains(item))
					{
						tmpRitualSeatsMerge.Add(item);
					}
				}
				list.Clear();
				list.AddRange(tmpRitualSeatsMerge);
				if (list.Count == 0)
				{
					Log.Error("Ideo has 2 memes which have conflicting ritual set requirements!");
				}
			}
			return list?.Contains(def) ?? true;
		}
	}
}
