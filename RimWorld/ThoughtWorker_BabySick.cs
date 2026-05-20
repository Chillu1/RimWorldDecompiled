using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_BabySick : ThoughtWorker
	{
		private const int SingleBabySickStage = 0;

		private const int SeveralBabiesSickStage = 1;

		public override string PostProcessLabel(Pawn p, string label)
		{
			IEnumerable<Pawn> source = p.relations.Children.Where(IsSickBaby);
			if (source.Count() > 1)
			{
				return base.PostProcessLabel(p, label) + " x" + Mathf.RoundToInt(MoodMultiplier(p));
			}
			Pawn arg = source.First();
			return label.Formatted(p.Named("PAWN"), arg.Named("BABY"));
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return ThoughtState.Inactive;
			}
			int num = BabiesSickCount(p);
			if (num > 1)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (num == 1)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			return ThoughtState.Inactive;
		}

		private int BabiesSickCount(Pawn pawn)
		{
			int num = 0;
			foreach (Pawn child in pawn.relations.Children)
			{
				if (!child.Dead && IsSickBaby(child))
				{
					num++;
				}
			}
			return num;
		}

		private bool IsSickBaby(Pawn pawn)
		{
			if (pawn.DevelopmentalStage.Baby())
			{
				return pawn.health.hediffSet.HasHediff(HediffDefOf.InfantIllness);
			}
			return false;
		}

		public override float MoodMultiplier(Pawn p)
		{
			return Mathf.Min(def.stackLimit, BabiesSickCount(p));
		}
	}
}
