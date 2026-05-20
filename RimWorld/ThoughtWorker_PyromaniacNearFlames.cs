using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PyromaniacNearFlames : ThoughtWorker
	{
		private const float MaxDist = 8f;

		public override string PostProcessLabel(Pawn p, string label)
		{
			int num = NumFiresNear(p);
			if (num == 1)
			{
				return base.PostProcessLabel(p, label);
			}
			return base.PostProcessLabel(p, label) + " x" + num;
		}

		public override float MoodMultiplier(Pawn p)
		{
			return NumFiresNear(p);
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return NumFiresNear(p) > 0;
		}

		private int NumFiresNear(Pawn p)
		{
			int num = 0;
			Room room = p.Position.GetRoom(p.Map);
			foreach (IntVec3 item in GenRadial.RadialCellsAround(p.Position, 8f, useCenter: true))
			{
				if (!item.InBounds(p.Map) || item.Fogged(p.Map))
				{
					continue;
				}
				Room room2 = item.GetRoom(p.Map);
				if (room2 != null && room == room2)
				{
					num += FireUtility.NumFiresAt(item, p.Map);
					if (num >= def.stackLimit)
					{
						break;
					}
				}
			}
			return Mathf.Min(num, def.stackLimit);
		}
	}
}
