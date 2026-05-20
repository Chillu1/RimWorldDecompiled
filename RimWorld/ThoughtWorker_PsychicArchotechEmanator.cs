using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class ThoughtWorker_PsychicArchotechEmanator : ThoughtWorker
	{
		protected abstract ThingDef EmanatorDef { get; }

		protected abstract int InnerRadius { get; }

		protected abstract int OuterRadius { get; }

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.Spawned)
			{
				return false;
			}
			List<Thing> list = p.Map.listerThings.ThingsOfDef(EmanatorDef);
			for (int i = 0; i < list.Count; i++)
			{
				if (p.Position.InHorDistOf(list[i].Position, InnerRadius))
				{
					return ThoughtState.ActiveAtStage(0);
				}
				if (p.Position.InHorDistOf(list[i].Position, OuterRadius))
				{
					return ThoughtState.ActiveAtStage(1);
				}
			}
			return false;
		}
	}
}
