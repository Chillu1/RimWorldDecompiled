using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class XenotypeIconDef : Def
	{
		[NoTranslate]
		public string texPath;

		[Unsaved(false)]
		private Texture2D cachedIcon;

		public Texture2D Icon
		{
			get
			{
				if (cachedIcon == null)
				{
					cachedIcon = ContentFinder<Texture2D>.Get(texPath);
				}
				return cachedIcon;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (texPath.NullOrEmpty())
			{
				yield return "texPath is empty";
			}
		}
	}
}
