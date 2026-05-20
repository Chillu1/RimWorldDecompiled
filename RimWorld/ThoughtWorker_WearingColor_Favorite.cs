using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtWorker_WearingColor_Favorite : ThoughtWorker_WearingColor
{
	protected override Color? Color(Pawn p)
	{
		if (p.DevelopmentalStage.Baby())
		{
			return null;
		}
		return p.story.favoriteColor?.color;
	}
}
