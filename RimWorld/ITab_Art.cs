using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Art : ITab
	{
		private static string cachedImageDescription;

		private static CompArt cachedImageSource;

		private static TaleReference cachedTaleRef;

		private static readonly Vector2 WinSize = new Vector2(400f, 300f);

		private CompArt SelectedCompArt
		{
			get
			{
				Thing thing = Find.Selector.SingleSelectedThing;
				MinifiedThing minifiedThing = thing as MinifiedThing;
				if (minifiedThing != null)
				{
					thing = minifiedThing.InnerThing;
				}
				return thing?.TryGetComp<CompArt>();
			}
		}

		public override bool IsVisible
		{
			get
			{
				if (SelectedCompArt != null)
				{
					return SelectedCompArt.Active;
				}
				return false;
			}
		}

		public ITab_Art()
		{
			size = WinSize;
			labelKey = "TabArt";
			tutorTag = "Art";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, SelectedCompArt.Title);
			if (cachedImageSource != SelectedCompArt || cachedTaleRef != SelectedCompArt.TaleRef)
			{
				cachedImageDescription = SelectedCompArt.GenerateImageDescription();
				cachedImageSource = SelectedCompArt;
				cachedTaleRef = SelectedCompArt.TaleRef;
			}
			Rect rect2 = rect;
			rect2.yMin += 35f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, cachedImageDescription);
		}
	}
}
