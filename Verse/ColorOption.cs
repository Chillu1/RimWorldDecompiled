using UnityEngine;

namespace Verse
{
	public class ColorOption
	{
		public float weight = 1f;

		public Color min = new Color(-1f, -1f, -1f, -1f);

		public Color max = new Color(-1f, -1f, -1f, -1f);

		public Color only = new Color(-1f, -1f, -1f, -1f);

		public Color RandomizedColor()
		{
			if (only.a >= 0f)
			{
				return only;
			}
			return new Color(Rand.Range(min.r, max.r), Rand.Range(min.g, max.g), Rand.Range(min.b, max.b), Rand.Range(min.a, max.a));
		}

		public void SetSingle(Color color)
		{
			only = color;
		}

		public void SetMin(Color color)
		{
			min = color;
		}

		public void SetMax(Color color)
		{
			max = color;
		}
	}
}
