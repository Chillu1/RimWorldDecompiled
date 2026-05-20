using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_WearingColor_Ideo : ThoughtWorker_WearingColor
	{
		protected override Color? Color(Pawn p)
		{
			return p.Ideo?.ApparelColor;
		}
	}
}
