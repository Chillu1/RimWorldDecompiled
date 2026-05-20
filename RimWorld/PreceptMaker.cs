using System;

namespace RimWorld
{
	public static class PreceptMaker
	{
		public static Precept MakePrecept(PreceptDef def)
		{
			Precept obj = (Precept)Activator.CreateInstance(def.preceptClass);
			obj.def = def;
			obj.PostMake();
			return obj;
		}
	}
}
