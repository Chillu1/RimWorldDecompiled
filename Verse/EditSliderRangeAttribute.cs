using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field)]
	public class EditSliderRangeAttribute : Attribute
	{
		public float min;

		public float max = 1f;

		public EditSliderRangeAttribute(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}
}
