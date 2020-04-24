using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_AssignCaravanDrugPolicies : Window
	{
		private Caravan caravan;

		private Vector2 scrollPos;

		private float lastHeight;

		private const float RowHeight = 30f;

		private const float AssignDrugPolicyButtonsTotalWidth = 354f;

		private const int ManageDrugPoliciesButtonHeight = 32;

		public override Vector2 InitialSize => new Vector2(550f, 500f);

		public Dialog_AssignCaravanDrugPolicies(Caravan caravan)
		{
			this.caravan = caravan;
			doCloseButton = true;
		}

		public override void DoWindowContents(Rect rect)
		{
			rect.height -= CloseButSize.y;
			float num = 0f;
			if (Widgets.ButtonText(new Rect(rect.width - 354f - 16f, num, 354f, 32f), "ManageDrugPolicies".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(null));
			}
			num += 42f;
			Rect outRect = new Rect(0f, num, rect.width, rect.height - num);
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, lastHeight);
			Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
			float num2 = 0f;
			for (int i = 0; i < caravan.pawns.Count; i++)
			{
				if (caravan.pawns[i].drugs != null)
				{
					if (num2 + 30f >= scrollPos.y && num2 <= scrollPos.y + outRect.height)
					{
						DoRow(new Rect(0f, num2, viewRect.width, 30f), caravan.pawns[i]);
					}
					num2 += 30f;
				}
			}
			lastHeight = num2;
			Widgets.EndScrollView();
		}

		private void DoRow(Rect rect, Pawn pawn)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width - 354f, 30f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Widgets.Label(rect2, pawn.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;
			GUI.color = Color.white;
			DrugPolicyUIUtility.DoAssignDrugPolicyButtons(new Rect(rect.x + rect.width - 354f, rect.y, 354f, 30f), pawn);
		}
	}
}
