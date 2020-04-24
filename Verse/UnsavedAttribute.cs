using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field)]
	public class UnsavedAttribute : Attribute
	{
		public bool allowLoading;

		public UnsavedAttribute(bool allowLoading = false)
		{
			this.allowLoading = allowLoading;
		}
	}
}
