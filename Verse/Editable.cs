using System.Collections.Generic;

namespace Verse
{
	public class Editable
	{
		public virtual void ResolveReferences()
		{
		}

		public virtual void PostLoad()
		{
		}

		public virtual IEnumerable<string> ConfigErrors()
		{
			yield break;
		}
	}
}
