using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_GameCondition : ScenPart
	{
		private float durationDays;

		private string durationDaysBuf;

		public override string Label => def.gameCondition.LabelCap;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref durationDays, "durationDayS", 0f);
		}

		public override string Summary(Scenario scen)
		{
			return def.gameCondition.LabelCap + ": " + def.gameCondition.description + " (" + ((int)(durationDays * 60000f)).ToStringTicksToDays() + ")";
		}

		public override void Randomize()
		{
			durationDays = Mathf.Round(def.durationRandomRange.RandomInRange);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Widgets.TextFieldNumericLabeled(listing.GetScenPartRect(this, ScenPart.RowHeight), "durationDays".Translate(), ref durationDays, ref durationDaysBuf);
		}

		public override void GenerateIntoMap(Map map)
		{
			if (!def.gameConditionTargetsWorld)
			{
				map.gameConditionManager.RegisterCondition(MakeCondition());
			}
		}

		public override void PostWorldGenerate()
		{
			if (def.gameConditionTargetsWorld)
			{
				Find.World.gameConditionManager.RegisterCondition(MakeCondition());
			}
		}

		private GameCondition MakeCondition()
		{
			return GameConditionMaker.MakeCondition(def.gameCondition, (int)(durationDays * 60000f));
		}

		public override bool CanCoexistWith(ScenPart other)
		{
			ScenPart_GameCondition scenPart_GameCondition = other as ScenPart_GameCondition;
			if (scenPart_GameCondition != null && !scenPart_GameCondition.def.gameCondition.CanCoexistWith(def.gameCondition))
			{
				return false;
			}
			return true;
		}
	}
}
