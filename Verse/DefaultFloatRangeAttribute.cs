using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field)]
	public class DefaultFloatRangeAttribute : DefaultValueAttribute
	{
		public DefaultFloatRangeAttribute(float min, float max)
			: base(new FloatRange(min, max))
		{
		}
	}
}
