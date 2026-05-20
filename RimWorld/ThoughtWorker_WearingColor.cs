using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ThoughtWorker_WearingColor : ThoughtWorker
	{
		public const float RequiredMinPercentage = 0.6f;

		protected abstract Color? Color(Pawn p);

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			Color? color = Color(p);
			if (!color.HasValue)
			{
				return false;
			}
			int num = 0;
			foreach (Apparel item in p.apparel.WornApparel)
			{
				CompColorable compColorable = item.TryGetComp<CompColorable>();
				if (compColorable != null && compColorable.Active && compColorable.Color.IndistinguishableFrom(color.Value))
				{
					num++;
				}
			}
			return (float)num / (float)p.apparel.WornApparelCount >= 0.6f;
		}
	}
}
