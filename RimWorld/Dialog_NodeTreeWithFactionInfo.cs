using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_NodeTreeWithFactionInfo : Dialog_NodeTree
	{
		private Faction faction;

		private const float RelatedFactionInfoSize = 79f;

		public Dialog_NodeTreeWithFactionInfo(DiaNode nodeRoot, Faction faction, bool delayInteractivity = false, bool radioMode = false, string title = null)
			: base(nodeRoot, delayInteractivity, radioMode, title)
		{
			this.faction = faction;
			if (faction != null)
			{
				minOptionsAreaHeight = 60f;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			if (faction != null)
			{
				float curY = inRect.height - 79f;
				FactionUIUtility.DrawRelatedFactionInfo(inRect, faction, ref curY);
			}
		}
	}
}
