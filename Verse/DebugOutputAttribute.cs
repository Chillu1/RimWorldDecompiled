using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Method)]
	public class DebugOutputAttribute : Attribute
	{
		public string name;

		public string category = "General";

		public bool onlyWhenPlaying;

		public DebugOutputAttribute()
		{
		}

		public DebugOutputAttribute(bool onlyWhenPlaying)
		{
			this.onlyWhenPlaying = onlyWhenPlaying;
		}

		public DebugOutputAttribute(string category, bool onlyWhenPlaying = false)
			: this(onlyWhenPlaying)
		{
			this.category = category;
		}
	}
}
