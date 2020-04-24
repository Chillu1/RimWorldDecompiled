using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class LifeStageDef : Def
	{
		[MustTranslate]
		private string adjective;

		public bool visible = true;

		public bool reproductive;

		public bool milkable;

		public bool shearable;

		public float voxPitch = 1f;

		public float voxVolume = 1f;

		[NoTranslate]
		public string icon;

		[Unsaved(false)]
		public Texture2D iconTex;

		public List<StatModifier> statFactors = new List<StatModifier>();

		public float bodySizeFactor = 1f;

		public float healthScaleFactor = 1f;

		public float hungerRateFactor = 1f;

		public float marketValueFactor = 1f;

		public float foodMaxFactor = 1f;

		public float meleeDamageFactor = 1f;

		public string Adjective => adjective ?? label;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (!icon.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					iconTex = ContentFinder<Texture2D>.Get(icon);
				});
			}
		}
	}
}
