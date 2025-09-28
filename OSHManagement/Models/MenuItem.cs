namespace OSHManagement.Models
{
    public class MenuItem
    {
        public string Label { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public bool HasSubMenu { get; set; }
        public List<MenuItem> Children { get; set; } = new List<MenuItem>();
    }
}
