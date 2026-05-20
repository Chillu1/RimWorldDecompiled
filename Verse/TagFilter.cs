using System.Collections.Generic;

namespace Verse
{
	public class TagFilter
	{
		public List<string> tags = new List<string>();

		public bool whitelist = true;

		public bool Allows(List<string> otherTags)
		{
			if (otherTags != null)
			{
				for (int i = 0; i < otherTags.Count; i++)
				{
					if (tags.Contains(otherTags[i]))
					{
						return whitelist;
					}
				}
			}
			return !whitelist;
		}
	}
}
