using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct ThingSetMakerParams
	{
		public TechLevel? techLevel;

		public IntRange? countRange;

		public ThingFilter filter;

		public Predicate<ThingDef> validator;

		public QualityGenerator? qualityGenerator;

		public float? maxTotalMass;

		public float? maxThingMarketValue;

		public bool? allowNonStackableDuplicates;

		public float? minSingleItemMarketValuePct;

		public FloatRange? totalMarketValueRange;

		public FloatRange? totalNutritionRange;

		public PodContentsType? podContentsType;

		public Faction makingFaction;

		public TraderKindDef traderDef;

		public int? tile;

		public Dictionary<string, object> custom;
	}
}
