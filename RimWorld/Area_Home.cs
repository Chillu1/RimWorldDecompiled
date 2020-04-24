using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_Home : Area
	{
		public override string Label => "Home".Translate();

		public override Color Color => new Color(0.3f, 0.3f, 0.9f);

		public override int ListPriority => 10000;

		public Area_Home()
		{
		}

		public Area_Home(AreaManager areaManager)
			: base(areaManager)
		{
		}

		public override bool AssignableAsAllowed()
		{
			return true;
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + ID + "_Home";
		}

		protected override void Set(IntVec3 c, bool val)
		{
			if (base[c] != val)
			{
				base.Set(c, val);
				base.Map.listerFilthInHomeArea.Notify_HomeAreaChanged(c);
			}
		}
	}
}
