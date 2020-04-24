using System.Collections.Generic;

namespace Verse
{
	public abstract class DefModExtension
	{
		public virtual IEnumerable<string> ConfigErrors()
		{
			yield break;
		}
	}
}
