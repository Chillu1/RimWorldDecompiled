using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GeneGizmo_DeathrestCapacity : Gizmo
	{
		protected Gene_Deathrest gene;

		private const float Padding = 6f;

		private const float Width = 140f;

		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}

		public GeneGizmo_DeathrestCapacity(Gene_Deathrest gene)
		{
			this.gene = gene;
			Order = -100f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect position = rect.ContractedBy(6f);
			float num = position.height / 3f;
			Widgets.DrawWindowBackground(rect);
			GUI.BeginGroup(position);
			Widgets.Label(new Rect(0f, 0f, position.width, num), "Deathrest".Translate().CapitalizeFirst());
			if (gene.DeathrestNeed != null)
			{
				gene.DeathrestNeed.DrawOnGUI(new Rect(0f, num, position.width, num + 2f), int.MaxValue, 2f, drawArrows: false, doTooltip: true, new Rect(0f, 0f, position.width, num * 2f), drawLabel: false);
			}
			Rect rect2 = new Rect(0f, num * 2f, position.width, Text.LineHeight);
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect2, string.Format("{0}: {1} / {2}", "Buildings".Translate().CapitalizeFirst(), gene.CurrentCapacity, gene.DeathrestCapacity));
			Text.Anchor = TextAnchor.UpperLeft;
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, "DeathrestCapacityDesc".Translate() + "\n\n" + "PawnIsConnectedToBuildings".Translate(gene.pawn.Named("PAWN"), gene.CurrentCapacity.Named("CURRENT"), gene.DeathrestCapacity.Named("MAX")));
			}
			GUI.EndGroup();
			return new GizmoResult(GizmoState.Clear);
		}
	}
}
