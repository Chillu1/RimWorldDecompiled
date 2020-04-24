using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field)]
	public class TranslationHandleAttribute : Attribute
	{
		public int Priority
		{
			get;
			set;
		}
	}
}
