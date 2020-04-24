using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal class ScenPart_CreateIncident : ScenPart_IncidentBase
	{
		private const float IntervalMidpoint = 30f;

		private const float IntervalDeviation = 15f;

		private float intervalDays;

		private bool repeat;

		private string intervalDaysBuffer;

		private float occurTick;

		private bool isFinished;

		protected override string IncidentTag => "CreateIncident";

		private float IntervalTicks => 60000f * intervalDays;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref intervalDays, "intervalDays", 0f);
			Scribe_Values.Look(ref repeat, "repeat", defaultValue: false);
			Scribe_Values.Look(ref occurTick, "occurTick", 0f);
			Scribe_Values.Look(ref isFinished, "isFinished", defaultValue: false);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
			Rect rect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, scenPartRect.height / 3f);
			Rect rect2 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height / 3f, scenPartRect.width, scenPartRect.height / 3f);
			Rect rect3 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height * 2f / 3f, scenPartRect.width, scenPartRect.height / 3f);
			DoIncidentEditInterface(rect);
			Widgets.TextFieldNumericLabeled(rect2, "intervalDays".Translate(), ref intervalDays, ref intervalDaysBuffer);
			Widgets.CheckboxLabeled(rect3, "repeat".Translate(), ref repeat);
		}

		public override void Randomize()
		{
			base.Randomize();
			intervalDays = 15f * Rand.Gaussian() + 30f;
			if (intervalDays < 0f)
			{
				intervalDays = 0f;
			}
			repeat = (Rand.Range(0, 100) < 50);
		}

		protected override IEnumerable<IncidentDef> RandomizableIncidents()
		{
			yield return IncidentDefOf.Eclipse;
			yield return IncidentDefOf.ToxicFallout;
			yield return IncidentDefOf.SolarFlare;
		}

		public override void PostGameStart()
		{
			base.PostGameStart();
			occurTick = (float)Find.TickManager.TicksGame + IntervalTicks;
		}

		public override void Tick()
		{
			base.Tick();
			if (Find.AnyPlayerHomeMap == null || isFinished)
			{
				return;
			}
			if (incident == null)
			{
				Log.Error("Trying to tick ScenPart_CreateIncident but the incident is null");
				isFinished = true;
			}
			else if ((float)Find.TickManager.TicksGame >= occurTick)
			{
				IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, Find.Maps.Where((Map x) => x.IsPlayerHome).RandomElement());
				if (!incident.Worker.TryExecute(parms))
				{
					isFinished = true;
				}
				else if (repeat && intervalDays > 0f)
				{
					occurTick += IntervalTicks;
				}
				else
				{
					isFinished = true;
				}
			}
		}
	}
}
