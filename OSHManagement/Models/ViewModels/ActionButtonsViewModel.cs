namespace OSHManagement.Models.ViewModels
{
    public class ActionButtonsViewModel
    {
        public bool ShowView { get; set; } = true;
        public string ViewUrl { get; set; } = "";

        public bool ShowEdit { get; set; } = true;
        public string EditUrl { get; set; } = "";

        public bool ShowDelete { get; set; } = true;
        public string DeleteJsFunction { get; set; } = "";

        public List<CustomAction> CustomActions { get; set; } = new List<CustomAction>();
    }

    public class CustomAction
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string IconClass { get; set; } = "ri-more-line";
        public string ColorClass { get; set; } = "secondary";
    }
}
