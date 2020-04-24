using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Building_NutrientPasteDispenser : Building
	{
		public CompPowerTrader powerComp;

		private List<IntVec3> cachedAdjCellsCardinal;

		public static int CollectDuration = 50;

		public bool CanDispenseNow
		{
			get
			{
				if (powerComp.PowerOn)
				{
					return HasEnoughFeedstockInHoppers();
				}
				return false;
			}
		}

		public List<IntVec3> AdjCellsCardinalInBounds
		{
			get
			{
				if (cachedAdjCellsCardinal == null)
				{
					cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
						where c.InBounds(base.Map)
						select c).ToList();
				}
				return cachedAdjCellsCardinal;
			}
		}

		public virtual ThingDef DispensableDef => ThingDefOf.MealNutrientPaste;

		public override Color DrawColor
		{
			get
			{
				if (!this.IsSociallyProper(null, forPrisoner: false))
				{
					return Building_Bed.SheetColorForPrisoner;
				}
				return base.DrawColor;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
		}

		public virtual Building AdjacentReachableHopper(Pawn reacher)
		{
			for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
			{
				Building edifice = AdjCellsCardinalInBounds[i].GetEdifice(base.Map);
				if (edifice != null && edifice.def == ThingDefOf.Hopper && reacher.CanReach(edifice, PathEndMode.Touch, Danger.Deadly))
				{
					return edifice;
				}
			}
			return null;
		}

		public virtual Thing TryDispenseFood()
		{
			if (!CanDispenseNow)
			{
				return null;
			}
			float num = def.building.nutritionCostPerDispense - 0.0001f;
			List<ThingDef> list = new List<ThingDef>();
			do
			{
				Thing thing = FindFeedInAnyHopper();
				if (thing == null)
				{
					Log.Error("Did not find enough food in hoppers while trying to dispense.");
					return null;
				}
				int num2 = Mathf.Min(thing.stackCount, Mathf.CeilToInt(num / thing.GetStatValue(StatDefOf.Nutrition)));
				num -= (float)num2 * thing.GetStatValue(StatDefOf.Nutrition);
				list.Add(thing.def);
				thing.SplitOff(num2);
			}
			while (!(num <= 0f));
			def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map));
			Thing thing2 = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste);
			CompIngredients compIngredients = thing2.TryGetComp<CompIngredients>();
			for (int i = 0; i < list.Count; i++)
			{
				compIngredients.RegisterIngredient(list[i]);
			}
			return thing2;
		}

		public virtual Thing FindFeedInAnyHopper()
		{
			for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
			{
				Thing thing = null;
				Thing thing2 = null;
				List<Thing> thingList = AdjCellsCardinalInBounds[i].GetThingList(base.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing3 = thingList[j];
					if (IsAcceptableFeedstock(thing3.def))
					{
						thing = thing3;
					}
					if (thing3.def == ThingDefOf.Hopper)
					{
						thing2 = thing3;
					}
				}
				if (thing != null && thing2 != null)
				{
					return thing;
				}
			}
			return null;
		}

		public virtual bool HasEnoughFeedstockInHoppers()
		{
			float num = 0f;
			for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
			{
				IntVec3 c = AdjCellsCardinalInBounds[i];
				Thing thing = null;
				Thing thing2 = null;
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing3 = thingList[j];
					if (IsAcceptableFeedstock(thing3.def))
					{
						thing = thing3;
					}
					if (thing3.def == ThingDefOf.Hopper)
					{
						thing2 = thing3;
					}
				}
				if (thing != null && thing2 != null)
				{
					num += (float)thing.stackCount * thing.GetStatValue(StatDefOf.Nutrition);
				}
				if (num >= def.building.nutritionCostPerDispense)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsAcceptableFeedstock(ThingDef def)
		{
			if (def.IsNutritionGivingIngestible && def.ingestible.preferability != 0 && (def.ingestible.foodType & FoodTypeFlags.Plant) != FoodTypeFlags.Plant)
			{
				return (def.ingestible.foodType & FoodTypeFlags.Tree) != FoodTypeFlags.Tree;
			}
			return false;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetInspectString());
			if (!this.IsSociallyProper(null, forPrisoner: false))
			{
				stringBuilder.AppendLine("InPrisonCell".Translate());
			}
			return stringBuilder.ToString().Trim();
		}
	}
}
