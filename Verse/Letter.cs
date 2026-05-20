using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public abstract class Letter : IArchivable, IExposable, ILoadReferenceable
{
	public int ID;

	public LetterDef def;

	private TaggedString label;

	public LookTargets lookTargets;

	public Faction relatedFaction;

	public int arrivalTick;

	public float arrivalTime;

	public string debugInfo;

	public const float DrawWidth = 38f;

	public const float DrawHeight = 30f;

	private const float FallTime = 1f;

	private const float FallDistance = 200f;

	public virtual bool CanShowInLetterStack => true;

	public virtual bool CanDismissWithRightClick => true;

	public bool ArchivedOnly => !Find.LetterStack.LettersListForReading.Contains(this);

	public IThingHolder ParentHolder => Find.World;

	Texture IArchivable.ArchivedIcon => def.Icon;

	Color IArchivable.ArchivedIconColor => def.color;

	string IArchivable.ArchivedLabel => label;

	string IArchivable.ArchivedTooltip => GetMouseoverText();

	int IArchivable.CreatedTicksGame => arrivalTick;

	bool IArchivable.CanCullArchivedNow => !Find.LetterStack.LettersListForReading.Contains(this);

	LookTargets IArchivable.LookTargets => lookTargets;

	public virtual bool ShouldAutomaticallyOpenLetter => false;

	public TaggedString Label
	{
		get
		{
			return label;
		}
		set
		{
			label = value.CapitalizeFirst();
		}
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref ID, "ID", 0);
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref label, "label");
		Scribe_Deep.Look(ref lookTargets, "lookTargets");
		Scribe_References.Look(ref relatedFaction, "relatedFaction");
		Scribe_Values.Look(ref arrivalTick, "arrivalTick", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && relatedFaction != null && !Find.FactionManager.AllFactionsListForReading.Contains(relatedFaction))
		{
			relatedFaction = null;
		}
	}

	public virtual void DrawButtonAt(float topY)
	{
		float num = (float)UI.screenWidth - 38f - 12f;
		Rect rect = new Rect(num, topY, 38f, 30f);
		Rect rect2 = new Rect(rect);
		float num2 = Time.time - arrivalTime;
		Color color = def.color;
		if (num2 < 1f)
		{
			rect2.y -= (1f - num2) * 200f;
			color.a = num2 / 1f;
		}
		if (!Mouse.IsOver(rect) && def.bounce && num2 > 15f && num2 % 5f < 1f)
		{
			float num3 = (float)UI.screenWidth * 0.06f;
			float num4 = 2f * (num2 % 1f) - 1f;
			float num5 = num3 * (1f - num4 * num4);
			rect2.x -= num5;
		}
		if (Event.current.type == EventType.Repaint)
		{
			if (def.flashInterval > 0f)
			{
				float num6 = Time.time - (arrivalTime + 1f);
				if (num6 > 0f && num6 % def.flashInterval < 1f)
				{
					GenUI.DrawFlash(num, topY, (float)UI.screenWidth * 0.6f, Pulser.PulseBrightness(1f, 1f, num6) * 0.55f, def.flashColor);
				}
			}
			GUI.color = color;
			Widgets.DrawShadowAround(rect2);
			GUI.DrawTexture(rect2, def.Icon);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperRight;
			string text = PostProcessedLabel();
			Vector2 vector = Text.CalcSize(text);
			float x = vector.x;
			float y = vector.y;
			Vector2 vector2 = new Vector2(rect2.x + rect2.width / 2f, rect2.center.y - y / 2f + 4f);
			float num7 = vector2.x + x / 2f - (float)(UI.screenWidth - 2);
			if (num7 > 0f)
			{
				vector2.x -= num7;
			}
			GUI.DrawTexture(new Rect(vector2.x - x / 2f - 6f - 1f, vector2.y, x + 12f, 16f), TexUI.GrayTextBG);
			GUI.color = new Color(1f, 1f, 1f, 0.75f);
			Rect rect3 = new Rect(vector2.x - x / 2f, vector2.y - 3f, x, 999f);
			Text.WordWrap = false;
			Widgets.Label(rect3, text);
			Text.WordWrap = true;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}
		if (CanDismissWithRightClick && Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
		{
			SoundDefOf.Click.PlayOneShotOnCamera();
			Find.LetterStack.RemoveLetter(this);
			Event.current.Use();
		}
		if (Widgets.ButtonInvisible(rect2))
		{
			OpenLetter();
			Event.current.Use();
		}
	}

	public virtual void CheckForMouseOverTextAt(float topY)
	{
		float num = (float)UI.screenWidth - 38f - 12f;
		if (!Mouse.IsOver(new Rect(num, topY, 38f, 30f)))
		{
			return;
		}
		Find.LetterStack.Notify_LetterMouseover(this);
		TaggedString mouseoverText = GetMouseoverText();
		if (!mouseoverText.Resolve().NullOrEmpty())
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			float num2 = Text.CalcHeight(mouseoverText, 310f);
			num2 += 20f;
			float x = num - 330f - 10f;
			float y = Mathf.Max(topY - num2 / 2f, 0f);
			Rect infoRect = new Rect(x, y, 330f, num2);
			Find.WindowStack.ImmediateWindow(2768333, infoRect, WindowLayer.Super, delegate
			{
				Text.Font = GameFont.Small;
				Rect rect = infoRect.AtZero().ContractedBy(10f);
				Widgets.BeginGroup(rect);
				Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), mouseoverText.Resolve());
				Widgets.EndGroup();
			});
		}
	}

	protected abstract string GetMouseoverText();

	public abstract void OpenLetter();

	public virtual void Received()
	{
	}

	public virtual void Removed()
	{
	}

	protected virtual string PostProcessedLabel()
	{
		return label;
	}

	void IArchivable.OpenArchived()
	{
		OpenLetter();
	}

	public string GetUniqueLoadID()
	{
		return "Letter_" + ID;
	}
}
