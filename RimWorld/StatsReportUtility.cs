using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class StatsReportUtility
	{
		private static StatDrawEntry selectedEntry;

		private static StatDrawEntry mousedOverEntry;

		private static Vector2 scrollPosition;

		private static Vector2 scrollPositionRightPanel;

		private static float listHeight;

		private static float rightPanelHeight;

		private static List<StatDrawEntry> cachedDrawEntries = new List<StatDrawEntry>();

		public static int SelectedStatIndex
		{
			get
			{
				if (cachedDrawEntries.NullOrEmpty() || selectedEntry == null)
				{
					return -1;
				}
				return cachedDrawEntries.IndexOf(selectedEntry);
			}
		}

		public static void Reset()
		{
			scrollPosition = default(Vector2);
			scrollPositionRightPanel = default(Vector2);
			selectedEntry = null;
			mousedOverEntry = null;
			cachedDrawEntries.Clear();
		}

		public static void DrawStatsReport(Rect rect, Def def, ThingDef stuff)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				BuildableDef buildableDef = def as BuildableDef;
				StatRequest req = (buildableDef != null) ? StatRequest.For(buildableDef, stuff) : StatRequest.ForEmpty();
				cachedDrawEntries.AddRange(def.SpecialDisplayStats(req));
				cachedDrawEntries.AddRange(from r in StatsToDraw(def, stuff)
					where r.ShouldDisplay
					select r);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, null, null);
		}

		public static void DrawStatsReport(Rect rect, AbilityDef def)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				StatRequest req = StatRequest.ForEmpty();
				cachedDrawEntries.AddRange(def.SpecialDisplayStats(req));
				cachedDrawEntries.AddRange(from r in StatsToDraw(def)
					where r.ShouldDisplay
					select r);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, null, null);
		}

		public static void DrawStatsReport(Rect rect, Thing thing)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				cachedDrawEntries.AddRange(thing.def.SpecialDisplayStats(StatRequest.For(thing)));
				cachedDrawEntries.AddRange(from r in StatsToDraw(thing)
					where r.ShouldDisplay
					select r);
				cachedDrawEntries.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, thing, null);
		}

		public static void DrawStatsReport(Rect rect, WorldObject worldObject)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				cachedDrawEntries.AddRange(worldObject.def.SpecialDisplayStats(StatRequest.ForEmpty()));
				cachedDrawEntries.AddRange(from r in StatsToDraw(worldObject)
					where r.ShouldDisplay
					select r);
				cachedDrawEntries.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, null, worldObject);
		}

		public static void DrawStatsReport(Rect rect, RoyalTitleDef title, Faction faction)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				cachedDrawEntries.AddRange(title.SpecialDisplayStats(StatRequest.For(title, faction)));
				cachedDrawEntries.AddRange(from r in StatsToDraw(title, faction)
					where r.ShouldDisplay
					select r);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, null, null);
		}

		public static void DrawStatsReport(Rect rect, Faction faction)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				StatRequest req = StatRequest.ForEmpty();
				cachedDrawEntries.AddRange(faction.def.SpecialDisplayStats(req));
				cachedDrawEntries.AddRange(from r in StatsToDraw(faction)
					where r.ShouldDisplay
					select r);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, null, null);
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(Def def, ThingDef stuff)
		{
			yield return DescriptionEntry(def);
			BuildableDef eDef = def as BuildableDef;
			if (eDef != null)
			{
				StatRequest statRequest = StatRequest.For(eDef, stuff);
				foreach (StatDef item in DefDatabase<StatDef>.AllDefs.Where((StatDef st) => st.Worker.ShouldShowFor(statRequest)))
				{
					yield return new StatDrawEntry(item.category, item, eDef.GetStatValueAbstract(item, stuff), StatRequest.For(eDef, stuff));
				}
			}
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(RoyalTitleDef title, Faction faction)
		{
			yield return DescriptionEntry(title, faction);
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(Faction faction)
		{
			yield return DescriptionEntry(faction);
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(AbilityDef def)
		{
			yield return DescriptionEntry(def);
			StatRequest statRequest = StatRequest.For(def);
			foreach (StatDef item in DefDatabase<StatDef>.AllDefs.Where((StatDef st) => st.Worker.ShouldShowFor(statRequest)))
			{
				yield return new StatDrawEntry(item.category, item, def.GetStatValueAbstract(item), StatRequest.For(def));
			}
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(Thing thing)
		{
			yield return DescriptionEntry(thing);
			StatDrawEntry statDrawEntry = QualityEntry(thing);
			if (statDrawEntry != null)
			{
				yield return statDrawEntry;
			}
			foreach (StatDef item in DefDatabase<StatDef>.AllDefs.Where((StatDef st) => st.Worker.ShouldShowFor(StatRequest.For(thing))))
			{
				if (!item.Worker.IsDisabledFor(thing))
				{
					float statValue = thing.GetStatValue(item);
					if (item.showOnDefaultValue || statValue != item.defaultBaseValue)
					{
						yield return new StatDrawEntry(item.category, item, statValue, StatRequest.For(thing));
					}
				}
				else
				{
					yield return new StatDrawEntry(item.category, item);
				}
			}
			if (thing.def.useHitPoints)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "HitPointsBasic".Translate().CapitalizeFirst(), thing.HitPoints.ToString() + " / " + thing.MaxHitPoints.ToString(), "Stat_HitPoints_Desc".Translate(), 99998);
			}
			foreach (StatDrawEntry item2 in thing.SpecialDisplayStats())
			{
				yield return item2;
			}
			if (!thing.def.IsStuff)
			{
				yield break;
			}
			if (!thing.def.stuffProps.statFactors.NullOrEmpty())
			{
				for (int j = 0; j < thing.def.stuffProps.statFactors.Count; j++)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.StuffStatFactors, thing.def.stuffProps.statFactors[j].stat, thing.def.stuffProps.statFactors[j].value, StatRequest.ForEmpty(), ToStringNumberSense.Factor);
				}
			}
			if (!thing.def.stuffProps.statOffsets.NullOrEmpty())
			{
				for (int j = 0; j < thing.def.stuffProps.statOffsets.Count; j++)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.StuffStatOffsets, thing.def.stuffProps.statOffsets[j].stat, thing.def.stuffProps.statOffsets[j].value, StatRequest.ForEmpty(), ToStringNumberSense.Offset);
				}
			}
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(WorldObject worldObject)
		{
			yield return DescriptionEntry(worldObject);
			foreach (StatDrawEntry specialDisplayStat in worldObject.SpecialDisplayStats)
			{
				yield return specialDisplayStat;
			}
		}

		private static void FinalizeCachedDrawEntries(IEnumerable<StatDrawEntry> original)
		{
			cachedDrawEntries = (from sd in original
				orderby sd.category.displayOrder, sd.DisplayPriorityWithinCategory descending, sd.LabelCap
				select sd).ToList();
		}

		private static StatDrawEntry DescriptionEntry(Def def)
		{
			return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Description".Translate(), "", def.description, 99999, null, Dialog_InfoCard.DefsToHyperlinks(def.descriptionHyperlinks));
		}

		private static StatDrawEntry DescriptionEntry(Faction faction)
		{
			return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Description".Translate(), "", faction.GetReportText, 99999, null, Dialog_InfoCard.DefsToHyperlinks(faction.def.descriptionHyperlinks));
		}

		private static StatDrawEntry DescriptionEntry(RoyalTitleDef title, Faction faction)
		{
			return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Description".Translate(), "", title.GetReportText(faction), 99999, null, Dialog_InfoCard.TitleDefsToHyperlinks(title.GetHyperlinks(faction)));
		}

		private static StatDrawEntry DescriptionEntry(Thing thing)
		{
			return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Description".Translate(), "", thing.DescriptionFlavor, 99999, null, Dialog_InfoCard.DefsToHyperlinks(thing.def.descriptionHyperlinks));
		}

		private static StatDrawEntry DescriptionEntry(WorldObject worldObject)
		{
			return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Description".Translate(), "", worldObject.GetDescription(), 99999);
		}

		private static StatDrawEntry QualityEntry(Thing t)
		{
			if (!t.TryGetQuality(out QualityCategory qc))
			{
				return null;
			}
			return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Quality".Translate(), qc.GetLabel().CapitalizeFirst(), "QualityDescription".Translate(), 99999);
		}

		public static void SelectEntry(int index)
		{
			if (index >= 0 && index <= cachedDrawEntries.Count)
			{
				SelectEntry(cachedDrawEntries[index]);
			}
		}

		public static void SelectEntry(StatDef stat, bool playSound = false)
		{
			foreach (StatDrawEntry cachedDrawEntry in cachedDrawEntries)
			{
				if (cachedDrawEntry.stat == stat)
				{
					SelectEntry(cachedDrawEntry, playSound);
					return;
				}
			}
			Messages.Message("MessageCannotSelectInvisibleStat".Translate(stat), MessageTypeDefOf.RejectInput, historical: false);
		}

		private static void SelectEntry(StatDrawEntry rec, bool playSound = true)
		{
			selectedEntry = rec;
			if (playSound)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
		}

		private static void DrawStatsWorker(Rect rect, Thing optionalThing, WorldObject optionalWorldObject)
		{
			Rect rect2 = new Rect(rect);
			rect2.width *= 0.5f;
			Rect rect3 = new Rect(rect);
			rect3.x = rect2.xMax;
			rect3.width = rect.xMax - rect3.x;
			Text.Font = GameFont.Small;
			Rect viewRect = new Rect(0f, 0f, rect2.width - 16f, listHeight);
			Widgets.BeginScrollView(rect2, ref scrollPosition, viewRect);
			float curY = 0f;
			string b = null;
			mousedOverEntry = null;
			for (int i = 0; i < cachedDrawEntries.Count; i++)
			{
				StatDrawEntry ent = cachedDrawEntries[i];
				if ((string)ent.category.LabelCap != b)
				{
					Widgets.ListSeparator(ref curY, viewRect.width, ent.category.LabelCap);
					b = ent.category.LabelCap;
				}
				curY += ent.Draw(8f, curY, viewRect.width - 8f, selectedEntry == ent, delegate
				{
					SelectEntry(ent);
				}, delegate
				{
					mousedOverEntry = ent;
				}, scrollPosition, rect2);
			}
			listHeight = curY + 100f;
			Widgets.EndScrollView();
			Rect outRect = rect3.ContractedBy(10f);
			StatDrawEntry statDrawEntry = selectedEntry ?? mousedOverEntry ?? cachedDrawEntries.FirstOrDefault();
			if (statDrawEntry != null)
			{
				Rect rect4 = new Rect(0f, 0f, outRect.width - 16f, rightPanelHeight);
				StatRequest statRequest = statDrawEntry.hasOptionalReq ? statDrawEntry.optionalReq : ((optionalThing == null) ? StatRequest.ForEmpty() : StatRequest.For(optionalThing));
				string explanationText = statDrawEntry.GetExplanationText(statRequest);
				float num = 0f;
				Widgets.BeginScrollView(outRect, ref scrollPositionRightPanel, rect4);
				Rect rect5 = rect4;
				rect5.width -= 4f;
				Widgets.Label(rect5, explanationText);
				float num2 = Text.CalcHeight(explanationText, rect5.width) + 10f;
				num += num2;
				IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks = statDrawEntry.GetHyperlinks(statRequest);
				if (hyperlinks != null)
				{
					Rect rect6 = new Rect(rect5.x, rect5.y + num2, rect5.width, rect5.height - num2);
					Color color = GUI.color;
					GUI.color = Widgets.NormalOptionColor;
					foreach (Dialog_InfoCard.Hyperlink item in hyperlinks)
					{
						float num3 = Text.CalcHeight(item.Label, rect6.width);
						Widgets.HyperlinkWithIcon(new Rect(rect6.x, rect6.y, rect6.width, num3), item, "ViewHyperlink".Translate(item.Label));
						rect6.y += num3;
						rect6.height -= num3;
						num += num3;
					}
					GUI.color = color;
				}
				rightPanelHeight = num;
				Widgets.EndScrollView();
			}
		}
	}
}
