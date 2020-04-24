using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_SnowClear : Area
	{
		public override string Label => "SnowClear".Translate();

		public override Color Color => new Color(0.8f, 0.1f, 0.1f);

		public override int ListPriority => 5000;

		public Area_SnowClear()
		{
		}

		public Area_SnowClear(AreaManager areaManager)
			: base(areaManager)
		{
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + ID + "_SnowClear";
		}
	}
}
