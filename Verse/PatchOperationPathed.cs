namespace Verse
{
	public abstract class PatchOperationPathed : PatchOperation
	{
		protected string xpath;

		public override string ToString()
		{
			return $"{base.ToString()}({xpath})";
		}
	}
}
