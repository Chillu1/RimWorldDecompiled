using System;
using UnityEngine;
using Verse;

namespace RimWorld;

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

	protected virtual int FirstCharLimit => 64;

	protected virtual int SecondCharLimit => 64;

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
			suggestingPawn = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended.RandomElement();
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
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
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
			curName = Widgets.TextField(new Rect(0f, 80f, rect.width / 2f + 70f, 35f), curName, FirstCharLimit);
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
			curName = Widgets.TextField(new Rect(0f, num, rect.width / 2f + 70f, 35f), curName, FirstCharLimit);
			num += 60f;
			text = secondNameMessageKey.Translate(suggestingPawn.LabelShort, suggestingPawn);
			Widgets.Label(new Rect(0f, num, rect.width, rect.height), text);
			num += Text.CalcHeight(text, rect.width) + 10f;
			if (secondNameGenerator != null && Widgets.ButtonText(new Rect(rect.width / 2f + 90f, num, rect.width / 2f - 90f, 35f), "Randomize".Translate()))
			{
				curSecondName = secondNameGenerator();
			}
			curSecondName = Widgets.TextField(new Rect(0f, num, rect.width / 2f + 70f, 35f), curSecondName, SecondCharLimit);
			num += 45f;
			float num2 = rect.width / 2f - 90f;
			rect2 = new Rect(rect.width / 2f - num2 / 2f, rect.height - 35f, num2, 35f);
		}
		if (!(Widgets.ButtonText(rect2, "OK".Translate()) || flag))
		{
			return;
		}
		string text2 = curName?.Trim();
		string text3 = curSecondName?.Trim();
		if (IsValidName(text2) && (!useSecondName || IsValidSecondName(text3)))
		{
			if (useSecondName)
			{
				Named(text2);
				NamedSecond(text3);
				Messages.Message(gainedNameMessageKey.Translate(text2, text3), MessageTypeDefOf.TaskCompletion, historical: false);
			}
			else
			{
				Named(text2);
				Messages.Message(gainedNameMessageKey.Translate(text2), MessageTypeDefOf.TaskCompletion, historical: false);
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
