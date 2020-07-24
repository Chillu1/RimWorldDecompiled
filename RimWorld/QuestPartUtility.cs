using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class QuestPartUtility
	{
		public const int RewardStackElementIconSize = 22;

		public const int RewardStackElementMarginHorizontal = 6;

		public const int RewardStackElementMarginVertical = 1;

		public const int RewardStackElementIconGap = 2;

		private static List<Pair<Thing, int>> tmpThings = new List<Pair<Thing, int>>();

		private static List<Reward_Items.RememberedItem> tmpThingDefs = new List<Reward_Items.RememberedItem>();

		public static T MakeAndAddEndCondition<T>(Quest quest, string inSignalActivate, QuestEndOutcome outcome, Letter letter = null) where T : QuestPartActivable, new()
		{
			T val = new T();
			val.inSignalEnable = inSignalActivate;
			quest.AddPart(val);
			if (letter != null)
			{
				QuestPart_Letter questPart_Letter = new QuestPart_Letter();
				questPart_Letter.letter = letter;
				questPart_Letter.inSignal = val.OutSignalCompleted;
				quest.AddPart(questPart_Letter);
			}
			QuestPart_QuestEnd questPart_QuestEnd = new QuestPart_QuestEnd();
			questPart_QuestEnd.inSignal = val.OutSignalCompleted;
			questPart_QuestEnd.outcome = outcome;
			quest.AddPart(questPart_QuestEnd);
			return val;
		}

		public static QuestPart_QuestEnd MakeAndAddEndNodeWithLetter(Quest quest, string inSignalActivate, QuestEndOutcome outcome, Letter letter)
		{
			QuestPart_Letter questPart_Letter = new QuestPart_Letter();
			questPart_Letter.letter = letter;
			questPart_Letter.inSignal = inSignalActivate;
			quest.AddPart(questPart_Letter);
			QuestPart_QuestEnd questPart_QuestEnd = new QuestPart_QuestEnd();
			questPart_QuestEnd.inSignal = inSignalActivate;
			questPart_QuestEnd.outcome = outcome;
			quest.AddPart(questPart_QuestEnd);
			return questPart_QuestEnd;
		}

		public static QuestPart_Delay MakeAndAddQuestTimeoutDelay(Quest quest, int delayTicks, WorldObject worldObject)
		{
			QuestPart_WorldObjectTimeout questPart_WorldObjectTimeout = new QuestPart_WorldObjectTimeout();
			questPart_WorldObjectTimeout.delayTicks = delayTicks;
			questPart_WorldObjectTimeout.expiryInfoPart = "QuestExpiresIn".Translate();
			questPart_WorldObjectTimeout.expiryInfoPartTip = "QuestExpiresOn".Translate();
			questPart_WorldObjectTimeout.isBad = true;
			questPart_WorldObjectTimeout.outcomeCompletedSignalArg = QuestEndOutcome.Fail;
			questPart_WorldObjectTimeout.inSignalEnable = quest.InitiateSignal;
			quest.AddPart(questPart_WorldObjectTimeout);
			string text = "Quest" + quest.id + ".DelayingWorldObject";
			QuestUtility.AddQuestTag(ref worldObject.questTags, text);
			questPart_WorldObjectTimeout.inSignalDisable = text + ".MapGenerated";
			QuestPart_QuestEnd questPart_QuestEnd = new QuestPart_QuestEnd();
			questPart_QuestEnd.inSignal = questPart_WorldObjectTimeout.OutSignalCompleted;
			quest.AddPart(questPart_QuestEnd);
			return questPart_WorldObjectTimeout;
		}

		public static IEnumerable<GenUI.AnonymousStackElement> GetRewardStackElementsForThings(IEnumerable<Thing> things, bool detailsHidden = false)
		{
			_003C_003Ec__DisplayClass8_0 _003C_003Ec__DisplayClass8_ = new _003C_003Ec__DisplayClass8_0();
			_003C_003Ec__DisplayClass8_.detailsHidden = detailsHidden;
			tmpThings.Clear();
			foreach (Thing thing2 in things)
			{
				bool flag = false;
				for (int j = 0; j < tmpThings.Count; j++)
				{
					if (tmpThings[j].First.CanStackWith(thing2))
					{
						tmpThings[j] = new Pair<Thing, int>(tmpThings[j].First, tmpThings[j].Second + thing2.stackCount);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					tmpThings.Add(new Pair<Thing, int>(thing2, thing2.stackCount));
				}
			}
			for (int i = 0; i < tmpThings.Count; i++)
			{
				_003C_003Ec__DisplayClass8_0 _003C_003Ec__DisplayClass8_2 = _003C_003Ec__DisplayClass8_;
				Thing thing = tmpThings[i].First.GetInnerIfMinified();
				int second = tmpThings[i].Second;
				Pawn value;
				string label;
				if ((value = (thing as Pawn)) != null)
				{
					label = "PawnQuestReward".Translate(value);
				}
				else
				{
					label = thing.LabelCapNoCount + ((second > 1) ? (" x" + second) : "");
				}
				yield return new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect rect)
					{
						Widgets.DrawHighlight(rect);
						Rect rect2 = new Rect(rect.x + 6f, rect.y + 1f, rect.width - 12f, rect.height - 2f);
						if (Mouse.IsOver(rect))
						{
							Widgets.DrawHighlight(rect);
							TaggedString t = _003C_003Ec__DisplayClass8_2.detailsHidden ? "NoMoreInfoAvailable".Translate() : "ClickForMoreInfo".Translate();
							Pawn pawn;
							if ((pawn = (thing as Pawn)) != null && pawn.RaceProps.Humanlike)
							{
								TooltipHandler.TipRegion(rect, pawn.LabelCap + "\n\n" + t);
							}
							else
							{
								TooltipHandler.TipRegion(rect, thing.DescriptionDetailed + "\n\n" + t);
							}
						}
						Widgets.ThingIcon(new Rect(rect2.x, rect2.y, 22f, 22f), thing);
						Rect rect3 = rect2;
						rect3.xMin += 24f;
						Widgets.Label(rect3, label);
						if (Widgets.ButtonInvisible(rect))
						{
							if (_003C_003Ec__DisplayClass8_2.detailsHidden)
							{
								Messages.Message("NoMoreInfoAvailable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
							}
							else
							{
								Find.WindowStack.Add(new Dialog_InfoCard(thing));
							}
						}
					},
					width = Text.CalcSize(label).x + 12f + 22f + 2f
				};
			}
		}

		public static IEnumerable<GenUI.AnonymousStackElement> GetRewardStackElementsForThings(IEnumerable<Reward_Items.RememberedItem> thingDefs)
		{
			tmpThingDefs.Clear();
			foreach (Reward_Items.RememberedItem thingDef in thingDefs)
			{
				bool flag = false;
				for (int j = 0; j < tmpThingDefs.Count; j++)
				{
					if (tmpThingDefs[j].thing == thingDef.thing && tmpThingDefs[j].label == thingDef.label)
					{
						tmpThingDefs[j] = new Reward_Items.RememberedItem(tmpThingDefs[j].thing, tmpThingDefs[j].stackCount + thingDef.stackCount, tmpThingDefs[j].label);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					tmpThingDefs.Add(thingDef);
				}
			}
			for (int i = 0; i < tmpThingDefs.Count; i++)
			{
				ThingStuffPairWithQuality thing = tmpThingDefs[i].thing;
				int stackCount = tmpThingDefs[i].stackCount;
				string label = tmpThingDefs[i].label.CapitalizeFirst() + ((stackCount > 1) ? (" x" + stackCount) : "");
				yield return new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect rect)
					{
						Widgets.DrawHighlight(rect);
						Rect rect2 = new Rect(rect.x + 6f, rect.y + 1f, rect.width - 12f, rect.height - 2f);
						if (Mouse.IsOver(rect))
						{
							Widgets.DrawHighlight(rect);
							TooltipHandler.TipRegion(rect, thing.thing.DescriptionDetailed + "\n\n" + "ClickForMoreInfo".Translate());
						}
						Widgets.ThingIcon(new Rect(rect2.x, rect2.y, 22f, 22f), thing.thing, thing.stuff);
						Rect rect3 = rect2;
						rect3.xMin += 24f;
						Widgets.Label(rect3, label);
						if (Widgets.ButtonInvisible(rect))
						{
							Find.WindowStack.Add(new Dialog_InfoCard(thing.thing, thing.stuff));
						}
					},
					width = Text.CalcSize(label).x + 12f + 22f + 2f
				};
			}
		}

		public static GenUI.AnonymousStackElement GetStandardRewardStackElement(string label, Texture2D icon, Func<string> tipGetter, Action onClick = null)
		{
			return GetStandardRewardStackElement(label, delegate(Rect r)
			{
				GUI.DrawTexture(r, icon);
			}, tipGetter, onClick);
		}

		public static GenUI.AnonymousStackElement GetStandardRewardStackElement(string label, Action<Rect> iconDrawer, Func<string> tipGetter, Action onClick = null)
		{
			return new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect rect)
				{
					Widgets.DrawHighlight(rect);
					Rect rect2 = new Rect(rect.x + 6f, rect.y + 1f, rect.width - 12f, rect.height - 2f);
					if (Mouse.IsOver(rect))
					{
						Widgets.DrawHighlight(rect);
						if (tipGetter != null)
						{
							TooltipHandler.TipRegion(rect, new TipSignal(tipGetter, 0x56AAA7E ^ (int)rect.x ^ (int)rect.y));
						}
					}
					Rect obj = new Rect(rect2.x, rect2.y, 22f, 22f);
					iconDrawer(obj);
					Rect rect3 = rect2;
					rect3.xMin += 24f;
					Widgets.Label(rect3, label);
					if (onClick != null && Widgets.ButtonInvisible(rect))
					{
						onClick();
					}
				},
				width = Text.CalcSize(label).x + 12f + 22f + 2f
			};
		}
	}
}
