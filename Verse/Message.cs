using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Message : IArchivable, IExposable, ILoadReferenceable
	{
		public MessageTypeDef def;

		private int ID;

		public string text;

		private float startingTime;

		public int startingFrame;

		public int startingTick;

		public LookTargets lookTargets;

		public Quest quest;

		private Vector2 cachedSize = new Vector2(-1f, -1f);

		public Rect lastDrawRect;

		private const float DefaultMessageLifespan = 13f;

		private const float FadeoutDuration = 0.6f;

		protected float Age => RealTime.LastRealTime - startingTime;

		protected float TimeLeft => 13f - Age;

		public bool Expired => TimeLeft <= 0f;

		public float Alpha
		{
			get
			{
				if (TimeLeft < 0.6f)
				{
					return TimeLeft / 0.6f;
				}
				return 1f;
			}
		}

		private static bool ShouldDrawBackground
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					return true;
				}
				WindowStack windowStack = Find.WindowStack;
				for (int i = 0; i < windowStack.Count; i++)
				{
					if (windowStack[i].CausesMessageBackground())
					{
						return true;
					}
				}
				return false;
			}
		}

		Texture IArchivable.ArchivedIcon => null;

		Color IArchivable.ArchivedIconColor => Color.white;

		string IArchivable.ArchivedLabel => text.Flatten();

		string IArchivable.ArchivedTooltip => text;

		int IArchivable.CreatedTicksGame => startingTick;

		bool IArchivable.CanCullArchivedNow => !Messages.IsLive(this);

		LookTargets IArchivable.LookTargets => lookTargets;

		public Message()
		{
		}

		public Message(string text, MessageTypeDef def)
		{
			this.text = text;
			this.def = def;
			startingFrame = RealTime.frameCount;
			startingTime = RealTime.LastRealTime;
			startingTick = GenTicks.TicksGame;
			if (Find.UniqueIDsManager != null)
			{
				ID = Find.UniqueIDsManager.GetNextMessageID();
			}
			else
			{
				ID = Rand.Int;
			}
		}

		public Message(string text, MessageTypeDef def, LookTargets lookTargets)
			: this(text, def)
		{
			this.lookTargets = lookTargets;
		}

		public Message(string text, MessageTypeDef def, LookTargets lookTargets, Quest quest)
			: this(text, def, lookTargets)
		{
			this.quest = quest;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref ID, "ID", 0);
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref startingTime, "startingTime", 0f);
			Scribe_Values.Look(ref startingFrame, "startingFrame", 0);
			Scribe_Values.Look(ref startingTick, "startingTick", 0);
			Scribe_Deep.Look(ref lookTargets, "lookTargets");
			Scribe_References.Look(ref quest, "quest");
		}

		public Rect CalculateRect(float x, float y)
		{
			Text.Font = GameFont.Small;
			if (cachedSize.x < 0f)
			{
				cachedSize = Text.CalcSize(text);
			}
			lastDrawRect = new Rect(x, y, cachedSize.x, cachedSize.y);
			lastDrawRect = lastDrawRect.ContractedBy(-2f);
			return lastDrawRect;
		}

		public void Draw(int xOffset, int yOffset)
		{
			Rect rect = CalculateRect(xOffset, yOffset);
			Find.WindowStack.ImmediateWindow(Gen.HashCombineInt(ID, 45574281), rect, WindowLayer.Super, delegate
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect2 = rect.AtZero();
				float alpha = Alpha;
				GUI.color = new Color(1f, 1f, 1f, alpha);
				if (ShouldDrawBackground)
				{
					GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.8f * alpha);
					GUI.DrawTexture(rect2, BaseContent.WhiteTex);
					GUI.color = new Color(1f, 1f, 1f, alpha);
				}
				if (CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()) || quest != null)
				{
					UIHighlighter.HighlightOpportunity(rect2, "Messages");
					Widgets.DrawHighlightIfMouseover(rect2);
				}
				Widgets.Label(new Rect(2f, 0f, rect2.width - 2f, rect2.height), text);
				if (Current.ProgramState == ProgramState.Playing && Widgets.ButtonInvisible(rect2))
				{
					if (CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()))
					{
						CameraJumper.TryJumpAndSelect(lookTargets.TryGetPrimaryTarget());
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ClickingMessages, KnowledgeAmount.Total);
					}
					else if (quest != null)
					{
						if (Find.MainTabsRoot.OpenTab == MainButtonDefOf.Quests)
						{
							SoundDefOf.Click.PlayOneShotOnCamera();
						}
						else
						{
							Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
						}
						((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
					}
				}
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				if (Mouse.IsOver(rect2))
				{
					Messages.Notify_Mouseover(this);
				}
			}, doBackground: false, absorbInputAroundWindow: false, 0f);
		}

		void IArchivable.OpenArchived()
		{
			Find.WindowStack.Add(new Dialog_MessageBox(text));
		}

		public string GetUniqueLoadID()
		{
			return "Message_" + ID;
		}
	}
}
