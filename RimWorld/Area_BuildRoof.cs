using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_BuildRoof : Area
	{
		public override string Label => "BuildRoof".Translate();

		public override Color Color => new Color(0.9f, 0.9f, 0.5f);

		public override int ListPriority => 9000;

		public Area_BuildRoof()
		{
		}

		public Area_BuildRoof(AreaManager areaManager)
			: base(areaManager)
		{
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + ID + "_BuildRoof";
		}
	}
}
