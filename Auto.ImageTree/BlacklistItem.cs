namespace ImageTree
{
	public class BlacklistItem
	{
		public BlacklistItemType Type { get; set; }
		public string Pattern { get; set; }
	}

	public enum BlacklistItemType
	{
		Unknown = 0,
		DirectoryPath = 1,
		PathContains = 2,
		RegularExpression = 3,
	}
}