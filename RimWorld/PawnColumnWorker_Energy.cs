using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_Energy : PawnColumnWorker
	{
		private const int Width = 120;

		private const int BarPadding = 4;

		public static readonly Texture2D EnergyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(252, byte.MaxValue, byte.MaxValue, 65));

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (!pawn.IsGestating())
			{
				Widgets.FillableBar(rect.ContractedBy(4f), pawn.needs.energy.CurLevelPercentage, EnergyBarTex, BaseContent.ClearTex, doBorder: false);
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect, Mathf.RoundToInt(pawn.needs.energy.CurLevel) + " / " + Mathf.RoundToInt(pawn.needs.energy.MaxLevel));
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 120);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), GetMinWidth(table));
		}
	}
}
