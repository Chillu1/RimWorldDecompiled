using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IdeoPresetDef : Def
	{
		public IdeoPresetCategoryDef categoryDef;

		public List<MemeDef> memes = new List<MemeDef>();

		[NoTranslate]
		public string iconPath;

		public bool classicPlus;

		private Texture2D cachedIcon;

		public Texture2D Icon
		{
			get
			{
				if (iconPath.NullOrEmpty())
				{
					return null;
				}
				if (cachedIcon == null)
				{
					cachedIcon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return cachedIcon;
			}
		}
	}
}
