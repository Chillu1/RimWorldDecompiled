using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompProperties_BiosculpterPod_BaseCycle : CompProperties
	{
		[NoTranslate]
		public string key;

		[MustTranslate]
		public string label;

		[MustTranslate]
		public string description;

		[NoTranslate]
		public string iconPath;

		public float durationDays;

		public Color operatingColor = new Color(0.5f, 0.7f, 0.5f);

		public ThoughtDef gainThoughtOnCompletion;

		public List<ResearchProjectDef> requiredResearch;

		public List<ThingDefCountClass> extraRequiredIngredients;

		private Texture2D icon;

		public Texture2D Icon
		{
			get
			{
				if (icon == null)
				{
					icon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return icon;
			}
		}

		public string LabelCap => label.CapitalizeFirst();
	}
}
