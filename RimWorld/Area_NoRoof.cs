using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_NoRoof : Area
	{
		public override string Label => "NoRoof".Translate();

		public override Color Color => new Color(0.9f, 0.5f, 0.1f);

		public override int ListPriority => 8000;

		public Area_NoRoof()
		{
		}

		public Area_NoRoof(AreaManager areaManager)
			: base(areaManager)
		{
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + ID + "_NoRoof";
		}
	}
}
