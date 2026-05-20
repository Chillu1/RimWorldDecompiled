using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IdeoIconDef : IdeoSymbolPartDef
	{
		[NoTranslate]
		public string iconPath;

		[Unsaved(false)]
		private Texture2D cachedIcon;

		public Texture2D Icon
		{
			get
			{
				if (cachedIcon == null)
				{
					cachedIcon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return cachedIcon;
			}
		}
	}
}
