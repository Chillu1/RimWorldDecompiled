using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_ColonistMood : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			List<Pawn> allMaps_FreeColonists = PawnsFinder.AllMaps_FreeColonists;
			if (!allMaps_FreeColonists.Any())
			{
				return 0f;
			}
			return allMaps_FreeColonists.Where((Pawn x) => x.needs.mood != null).Average((Pawn x) => x.needs.mood.CurLevel * 100f);
		}
	}
}
