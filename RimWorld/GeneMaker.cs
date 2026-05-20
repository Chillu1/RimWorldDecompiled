using System;
using Verse;

namespace RimWorld
{
	public static class GeneMaker
	{
		public static Gene MakeGene(GeneDef def, Pawn pawn)
		{
			Gene obj = (Gene)Activator.CreateInstance(def.geneClass);
			obj.def = def;
			obj.pawn = pawn;
			obj.loadID = Find.UniqueIDsManager.GetNextGeneID();
			obj.PostMake();
			return obj;
		}
	}
}
