using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MyChildEnslaved : ThoughtWorker
	{
		public override float MoodMultiplier(Pawn p)
		{
			return Mathf.Min(def.stackLimit, ChildrenEnslavedCount(p));
		}

		public override string PostProcessLabel(Pawn p, string label)
		{
			int num = Mathf.RoundToInt(MoodMultiplier(p));
			if (num <= 1)
			{
				return base.PostProcessLabel(p, label);
			}
			return base.PostProcessLabel(p, label) + " x" + num;
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return ThoughtState.Inactive;
			}
			if (p.Suspended)
			{
				return ThoughtState.Inactive;
			}
			if (ChildrenEnslavedCount(p) <= 0)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveDefault;
		}

		private static int ChildrenEnslavedCount(Pawn parent)
		{
			return parent.relations.Children.Count((Pawn child) => child.DevelopmentalStage.Juvenile() && !child.Dead && child.IsSlave);
		}
	}
}
