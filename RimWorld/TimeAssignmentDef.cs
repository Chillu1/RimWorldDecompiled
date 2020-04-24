using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TimeAssignmentDef : Def
	{
		public Color color;

		public bool allowRest = true;

		public bool allowJoy = true;

		private Texture2D colorTextureInt;

		public Texture2D ColorTexture
		{
			get
			{
				if (colorTextureInt == null)
				{
					colorTextureInt = SolidColorMaterials.NewSolidColorTexture(color);
				}
				return colorTextureInt;
			}
		}
	}
}
