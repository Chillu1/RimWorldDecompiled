using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart_ThingCount : ScenPart
	{
		protected ThingDef thingDef;

		protected ThingDef stuff;

		protected int count = 1;

		private string countBuf;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Defs.Look(ref stuff, "stuff");
			Scribe_Values.Look(ref count, "count", 1);
		}

		public override void Randomize()
		{
			thingDef = PossibleThingDefs().RandomElement();
			stuff = GenStuff.RandomStuffFor(thingDef);
			if (thingDef.statBases.StatListContains(StatDefOf.MarketValue))
			{
				float num = Rand.Range(200, 2000);
				float statValueAbstract = thingDef.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
				count = Mathf.CeilToInt(num / statValueAbstract);
			}
			else
			{
				count = Rand.RangeInclusive(1, 100);
			}
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
			Rect rect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, scenPartRect.height / 3f);
			Rect rect2 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height / 3f, scenPartRect.width, scenPartRect.height / 3f);
			Rect rect3 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height * 2f / 3f, scenPartRect.width, scenPartRect.height / 3f);
			if (Widgets.ButtonText(rect, thingDef.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef item in from t in PossibleThingDefs()
					orderby t.label
					select t)
				{
					ThingDef localTd = item;
					list.Add(new FloatMenuOption(localTd.LabelCap, delegate
					{
						thingDef = localTd;
						stuff = GenStuff.DefaultStuffFor(localTd);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			if (thingDef.MadeFromStuff && Widgets.ButtonText(rect2, stuff.LabelCap))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (ThingDef item2 in from t in GenStuff.AllowedStuffsFor(thingDef)
					orderby t.label
					select t)
				{
					ThingDef localSd = item2;
					list2.Add(new FloatMenuOption(localSd.LabelCap, delegate
					{
						stuff = localSd;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			Widgets.TextFieldNumeric(rect3, ref count, ref countBuf, 1f);
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_ThingCount scenPart_ThingCount = other as ScenPart_ThingCount;
			if (scenPart_ThingCount != null && GetType() == scenPart_ThingCount.GetType() && thingDef == scenPart_ThingCount.thingDef && stuff == scenPart_ThingCount.stuff && count >= 0 && scenPart_ThingCount.count >= 0)
			{
				count += scenPart_ThingCount.count;
				return true;
			}
			return false;
		}

		protected virtual IEnumerable<ThingDef> PossibleThingDefs()
		{
			return DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => (d.category == ThingCategory.Item && d.scatterableOnMapGen && !d.destroyOnDrop) || (d.category == ThingCategory.Building && d.Minifiable) || (d.category == ThingCategory.Building && d.scatterableOnMapGen));
		}

		public override bool HasNullDefs()
		{
			if (!base.HasNullDefs() && thingDef != null)
			{
				if (thingDef.MadeFromStuff)
				{
					return stuff == null;
				}
				return false;
			}
			return true;
		}
	}
}
