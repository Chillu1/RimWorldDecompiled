using UnityEngine;

namespace Verse
{
	public class ColorGenerator_Single : ColorGenerator
	{
		public Color color;

		public override Color NewRandomizedColor()
		{
			return color;
		}
	}
}
