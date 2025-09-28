// Services/IMenuService.cs
using OSHManagement.Models;

public interface IMenuService
{
    SidebarMenuConfig GetMenuConfig();
}

// Services/MenuService.cs
public class MenuService : IMenuService
{
    public SidebarMenuConfig GetMenuConfig()
    {
        return new SidebarMenuConfig
        {
            Categories = new List<MenuCategory>
            {
                new MenuCategory
                {
                    Name = "Main",
                    Items = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "Dashboards",
                            Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu__icon"" viewBox=""0 0 256 256""><rect width=""256"" height=""256"" fill=""none""></rect><path d=""M133.66,34.34a8,8,0,0,0-11.32,0L40,116.69V216h64V152h48v64h64V116.69Z"" opacity=""0.2""></path><line x1=""16"" y1=""216"" x2=""240"" y2=""216"" fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""16""></line><polyline points=""152 216 152 152 104 152 104 216"" fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""16""></polyline><line x1=""40"" y1=""116.69"" x2=""40"" y2=""216"" fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""16""></line><line x1=""216"" y1=""216"" x2=""216"" y2=""116.69"" fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""16""></line><path d=""M24,132.69l98.34-98.35a8,8,0,0,1,11.32,0L232,132.69"" fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""16""></path></svg>",
                            Url = "javascript:void(0);",
                            IsActive = true,
                            HasSubMenu = true,
                            Children = new List<MenuItem>
                            {
                                new MenuItem
                                {
                                    Label = "Sales",
                                    Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu-doublemenu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                                    Url = "index.html"
                                },
                                new MenuItem
                                {
                                    Label = "Analytics",
                                    Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu-doublemenu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                                    Url = "index-1.html",
                                    IsActive = true
                                }
                            }
                        }
                    }
                },
                new MenuCategory
                {
                    Name = "Web Apps",
                    Items = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "Applications",
                            Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                            Url = "javascript:void(0);",
                            HasSubMenu = true,
                            Children = new List<MenuItem>
                            {
                                new MenuItem
                                {
                                    Label = "Chat",
                                    Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu-doublemenu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                                    Url = "chat.html"
                                },
                                new MenuItem
                                {
                                    Label = "File Manager",
                                    Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu-doublemenu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                                    Url = "file-manager.html"
                                }
                            }
                        }
                    }
                }
            },
            BottomMenuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Theme Settings",
                    Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                    Url = "javascript:void(0);"
                },
                new MenuItem
                {
                    Label = "Profile Settings",
                    Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                    Url = "profile-settings.html"
                },
                new MenuItem
                {
                    Label = "Logout",
                    Icon = @"<svg xmlns=""http://www.w3.org/2000/svg"" class=""side-menu__icon"" viewBox=""0 0 256 256""><!-- SVG content --></svg>",
                    Url = "sign-in-cover.html"
                }
            }
        };
    }
}