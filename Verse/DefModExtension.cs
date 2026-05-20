using System.Collections.Generic;
using System.Linq;

namespace Verse;

public abstract class DefModExtension
{
	public virtual IEnumerable<string> ConfigErrors()
	{
		return Enumerable.Empty<string>();
	}

	public virtual void ResolveReferences(Def parentDef)
	{
	}
}
