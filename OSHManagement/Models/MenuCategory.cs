namespace OSHManagement.Models
{
    public class MenuCategory
    {
        public string Name { get; set; }
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }
}
