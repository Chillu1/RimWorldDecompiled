using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SitePartDef : Def
	{
		public ThingDef conditionCauserDef;

		public float activeThreatDisturbanceFactor = 1f;

		public bool defaultHidden;

		public Type workerClass = typeof(SitePartWorker);

		[NoTranslate]
		public string siteTexture;

		[NoTranslate]
		public string expandingIconTexture;

		public bool applyFactionColorToSiteTexture;

		public bool showFactionInInspectString;

		public bool requiresFaction;

		public TechLevel minFactionTechLevel;

		[MustTranslate]
		public string approachOrderString;

		[MustTranslate]
		public string approachingReportString;

		[NoTranslate]
		public List<string> tags = new List<string>();

		[NoTranslate]
		public List<string> excludesTags = new List<string>();

		[MustTranslate]
		public string arrivedLetter;

		[MustTranslate]
		public string arrivedLetterLabelPart;

		public List<HediffDef> arrivedLetterHediffHyperlinks;

		public LetterDef arrivedLetterDef;

		public bool wantsThreatPoints;

		public float minThreatPoints;

		public bool increasesPopulation;

		public bool badEvenIfNoMap;

		public float forceExitAndRemoveMapCountdownDurationDays = 4f;

		public bool handlesWorldObjectTimeoutInspectString;

		public string mainPartAllThreatsLabel;

		[Unsaved(false)]
		private SitePartWorker workerInt;

		[Unsaved(false)]
		private Texture2D expandingIconTextureInt;

		[Unsaved(false)]
		private List<GenStepDef> extraGenSteps;

		public SitePartWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (SitePartWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public Texture2D ExpandingIconTexture
		{
			get
			{
				if (expandingIconTextureInt == null)
				{
					if (!expandingIconTexture.NullOrEmpty())
					{
						expandingIconTextureInt = ContentFinder<Texture2D>.Get(expandingIconTexture);
					}
					else
					{
						expandingIconTextureInt = BaseContent.BadTex;
					}
				}
				return expandingIconTextureInt;
			}
		}

		public List<GenStepDef> ExtraGenSteps
		{
			get
			{
				if (extraGenSteps == null)
				{
					extraGenSteps = new List<GenStepDef>();
					List<GenStepDef> allDefsListForReading = DefDatabase<GenStepDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].linkWithSite == this)
						{
							extraGenSteps.Add(allDefsListForReading[i]);
						}
					}
				}
				return extraGenSteps;
			}
		}

		public SitePartDef()
		{
			workerClass = typeof(SitePartWorker);
		}

		public bool FactionCanOwn(Faction faction)
		{
			if (requiresFaction && faction == null)
			{
				return false;
			}
			if (minFactionTechLevel != 0 && (faction == null || (int)faction.def.techLevel < (int)minFactionTechLevel))
			{
				return false;
			}
			if (faction != null && (faction.IsPlayer || faction.defeated || faction.Hidden))
			{
				return false;
			}
			if (!Worker.FactionCanOwn(faction))
			{
				return false;
			}
			return true;
		}

		public bool CompatibleWith(SitePartDef part)
		{
			for (int i = 0; i < part.excludesTags.Count; i++)
			{
				if (tags.Contains(part.excludesTags[i]))
				{
					return false;
				}
			}
			for (int j = 0; j < excludesTags.Count; j++)
			{
				if (part.tags.Contains(excludesTags[j]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
