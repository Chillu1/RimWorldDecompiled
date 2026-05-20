using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GeneLeftChosenGroup
	{
		public GeneDef leftChosen;

		public List<GeneDef> overriddenGenes = new List<GeneDef>();

		public GeneLeftChosenGroup(GeneDef left)
		{
			leftChosen = left;
		}
	}
}
