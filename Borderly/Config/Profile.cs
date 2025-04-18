namespace Borderly.Config
{
    public class Profile
    {
        public required string Name { get; set; }
        public string? BorderColour { get; set; }
        public int BorderWidth { get; set; }
        public int Quality { get; set; }
        public int? ResizeWidth { get; set; }
        public int? ResizeHeight { get; set; }
        public int? ResizeWidthPercentage { get; set; }
        public int? ResizeHeightPercentage { get; set; }
    }
}
