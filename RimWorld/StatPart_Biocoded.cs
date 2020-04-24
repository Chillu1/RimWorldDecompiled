namespace RimWorld
{
	public class StatPart_Biocoded : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && EquipmentUtility.IsBiocoded(req.Thing))
			{
				val *= 0f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			return null;
		}
	}
}
