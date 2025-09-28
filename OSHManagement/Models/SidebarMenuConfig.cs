namespace OSHManagement.Models
{
    public class SidebarMenuConfig
    {
        public List<MenuCategory> Categories { get; set; } = new List<MenuCategory>();
        public List<MenuItem> BottomMenuItems { get; set; } = new List<MenuItem>();
    }
}
