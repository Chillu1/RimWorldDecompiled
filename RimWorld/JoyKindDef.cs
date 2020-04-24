using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class JoyKindDef : Def
	{
		public List<RoyalTitleDef> titleRequiredAny;

		public bool needsThing = true;

		public bool PawnCanDo(Pawn pawn)
		{
			if (pawn.royalty != null)
			{
				foreach (RoyalTitle item in pawn.royalty.AllTitlesInEffectForReading)
				{
					if (item.conceited && item.def.JoyKindDisabled(this))
					{
						return false;
					}
				}
				if (titleRequiredAny != null)
				{
					bool flag = false;
					foreach (RoyalTitle item2 in pawn.royalty.AllTitlesInEffectForReading)
					{
						if (titleRequiredAny.Contains(item2.def))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
