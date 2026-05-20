using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public class TradeRequestComp : WorldObjectComp
{
	public ThingDef requestThingDef;

	public int requestCount;

	public int expiration = -1;

	public string outSignalFulfilled;

	private static readonly Texture2D TradeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/FulfillTradeRequest");

	public bool ActiveRequest => expiration > Find.TickManager.TicksGame;

	public override string CompInspectStringExtra()
	{
		if (ActiveRequest)
		{
			return "CaravanRequestInfo".Translate(TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount).CapitalizeFirst(), (expiration - Find.TickManager.TicksGame).ToStringTicksToDays(), (requestThingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)requestCount).ToStringMoney());
		}
		return null;
	}

	public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
	{
		if (ActiveRequest && CaravanVisitUtility.SettlementVisitedNow(caravan) == parent)
		{
			yield return FulfillRequestCommand(caravan);
		}
	}

	public void Disable()
	{
		expiration = -1;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Defs.Look(ref requestThingDef, "requestThingDef");
		Scribe_Values.Look(ref requestCount, "requestCount", 0);
		Scribe_Values.Look(ref expiration, "expiration", 0);
		BackCompatibility.PostExposeData(this);
	}

	private Command FulfillRequestCommand(Caravan caravan)
	{
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandFulfillTradeOffer".Translate();
		command_Action.defaultDesc = "CommandFulfillTradeOfferDesc".Translate();
		command_Action.icon = TradeCommandTex;
		command_Action.action = delegate
		{
			if (!ActiveRequest)
			{
				Log.Error("Attempted to fulfill an unavailable request");
			}
			else if (!CaravanInventoryUtility.HasThings(caravan, requestThingDef, requestCount, PlayerCanGive))
			{
				Messages.Message("CommandFulfillTradeOfferFailInsufficient".Translate(TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount)), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CommandFulfillTradeOfferConfirm".Translate(GenLabel.ThingLabel(requestThingDef, null, requestCount)), delegate
				{
					Fulfill(caravan);
				}));
			}
		};
		if (!CaravanInventoryUtility.HasThings(caravan, requestThingDef, requestCount, PlayerCanGive))
		{
			command_Action.Disable("CommandFulfillTradeOfferFailInsufficient".Translate(TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount)));
		}
		return command_Action;
	}

	private void Fulfill(Caravan caravan)
	{
		int remaining = requestCount;
		List<Thing> list = CaravanInventoryUtility.TakeThings(caravan, delegate(Thing thing)
		{
			if (requestThingDef != thing.def)
			{
				return 0;
			}
			if (!PlayerCanGive(thing))
			{
				return 0;
			}
			int num2 = Mathf.Min(remaining, thing.stackCount);
			remaining -= num2;
			return num2;
		});
		for (int num = 0; num < list.Count; num++)
		{
			list[num].Destroy();
		}
		if (parent.Faction != null)
		{
			Faction.OfPlayer.TryAffectGoodwillWith(parent.Faction, 12, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.QuestGoodwillReward);
		}
		QuestUtility.SendQuestTargetSignals(parent.questTags, "TradeRequestFulfilled", parent.Named("SUBJECT"), caravan.Named("CARAVAN"));
		Disable();
	}

	private bool PlayerCanGive(Thing thing)
	{
		if (thing.GetRotStage() != RotStage.Fresh)
		{
			return false;
		}
		if (thing is Apparel { WornByCorpse: not false })
		{
			return false;
		}
		CompQuality compQuality = thing.TryGetComp<CompQuality>();
		if (compQuality != null && (int)compQuality.Quality < 2)
		{
			return false;
		}
		return true;
	}
}
