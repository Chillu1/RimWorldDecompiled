using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_PollutionClear : Area
	{
		public override string Label => "PollutionClear".Translate();

		public override Color Color => new Color(0.1f, 0.8f, 0.1f);

		public override int ListPriority => 5000;

		public Area_PollutionClear()
		{
		}

		public Area_PollutionClear(AreaManager areaManager)
			: base(areaManager)
		{
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + ID + "_PollutionClear";
		}
	}
}
