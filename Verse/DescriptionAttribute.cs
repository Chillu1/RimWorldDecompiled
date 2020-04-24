using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field)]
	public class DescriptionAttribute : Attribute
	{
		public string description;

		public DescriptionAttribute(string description)
		{
			this.description = description;
		}
	}
}
