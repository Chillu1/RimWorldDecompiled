using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_EditIdeoDescription : Window
	{
		private Ideo ideo;

		private string newDescription;

		private string newDescriptionTemplate;

		private static readonly Vector2 ButSize = new Vector2(150f, 38f);

		private const float HeaderHeight = 35f;

		public override Vector2 InitialSize => new Vector2(700f, 400f);

		public Dialog_EditIdeoDescription(Ideo ideo)
		{
			this.ideo = ideo;
			newDescription = ideo.description;
			absorbInputAroundWindow = true;
		}

		public override void OnAcceptKeyPressed()
		{
			Event.current.Use();
		}

		public override void DoWindowContents(Rect rect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(rect.x, rect.y, rect.width, 35f), "EditNarrative".Translate());
			Text.Font = GameFont.Small;
			float num = rect.y + 35f + 10f;
			string text = Widgets.TextArea(new Rect(rect.x, num, rect.width, rect.height - ButSize.y - 17f - num), newDescription);
			if (text != newDescription)
			{
				newDescription = text;
				newDescriptionTemplate = null;
			}
			if (Widgets.ButtonText(new Rect(0f, rect.height - ButSize.y, ButSize.x, ButSize.y), "Cancel".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(rect.width / 2f - ButSize.x / 2f, rect.height - ButSize.y, ButSize.x, ButSize.y), "Randomize".Translate()))
			{
				IdeoDescriptionResult ideoDescriptionResult = ideo.GetNewDescription(force: true);
				newDescription = ideoDescriptionResult.text;
				newDescriptionTemplate = ideoDescriptionResult.template;
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			if (Widgets.ButtonText(new Rect(rect.width - ButSize.x, rect.height - ButSize.y, ButSize.x, ButSize.y), "DoneButton".Translate()))
			{
				ApplyChanges();
			}
		}

		private void ApplyChanges()
		{
			if (ideo.description != newDescription)
			{
				ideo.description = newDescription;
				ideo.descriptionTemplate = newDescriptionTemplate;
				ideo.descriptionLocked = true;
			}
			Close();
		}
	}
}
