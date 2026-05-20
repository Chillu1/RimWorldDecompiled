using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IPreceptCompDescriptionArgs
	{
		IEnumerable<NamedArgument> GetDescriptionArgs();
	}
}
