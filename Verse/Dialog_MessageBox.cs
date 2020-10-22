using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_MessageBox : Window
	{
		public TaggedString text;

		public string title;

		public string buttonAText;

		public Action buttonAAction;

		public bool buttonADestructive;

		public string buttonBText;

		public Action buttonBAction;

		public string buttonCText;

		public Action buttonCAction;

		public bool buttonCClose = true;

		public float interactionDelay;

		public Action acceptAction;

		public Action cancelAction;

		private Vector2 scrollPosition = Vector2.zero;

		private float creationRealTime = -1f;

		private const float TitleHeight = 42f;

		protected const float ButtonHeight = 35f;

		public override Vector2 InitialSize => new Vector2(640f, 460f);

		private float TimeUntilInteractive => interactionDelay - (Time.realtimeSinceStartup - creationRealTime);

		private bool InteractionDelayExpired => TimeUntilInteractive <= 0f;

		public static Dialog_MessageBox CreateConfirmation(TaggedString text, Action confirmedAct, bool destructive = false, string title = null)
		{
			return new Dialog_MessageBox(text, "Confirm".Translate(), confirmedAct, "GoBack".Translate(), null, title, destructive, confirmedAct, delegate
			{
			});
		}

		public Dialog_MessageBox(TaggedString text, string buttonAText = null, Action buttonAAction = null, string buttonBText = null, Action buttonBAction = null, string title = null, bool buttonADestructive = false, Action acceptAction = null, Action cancelAction = null)
		{
			this.text = text;
			this.buttonAText = buttonAText;
			this.buttonAAction = buttonAAction;
			this.buttonADestructive = buttonADestructive;
			this.buttonBText = buttonBText;
			this.buttonBAction = buttonBAction;
			this.title = title;
			this.acceptAction = acceptAction;
			this.cancelAction = cancelAction;
			if (buttonAText.NullOrEmpty())
			{
				this.buttonAText = "OK".Translate();
			}
			forcePause = true;
			absorbInputAroundWindow = true;
			creationRealTime = RealTime.LastRealTime;
			onlyOneOfTypeAllowed = false;
			bool flag = buttonAAction == null && buttonBAction == null && buttonCAction == null;
			forceCatchAcceptAndCancelEventEvenIfUnfocused = acceptAction != null || cancelAction != null || flag;
			closeOnAccept = flag;
			closeOnCancel = flag;
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = inRect.y;
			if (!title.NullOrEmpty())
			{
				Text.Font = GameFont.Medium;
				Widgets.Label(new Rect(0f, num, inRect.width, 42f), title);
				num += 42f;
			}
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect.x, num, inRect.width, inRect.height - 35f - 5f - num);
			float width = outRect.width - 16f;
			Rect viewRect = new Rect(0f, 0f, width, Text.CalcHeight(text, width));
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			Widgets.Label(new Rect(0f, 0f, viewRect.width, viewRect.height), text);
			Widgets.EndScrollView();
			int num2 = (buttonCText.NullOrEmpty() ? 2 : 3);
			float num3 = inRect.width / (float)num2;
			float width2 = num3 - 10f;
			if (buttonADestructive)
			{
				GUI.color = new Color(1f, 0.3f, 0.35f);
			}
			string label = (InteractionDelayExpired ? buttonAText : (buttonAText + "(" + Mathf.Ceil(TimeUntilInteractive).ToString("F0") + ")"));
			if (Widgets.ButtonText(new Rect(num3 * (float)(num2 - 1) + 10f, inRect.height - 35f, width2, 35f), label) && InteractionDelayExpired)
			{
				if (buttonAAction != null)
				{
					buttonAAction();
				}
				Close();
			}
			GUI.color = Color.white;
			if (buttonBText != null && Widgets.ButtonText(new Rect(0f, inRect.height - 35f, width2, 35f), buttonBText))
			{
				if (buttonBAction != null)
				{
					buttonBAction();
				}
				Close();
			}
			if (buttonCText != null && Widgets.ButtonText(new Rect(num3, inRect.height - 35f, width2, 35f), buttonCText))
			{
				if (buttonCAction != null)
				{
					buttonCAction();
				}
				if (buttonCClose)
				{
					Close();
				}
			}
		}

		public override void OnCancelKeyPressed()
		{
			if (cancelAction != null)
			{
				cancelAction();
				Close();
			}
			else
			{
				base.OnCancelKeyPressed();
			}
		}

		public override void OnAcceptKeyPressed()
		{
			if (acceptAction != null)
			{
				acceptAction();
				Close();
			}
			else
			{
				base.OnAcceptKeyPressed();
			}
		}
	}
}
