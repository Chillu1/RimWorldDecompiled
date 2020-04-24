using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class IgnoreSavedElementAttribute : Attribute
	{
		public string elementToIgnore;

		public IgnoreSavedElementAttribute(string elementToIgnore)
		{
			this.elementToIgnore = elementToIgnore;
		}
	}
}
