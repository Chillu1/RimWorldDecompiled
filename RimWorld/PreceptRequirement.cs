using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class PreceptRequirement : IExposable
	{
		public abstract bool Met(List<Precept> precepts);

		public abstract Precept MakePrecept(Ideo ideo);

		public virtual void ExposeData()
		{
		}
	}
}
