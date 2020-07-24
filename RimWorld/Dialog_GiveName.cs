using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_GiveName : Window
	{
		protected Pawn suggestingPawn;

		protected string curName;

		protected Func<string> nameGenerator;

		protected string nameMessageKey;

		protected string gainedNameMessageKey;

		protected string invalidNameMessageKey;

		protected bool useSecondName;

		protected string curSecondName;

		protected Func<string> secondNameGenerator;

		protected string secondNameMessageKey;

		protected string invalidSecondNameMessageKey;

		private float Height
		{
			get
			{
				if (!useSecondName)
				{
					return 200f;
				}
				return 300f;
			}
		}

		public override Vector2 InitialSize => new Vector2(640f, Height);

		public Dialog_GiveName()
		{
			if (Find.AnyPlayerHomeMap != null && Find.AnyPlayerHomeMap.mapPawns.FreeColonistsCount != 0)
			{
				if (Find.AnyPlayerHomeMap.mapPawns.FreeColonistsSpawnedCount != 0)
				{
					suggestingPawn = Find.AnyPlayerHomeMap.mapPawns.FreeColonistsSpawned.RandomElement();
				}
				else
				{
					suggestingPawn = Find.AnyPlayerHomeMap.mapPawns.FreeColonists.RandomElement();
				}
			}
			else
			{
				suggestingPawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.RandomElement();
			}
			forcePause = true;
			closeOnAccept = false;
			closeOnCancel = false;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect rect)
		{
			Text.Font = GameFont.Small;
			bool flag = false;
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
			{
				flag = true;
				Event.current.Use();
			}
			Rect rect2;
			if (!useSecondName)
			{
				Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), nameMessageKey.Translate(suggestingPawn.LabelShort, suggestingPawn).CapitalizeFirst());
				if (nameGenerator != null && Widgets.ButtonText(new Rect(rect.width / 2f + 90f, 80f, rect.width / 2f - 90f, 35f), "Randomize".Translate()))
				{
					curName = nameGenerator();
				}
				curName = Widgets.TextField(new Rect(0f, 80f, rect.width / 2f + 70f, 35f), curName);
				rect2 = new Rect(rect.width / 2f + 90f, rect.height - 35f, rect.width / 2f - 90f, 35f);
			}
			else
			{
				float num = 0f;
				string text = nameMessageKey.Translate(suggestingPawn.LabelShort, suggestingPawn).CapitalizeFirst();
				Widgets.Label(new Rect(0f, num, rect.width, rect.height), text);
				num += Text.CalcHeight(text, rect.width) + 10f;
				if (nameGenerator != null && Widgets.ButtonText(new Rect(rect.width / 2f + 90f, num, rect.width / 2f - 90f, 35f), "Randomize".Translate()))
				{
					curName = nameGenerator();
				}
				curName = Widgets.TextField(new Rect(0f, num, rect.width / 2f + 70f, 35f), curName);
				num += 60f;
				text = secondNameMessageKey.Translate(suggestingPawn.LabelShort, suggestingPawn);
				Widgets.Label(new Rect(0f, num, rect.width, rect.height), text);
				num += Text.CalcHeight(text, rect.width) + 10f;
				if (secondNameGenerator != null && Widgets.ButtonText(new Rect(rect.width / 2f + 90f, num, rect.width / 2f - 90f, 35f), "Randomize".Translate()))
				{
					curSecondName = secondNameGenerator();
				}
				curSecondName = Widgets.TextField(new Rect(0f, num, rect.width / 2f + 70f, 35f), curSecondName);
				num += 45f;
				float num2 = rect.width / 2f - 90f;
				rect2 = new Rect(rect.width / 2f - num2 / 2f, rect.height - 35f, num2, 35f);
			}
			if (!(Widgets.ButtonText(rect2, "OK".Translate()) || flag))
			{
				return;
			}
			if (IsValidName(curName) && (!useSecondName || IsValidSecondName(curSecondName)))
			{
				if (useSecondName)
				{
					Named(curName);
					NamedSecond(curSecondName);
					Messages.Message(gainedNameMessageKey.Translate(curName, curSecondName), MessageTypeDefOf.TaskCompletion, historical: false);
				}
				else
				{
					Named(curName);
					Messages.Message(gainedNameMessageKey.Translate(curName), MessageTypeDefOf.TaskCompletion, historical: false);
				}
				Find.WindowStack.TryRemove(this);
			}
			else
			{
				Messages.Message(invalidNameMessageKey.Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			Event.current.Use();
		}

		protected abstract bool IsValidName(string s);

		protected abstract void Named(string s);

		protected virtual bool IsValidSecondName(string s)
		{
			return true;
		}

		protected virtual void NamedSecond(string s)
		{
		}
	}
}
