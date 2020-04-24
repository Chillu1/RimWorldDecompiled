using UnityEngine;

namespace RimWorld
{
	public class ITab_Pawn_Needs : ITab
	{
		private Vector2 thoughtScrollPosition;

		public override bool IsVisible
		{
			get
			{
				if (base.SelPawn.RaceProps.Animal && base.SelPawn.Faction == null)
				{
					return false;
				}
				if (base.SelPawn.needs != null)
				{
					return base.SelPawn.needs.AllNeeds.Count > 0;
				}
				return false;
			}
		}

		public ITab_Pawn_Needs()
		{
			labelKey = "TabNeeds";
			tutorTag = "Needs";
		}

		public override void OnOpen()
		{
			thoughtScrollPosition = default(Vector2);
		}

		protected override void FillTab()
		{
			NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, size.x, size.y), base.SelPawn, ref thoughtScrollPosition);
		}

		protected override void UpdateSize()
		{
			size = NeedsCardUtility.GetSize(base.SelPawn);
		}
	}
}
