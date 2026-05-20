using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_EditDeity : Window
	{
		private IdeoFoundation_Deity.Deity deity;

		private Ideo ideo;

		private string newDeityName;

		private string newDeityTitle;

		private Gender newDeityGender;

		private static readonly Vector2 ButSize = new Vector2(150f, 38f);

		private static readonly float EditFieldHeight = 30f;

		public override Vector2 InitialSize => new Vector2(700f, 250f);

		public Dialog_EditDeity(IdeoFoundation_Deity.Deity deity, Ideo ideo)
		{
			this.deity = deity;
			this.ideo = ideo;
			absorbInputAroundWindow = true;
			newDeityName = deity.name;
			newDeityTitle = deity.type;
			newDeityGender = deity.gender;
		}

		public override void OnAcceptKeyPressed()
		{
			ApplyChanges();
			Event.current.Use();
		}

		public override void DoWindowContents(Rect rect)
		{
			float num = rect.x + rect.width / 3f;
			float width = rect.xMax - num;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(rect.x, rect.y, rect.width, 35f), "EditDeity".Translate());
			Text.Font = GameFont.Small;
			float num2 = rect.y + 35f + 10f;
			Widgets.Label(new Rect(rect.x, num2, width, EditFieldHeight), "DeityName".Translate());
			newDeityName = Widgets.TextField(new Rect(num, num2, width, EditFieldHeight), newDeityName);
			num2 += EditFieldHeight + 10f;
			Widgets.Label(new Rect(rect.x, num2, width, EditFieldHeight), "DeityTitle".Translate());
			newDeityTitle = Widgets.TextField(new Rect(num, num2, width, EditFieldHeight), newDeityTitle);
			num2 += EditFieldHeight + 10f;
			Widgets.Label(new Rect(rect.x, num2, width, EditFieldHeight), "DeityGender".Translate());
			Rect rect2 = new Rect(num, num2, EditFieldHeight + 8f + Text.CalcSize(newDeityGender.GetLabel().CapitalizeFirst()).x, EditFieldHeight);
			Rect rect3 = new Rect(rect2.x, num2, EditFieldHeight, EditFieldHeight);
			GUI.DrawTexture(rect3.ContractedBy(2f), newDeityGender.GetIcon());
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(new Rect(rect3.xMax + 4f, num2, rect2.width - rect3.width, EditFieldHeight), newDeityGender.GetLabel().CapitalizeFirst());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.DrawHighlightIfMouseover(rect2);
			if (Widgets.ButtonInvisible(rect2))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				Gender[] array = (Gender[])Enum.GetValues(typeof(Gender));
				foreach (Gender g in array)
				{
					list.Add(new FloatMenuOption(g.GetLabel().CapitalizeFirst(), delegate
					{
						newDeityGender = g;
					}, g.GetIcon(), Color.white));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			num2 += EditFieldHeight + 10f;
			if (Widgets.ButtonText(new Rect(0f, rect.height - ButSize.y, ButSize.x, ButSize.y), "Back".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(rect.width - ButSize.x, rect.height - ButSize.y, ButSize.x, ButSize.y), "DoneButton".Translate()))
			{
				ApplyChanges();
			}
		}

		private void ApplyChanges()
		{
			deity.name = newDeityName;
			deity.type = newDeityTitle;
			deity.gender = newDeityGender;
			ideo.RegenerateAllPreceptNames();
			ideo.RegenerateDescription();
			Close();
		}
	}
}
