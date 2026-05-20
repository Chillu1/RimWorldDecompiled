using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ViewGenes : Window
	{
		private Pawn target;

		private Vector2 scrollPosition;

		private const float HeaderHeight = 30f;

		public override Vector2 InitialSize => new Vector2(736f, 700f);

		public Dialog_ViewGenes(Pawn target)
		{
			this.target = target;
			closeOnClickedOutside = true;
		}

		public override void PostOpen()
		{
			if (!ModLister.CheckBiotech("genes viewing"))
			{
				Close(doCloseSound: false);
			}
			else
			{
				base.PostOpen();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			inRect.yMax -= Window.CloseButSize.y;
			Rect rect = inRect;
			rect.xMin += 34f;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, "ViewGenes".Translate() + ": " + target.genes.XenotypeLabelCap);
			Text.Font = GameFont.Small;
			GUI.color = XenotypeDef.IconColor;
			GUI.DrawTexture(new Rect(inRect.x, inRect.y, 30f, 30f), target.genes.XenotypeIcon);
			GUI.color = Color.white;
			inRect.yMin += 34f;
			Vector2 size = Vector2.zero;
			GeneUIUtility.DrawGenesInfo(inRect, target, InitialSize.y, ref size, ref scrollPosition);
			if (Widgets.ButtonText(new Rect(inRect.xMax - Window.CloseButSize.x, inRect.yMax, Window.CloseButSize.x, Window.CloseButSize.y), "Close".Translate()))
			{
				Close();
			}
		}
	}
}
