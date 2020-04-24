using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class FleshTypeDef : Def
	{
		public class Wound
		{
			[NoTranslate]
			public string texture;

			public Color color = Color.white;

			public Material GetMaterial()
			{
				return MaterialPool.MatFrom(texture, ShaderDatabase.Cutout, color);
			}
		}

		public ThoughtDef ateDirect;

		public ThoughtDef ateAsIngredient;

		public ThingCategoryDef corpseCategory;

		public EffecterDef damageEffecter;

		public List<Wound> wounds;

		private List<Material> woundsResolved;

		public Material ChooseWoundOverlay()
		{
			if (wounds == null)
			{
				return null;
			}
			if (woundsResolved == null)
			{
				woundsResolved = wounds.Select((Wound wound) => wound.GetMaterial()).ToList();
			}
			return woundsResolved.RandomElement();
		}
	}
}
