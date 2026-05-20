using System.Collections.Generic;
using System.Linq;

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
			return Enumerable.Empty<string>();
		}
	}
}
