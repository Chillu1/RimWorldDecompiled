using Verse;

namespace RimWorld;

public class StatPart_RoomStat : StatPart
{
	private RoomStatDef roomStat;

	[MustTranslate]
	private string customLabel;

	[Unsaved(false)]
	[TranslationHandle(Priority = 100)]
	public string untranslatedCustomLabel;

	public void PostLoad()
	{
		untranslatedCustomLabel = customLabel;
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing)
		{
			Room room = req.Thing.GetRoom();
			if (room != null)
			{
				val *= room.GetStat(roomStat);
			}
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing)
		{
			Room room = req.Thing.GetRoom();
			if (room != null)
			{
				string text = (customLabel.NullOrEmpty() ? ((string)roomStat.LabelCap) : customLabel);
				return text + ": x" + room.GetStat(roomStat).ToStringPercent();
			}
		}
		return null;
	}
}
