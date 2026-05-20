using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public abstract class Bill : IExposable, ILoadReferenceable
{
	[Unsaved(false)]
	public BillStack billStack;

	private int loadID = -1;

	public RecipeDef recipe;

	public Precept_ThingStyle precept;

	public ThingStyleDef style;

	public bool globalStyle = true;

	public int? graphicIndexOverride;

	public Xenogerm xenogerm;

	public bool suspended;

	public ThingFilter ingredientFilter;

	public float ingredientSearchRadius = 999f;

	public IntRange allowedSkillRange = new IntRange(0, 20);

	private Pawn pawnRestriction;

	private bool slavesOnly;

	private bool mechsOnly;

	private bool nonMechsOnly;

	public bool deleted;

	public int nextTickToSearchForIngredients;

	public const int MaxIngredientSearchRadius = 999;

	public const float ButSize = 24f;

	private const float InterfaceBaseHeight = 53f;

	private const float InterfaceStatusLineHeight = 17f;

	public Map Map
	{
		get
		{
			object obj = billStack?.billGiver?.Map;
			if (obj == null)
			{
				Thing obj2 = billStack?.billGiver as Thing;
				if (obj2 == null)
				{
					return null;
				}
				obj = obj2.MapHeld;
			}
			return (Map)obj;
		}
	}

	public virtual string Label
	{
		get
		{
			if (precept == null)
			{
				return recipe.label;
			}
			return GenText.UncapitalizeFirst("RecipeMake".Translate(precept.LabelCap));
		}
	}

	public virtual string LabelCap
	{
		get
		{
			if (precept == null)
			{
				return Label.CapitalizeFirst(recipe);
			}
			return GenText.CapitalizeFirst("RecipeMake".Translate(precept.LabelCap));
		}
	}

	public virtual bool CheckIngredientsIfSociallyProper => true;

	public virtual bool CompletableEver => true;

	protected virtual string StatusString => null;

	protected virtual float StatusLineMinHeight => 0f;

	protected virtual bool CanCopy => true;

	public virtual bool CanFinishNow => true;

	public bool DeletedOrDereferenced
	{
		get
		{
			if (deleted)
			{
				return true;
			}
			if (billStack.billGiver is Thing { Destroyed: not false })
			{
				return true;
			}
			return false;
		}
	}

	public Pawn PawnRestriction => pawnRestriction;

	public bool SlavesOnly => slavesOnly;

	public bool MechsOnly => mechsOnly;

	public bool NonMechsOnly => nonMechsOnly;

	protected virtual Color BaseColor
	{
		get
		{
			if (ShouldDoNow())
			{
				return Color.white;
			}
			return new Color(1f, 0.7f, 0.7f, 0.7f);
		}
	}

	public Bill()
	{
	}

	public Bill(RecipeDef recipe, Precept_ThingStyle precept = null)
	{
		this.recipe = recipe;
		this.precept = precept;
		ingredientFilter = new ThingFilter();
		ingredientFilter.CopyAllowancesFrom(recipe.defaultIngredientFilter);
		InitializeAfterClone();
	}

	public void InitializeAfterClone()
	{
		loadID = Find.UniqueIDsManager.GetNextBillID();
	}

	public void SetPawnRestriction(Pawn pawn)
	{
		pawnRestriction = pawn;
		slavesOnly = false;
		mechsOnly = false;
		nonMechsOnly = false;
	}

	public void SetAnySlaveRestriction()
	{
		pawnRestriction = null;
		slavesOnly = true;
		mechsOnly = false;
		nonMechsOnly = false;
	}

	public void SetAnyPawnRestriction()
	{
		slavesOnly = false;
		pawnRestriction = null;
		mechsOnly = false;
		nonMechsOnly = false;
	}

	public void SetAnyMechRestriction()
	{
		slavesOnly = false;
		pawnRestriction = null;
		mechsOnly = true;
		nonMechsOnly = false;
	}

	public void SetAnyNonMechRestriction()
	{
		slavesOnly = false;
		pawnRestriction = null;
		mechsOnly = false;
		nonMechsOnly = true;
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Defs.Look(ref recipe, "recipe");
		Scribe_Values.Look(ref suspended, "suspended", defaultValue: false);
		Scribe_Values.Look(ref ingredientSearchRadius, "ingredientSearchRadius", 999f);
		Scribe_Values.Look(ref allowedSkillRange, "allowedSkillRange");
		Scribe_References.Look(ref pawnRestriction, "pawnRestriction");
		Scribe_References.Look(ref precept, "precept");
		Scribe_References.Look(ref xenogerm, "xenogerm");
		Scribe_Values.Look(ref slavesOnly, "slavesOnly", defaultValue: false);
		Scribe_Values.Look(ref mechsOnly, "mechsOnly", defaultValue: false);
		Scribe_Values.Look(ref nonMechsOnly, "nonMechsOnly", defaultValue: false);
		Scribe_Defs.Look(ref style, "style");
		Scribe_Values.Look(ref globalStyle, "globalStyle", defaultValue: true);
		Scribe_Values.Look(ref graphicIndexOverride, "graphicIndexOverride");
		if (Scribe.mode == LoadSaveMode.Saving && recipe.fixedIngredientFilter != null)
		{
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (!recipe.fixedIngredientFilter.Allows(allDef))
				{
					ingredientFilter.SetAllow(allDef, allow: false);
				}
			}
		}
		Scribe_Deep.Look(ref ingredientFilter, "ingredientFilter");
	}

	public virtual bool PawnAllowedToStartAnew(Pawn p)
	{
		if (pawnRestriction != null)
		{
			return pawnRestriction == p;
		}
		if (slavesOnly && !p.IsSlave)
		{
			return false;
		}
		if (mechsOnly && !p.IsColonyMechPlayerControlled)
		{
			return false;
		}
		if (nonMechsOnly && p.IsColonyMechPlayerControlled)
		{
			return false;
		}
		if (recipe.workSkill != null && (p.skills != null || p.IsColonyMech))
		{
			int num = ((p.skills != null) ? p.skills.GetSkill(recipe.workSkill).Level : p.RaceProps.mechFixedSkillLevel);
			if (num < allowedSkillRange.min)
			{
				JobFailReason.Is("UnderAllowedSkill".Translate(allowedSkillRange.min), Label);
				return false;
			}
			if (num > allowedSkillRange.max)
			{
				JobFailReason.Is("AboveAllowedSkill".Translate(allowedSkillRange.max), Label);
				return false;
			}
		}
		if (ModsConfig.BiotechActive && recipe.mechanitorOnlyRecipe && !MechanitorUtility.IsMechanitor(p))
		{
			JobFailReason.Is("NotAMechanitor".Translate());
			return false;
		}
		return true;
	}

	public virtual void Notify_PawnDidWork(Pawn p)
	{
	}

	public virtual void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
	{
	}

	public abstract bool ShouldDoNow();

	public virtual void Notify_DoBillStarted(Pawn billDoer)
	{
	}

	public virtual void Notify_BillWorkStarted(Pawn billDoer)
	{
	}

	public virtual void Notify_BillWorkFinished(Pawn billDoer)
	{
	}

	protected virtual void DoConfigInterface(Rect rect, Color baseColor)
	{
		rect.yMin += 29f;
		float y = rect.center.y;
		Widgets.InfoCardButton(rect.xMax - (rect.yMax - y) - 12f, y - 12f, recipe);
	}

	public virtual void DoStatusLineInterface(Rect rect)
	{
	}

	public Rect DoInterface(float x, float y, float width, int index)
	{
		Rect rect = new Rect(x, y, width, 53f);
		float num = 0f;
		if (!StatusString.NullOrEmpty())
		{
			num = Mathf.Max(Text.TinyFontSupported ? 17f : 21f, StatusLineMinHeight);
		}
		rect.height += num;
		Color color = (GUI.color = BaseColor);
		Text.Font = GameFont.Small;
		if (index % 2 == 0)
		{
			Widgets.DrawAltRect(rect);
		}
		Widgets.BeginGroup(rect);
		Rect rect2 = new Rect(0f, 0f, 24f, 24f);
		if (billStack.IndexOf(this) > 0)
		{
			if (Widgets.ButtonImage(rect2, TexButton.ReorderUp, color))
			{
				billStack.Reorder(this, -1);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegionByKey(rect2, "ReorderBillUpTip");
		}
		if (billStack.IndexOf(this) < billStack.Count - 1)
		{
			Rect rect3 = new Rect(0f, 24f, 24f, 24f);
			if (Widgets.ButtonImage(rect3, TexButton.ReorderDown, color))
			{
				billStack.Reorder(this, 1);
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegionByKey(rect3, "ReorderBillDownTip");
		}
		GUI.color = color;
		Widgets.Label(new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f), LabelCap);
		DoConfigInterface(rect.AtZero(), color);
		Rect rect4 = new Rect(rect.width - 24f, 0f, 24f, 24f);
		if (Widgets.ButtonImage(rect4, TexButton.Delete, color, color * GenUI.SubtleMouseoverColor))
		{
			billStack.Delete(this);
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		TooltipHandler.TipRegionByKey(rect4, "DeleteBillTip");
		Rect rect6;
		if (CanCopy)
		{
			Rect rect5 = new Rect(rect4);
			rect5.x -= rect5.width + 4f;
			if (Widgets.ButtonImageFitted(rect5, TexButton.Copy, color))
			{
				BillUtility.Clipboard = Clone();
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegionByKey(rect5, "CopyBillTip");
			rect6 = new Rect(rect5);
		}
		else
		{
			rect6 = new Rect(rect4);
		}
		rect6.x -= rect6.width;
		if (Widgets.ButtonImage(rect6, TexButton.Suspend, color))
		{
			suspended = !suspended;
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		TooltipHandler.TipRegionByKey(rect6, "SuspendBillTip");
		if (!StatusString.NullOrEmpty())
		{
			Text.Font = GameFont.Tiny;
			Rect rect7 = new Rect(24f, rect.height - num, rect.width - 24f, num);
			Widgets.Label(rect7, StatusString);
			DoStatusLineInterface(rect7);
		}
		Widgets.EndGroup();
		if (suspended)
		{
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Rect rect8 = new Rect(rect.x + rect.width / 2f - 70f, rect.y + rect.height / 2f - 20f, 140f, 40f);
			GUI.DrawTexture(rect8, TexUI.GrayTextBG);
			Widgets.Label(rect8, "SuspendedCaps".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}
		Text.Font = GameFont.Small;
		GUI.color = Color.white;
		return rect;
	}

	public virtual bool IsFixedOrAllowedIngredient(Thing thing)
	{
		for (int i = 0; i < recipe.ingredients.Count; i++)
		{
			IngredientCount ingredientCount = recipe.ingredients[i];
			if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(thing))
			{
				return true;
			}
		}
		if (recipe.fixedIngredientFilter.Allows(thing))
		{
			return ingredientFilter.Allows(thing);
		}
		return false;
	}

	public bool IsFixedOrAllowedIngredient(ThingDef def)
	{
		for (int i = 0; i < recipe.ingredients.Count; i++)
		{
			IngredientCount ingredientCount = recipe.ingredients[i];
			if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(def))
			{
				return true;
			}
		}
		if (recipe.fixedIngredientFilter.Allows(def))
		{
			return ingredientFilter.Allows(def);
		}
		return false;
	}

	public static void CreateNoPawnsWithSkillDialog(RecipeDef recipe)
	{
		string text = "RecipeRequiresSkills".Translate(recipe.LabelCap);
		text += "\n\n";
		text += recipe.MinSkillString;
		Find.WindowStack.Add(new Dialog_MessageBox(text));
	}

	public virtual BillStoreModeDef GetStoreMode()
	{
		return BillStoreModeDefOf.BestStockpile;
	}

	public virtual ISlotGroup GetSlotGroup()
	{
		return null;
	}

	public virtual void SetStoreMode(BillStoreModeDef mode, ISlotGroup group = null)
	{
		Log.ErrorOnce("Tried to set store mode of a non-production bill", 27190980);
	}

	public virtual float GetWorkAmount(Thing thing = null)
	{
		return recipe.WorkAmountTotal(thing);
	}

	public virtual Bill Clone()
	{
		Bill obj = (Bill)Activator.CreateInstance(GetType());
		obj.recipe = recipe;
		obj.precept = precept;
		obj.style = style;
		obj.globalStyle = globalStyle;
		obj.suspended = suspended;
		obj.ingredientFilter = new ThingFilter();
		obj.ingredientFilter.CopyAllowancesFrom(ingredientFilter);
		obj.ingredientSearchRadius = ingredientSearchRadius;
		obj.allowedSkillRange = allowedSkillRange;
		obj.pawnRestriction = pawnRestriction;
		obj.slavesOnly = slavesOnly;
		obj.xenogerm = xenogerm;
		obj.mechsOnly = mechsOnly;
		obj.nonMechsOnly = nonMechsOnly;
		return obj;
	}

	public virtual void ValidateSettings()
	{
		if (pawnRestriction != null && (pawnRestriction.Dead || pawnRestriction.Faction != Faction.OfPlayer || pawnRestriction.IsKidnapped()))
		{
			if (this != BillUtility.Clipboard)
			{
				Messages.Message("MessageBillValidationPawnUnavailable".Translate(pawnRestriction.LabelShortCap, Label, billStack.billGiver.LabelShort), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
			}
			pawnRestriction = null;
		}
	}

	public string GetUniqueLoadID()
	{
		return "Bill_" + recipe.defName + "_" + loadID;
	}

	public override string ToString()
	{
		return GetUniqueLoadID();
	}
}
