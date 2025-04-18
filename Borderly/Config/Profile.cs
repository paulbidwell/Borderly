namespace Borderly.Config
{
    public class Profile
    {
        public required string Name { get; set; }
        public string? BorderColour { get; set; }
        public required string BorderWidth { get; set; }
        public int Quality { get; set; }
        public string? ResizeWidth { get; set; }
        public string? ResizeHeight { get; set; }
    }
}