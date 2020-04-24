using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_SetNeedLevel : ScenPart_PawnModifier
	{
		private NeedDef need;

		private FloatRange levelRange;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f + 31f);
			if (Widgets.ButtonText(scenPartRect.TopPartPixels(ScenPart.RowHeight), need.LabelCap))
			{
				FloatMenuUtility.MakeMenu(PossibleNeeds(), (NeedDef hd) => hd.LabelCap, delegate(NeedDef n)
				{
					ScenPart_SetNeedLevel scenPart_SetNeedLevel = this;
					return delegate
					{
						scenPart_SetNeedLevel.need = n;
					};
				});
			}
			Widgets.FloatRange(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f), listing.CurHeight.GetHashCode(), ref levelRange, 0f, 1f, "ConfigurableLevel");
			DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(ScenPart.RowHeight * 2f));
		}

		private IEnumerable<NeedDef> PossibleNeeds()
		{
			return DefDatabase<NeedDef>.AllDefsListForReading.Where((NeedDef x) => x.major);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref need, "need");
			Scribe_Values.Look(ref levelRange, "levelRange");
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_SetNeed".Translate(context.ToStringHuman(), chance.ToStringPercent(), need.label, levelRange.min.ToStringPercent(), levelRange.max.ToStringPercent()).CapitalizeFirst();
		}

		public override void Randomize()
		{
			base.Randomize();
			need = PossibleNeeds().RandomElement();
			levelRange.max = Rand.Range(0f, 1f);
			levelRange.min = levelRange.max * Rand.Range(0f, 0.95f);
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_SetNeedLevel scenPart_SetNeedLevel = other as ScenPart_SetNeedLevel;
			if (scenPart_SetNeedLevel != null && need == scenPart_SetNeedLevel.need)
			{
				chance = GenMath.ChanceEitherHappens(chance, scenPart_SetNeedLevel.chance);
				return true;
			}
			return false;
		}

		protected override void ModifyPawnPostGenerate(Pawn p, bool redressed)
		{
			if (p.needs != null)
			{
				Need need = p.needs.TryGetNeed(this.need);
				if (need != null)
				{
					need.CurLevelPercentage = levelRange.RandomInRange;
				}
			}
		}
	}
}
