using UnityEngine;

namespace RimWorld
{
	public class PawnColumnWorker_AllowedAreaWide : PawnColumnWorker_AllowedArea
	{
		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(350, GetMinWidth(table), GetMaxWidth(table));
		}
	}
}
