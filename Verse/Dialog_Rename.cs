using RimWorld;
using UnityEngine;

namespace Verse;

public abstract class Dialog_Rename<T> : Window where T : class, IRenameable
{
	protected string curName;

	private bool focusedRenameField;

	private int startAcceptingInputAtFrame;

	protected readonly T renaming;

	private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

	protected virtual int MaxNameLength => 28;

	public override Vector2 InitialSize => new Vector2(280f, 175f);

	protected Dialog_Rename(T renaming)
	{
		this.renaming = renaming;
		curName = renaming?.RenamableLabel;
		doCloseX = true;
		forcePause = true;
		closeOnAccept = false;
		closeOnClickedOutside = true;
		absorbInputAroundWindow = true;
	}

	public void WasOpenedByHotkey()
	{
		startAcceptingInputAtFrame = Time.frameCount + 1;
	}

	protected virtual AcceptanceReport NameIsValid(string name)
	{
		return name.Length != 0;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		bool flag = false;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
		{
			flag = true;
			Event.current.Use();
		}
		Rect rect = new Rect(inRect);
		Text.Font = GameFont.Medium;
		rect.height = Text.LineHeight + 10f;
		Widgets.Label(rect, "Rename".Translate());
		Text.Font = GameFont.Small;
		GUI.SetNextControlName("RenameField");
		string text = Widgets.TextField(new Rect(0f, rect.height, inRect.width, 35f), curName);
		if (AcceptsInput && text.Length < MaxNameLength)
		{
			curName = text;
		}
		else if (!AcceptsInput)
		{
			((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).SelectAll();
		}
		if (!focusedRenameField)
		{
			UI.FocusControl("RenameField", this);
			focusedRenameField = true;
		}
		if (!(Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 10f, inRect.width - 15f - 15f, 35f), "OK") || flag))
		{
			return;
		}
		AcceptanceReport acceptanceReport = NameIsValid(curName);
		if (!acceptanceReport.Accepted)
		{
			if (acceptanceReport.Reason.NullOrEmpty())
			{
				Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
			}
			return;
		}
		if (renaming != null)
		{
			renaming.RenamableLabel = curName;
		}
		OnRenamed(curName);
		Find.WindowStack.TryRemove(this);
	}

	protected virtual void OnRenamed(string name)
	{
	}
}
