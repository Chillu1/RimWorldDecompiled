using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class TweakValue : Attribute
	{
		public string category;

		public float min;

		public float max;

		public TweakValue(string category, float min = 0f, float max = 100f)
		{
			this.category = category;
			this.min = min;
			this.max = max;
		}
	}
}
