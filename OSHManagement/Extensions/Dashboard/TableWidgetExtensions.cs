using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Extensions.Dashboard
{
    /// <summary>
    /// Extension methods for building Table Widget components
    /// ALL LOGIC - NO RENDERING
    /// </summary>
    public static class TableWidgetExtensions
    {
        /// <summary>
        /// Build a standard table widget
        /// </summary>
        public static TableWidgetViewModel BuildTableWidget(
            string title,
            List<TableColumnViewModel> columns,
            List<TableRowViewModel> rows,
            string? viewAllUrl = null,
            string? viewAllText = "View All",
            bool striped = true,
            bool hoverable = true,
            int maxRows = 10)
        {
            return new TableWidgetViewModel
            {
                Title = title,
                Columns = columns,
                Rows = rows,
                ViewAllUrl = viewAllUrl,
                ViewAllText = viewAllText,
                Striped = striped,
                Hoverable = hoverable,
                MaxRows = maxRows,
                WidgetType = TableWidgetType.Standard
            };
        }

        /// <summary>
        /// Build a compact table widget (minimal spacing)
        /// </summary>
        public static TableWidgetViewModel BuildCompactTableWidget(
            string title,
            List<TableColumnViewModel> columns,
            List<TableRowViewModel> rows,
            string? viewAllUrl = null,
            int maxRows = 15)
        {
            return new TableWidgetViewModel
            {
                Title = title,
                Columns = columns,
                Rows = rows,
                ViewAllUrl = viewAllUrl,
                Striped = false,
                Compact = true,
                MaxRows = maxRows,
                WidgetType = TableWidgetType.Compact
            };
        }

        /// <summary>
        /// Build a table column
        /// </summary>
        public static TableColumnViewModel BuildColumn(
            string header,
            string propertyName,
            ColumnType type = ColumnType.Text,
            string? cssClass = null,
            bool sortable = false,
            int? width = null)
        {
            return new TableColumnViewModel
            {
                Header = header,
                PropertyName = propertyName,
                Type = type,
                CssClass = cssClass,
                Sortable = sortable,
                Width = width
            };
        }

        /// <summary>
        /// Build a table row
        /// </summary>
        public static TableRowViewModel BuildRow(
            Dictionary<string, object> data,
            string? linkUrl = null,
            string? rowCssClass = null,
            string? id = null)
        {
            return new TableRowViewModel
            {
                Data = data,
                LinkUrl = linkUrl,
                RowCssClass = rowCssClass,
                Id = id
            };
        }

        /// <summary>
        /// Build recent incidents table (common use case)
        /// </summary>
        public static TableWidgetViewModel BuildRecentIncidentsTable(
            List<(int Id, string Title, string Location, string Severity, DateTime Date, string ReportedBy)> incidents,
            int maxRows = 10)
        {
            var columns = new List<TableColumnViewModel>
            {
                BuildColumn("Incident", "Title", ColumnType.Text),
                BuildColumn("Location", "Location", ColumnType.Text),
                BuildColumn("Severity", "Severity", ColumnType.Badge),
                BuildColumn("Date", "Date", ColumnType.Date),
                BuildColumn("Reported By", "ReportedBy", ColumnType.Text)
            };

            var rows = incidents.Take(maxRows).Select(i => BuildRow(
                data: new Dictionary<string, object>
                {
                    { "Title", i.Title },
                    { "Location", i.Location },
                    { "Severity", new TableBadgeData 
                        { 
                            Text = i.Severity, 
                            ColorClass = GetSeverityColor(i.Severity) 
                        } 
                    },
                    { "Date", i.Date },
                    { "ReportedBy", i.ReportedBy }
                },
                linkUrl: $"/Incident/Details/{i.Id}",
                id: i.Id.ToString()
            )).ToList();

            return BuildTableWidget(
                title: "Recent Incidents",
                columns: columns,
                rows: rows,
                viewAllUrl: "/Incident/Index",
                maxRows: maxRows
            );
        }

        /// <summary>
        /// Build actions table (common use case)
        /// </summary>
        public static TableWidgetViewModel BuildActionsTable(
            List<(int Id, string Title, string AssignedTo, string Status, DateTime DueDate, string Priority)> actions,
            int maxRows = 10)
        {
            var columns = new List<TableColumnViewModel>
            {
                BuildColumn("Action", "Title", ColumnType.Text),
                BuildColumn("Assigned To", "AssignedTo", ColumnType.Text),
                BuildColumn("Status", "Status", ColumnType.Badge),
                BuildColumn("Priority", "Priority", ColumnType.Badge),
                BuildColumn("Due Date", "DueDate", ColumnType.Date)
            };

            var rows = actions.Take(maxRows).Select(a => BuildRow(
                data: new Dictionary<string, object>
                {
                    { "Title", a.Title },
                    { "AssignedTo", a.AssignedTo },
                    { "Status", new TableBadgeData 
                        { 
                            Text = a.Status, 
                            ColorClass = GetStatusColor(a.Status) 
                        } 
                    },
                    { "Priority", new TableBadgeData 
                        { 
                            Text = a.Priority, 
                            ColorClass = GetPriorityColor(a.Priority) 
                        } 
                    },
                    { "DueDate", a.DueDate }
                },
                linkUrl: $"/Action/Details/{a.Id}",
                id: a.Id.ToString()
            )).ToList();

            return BuildTableWidget(
                title: "Recent Actions",
                columns: columns,
                rows: rows,
                viewAllUrl: "/Action/Index",
                maxRows: maxRows
            );
        }

        /// <summary>
        /// Build equipment table (common use case)
        /// </summary>
        public static TableWidgetViewModel BuildEquipmentTable(
            List<(int Id, string Name, string Type, string Status, DateTime LastInspection, string Condition)> equipment,
            int maxRows = 10)
        {
            var columns = new List<TableColumnViewModel>
            {
                BuildColumn("Equipment", "Name", ColumnType.Text),
                BuildColumn("Type", "Type", ColumnType.Text),
                BuildColumn("Status", "Status", ColumnType.Badge),
                BuildColumn("Condition", "Condition", ColumnType.Badge),
                BuildColumn("Last Inspection", "LastInspection", ColumnType.Date)
            };

            var rows = equipment.Take(maxRows).Select(e => BuildRow(
                data: new Dictionary<string, object>
                {
                    { "Name", e.Name },
                    { "Type", e.Type },
                    { "Status", new TableBadgeData 
                        { 
                            Text = e.Status, 
                            ColorClass = GetStatusColor(e.Status) 
                        } 
                    },
                    { "Condition", new TableBadgeData 
                        { 
                            Text = e.Condition, 
                            ColorClass = GetConditionColor(e.Condition) 
                        } 
                    },
                    { "LastInspection", e.LastInspection }
                },
                linkUrl: $"/Equipment/Details/{e.Id}",
                id: e.Id.ToString()
            )).ToList();

            return BuildTableWidget(
                title: "Equipment Status",
                columns: columns,
                rows: rows,
                viewAllUrl: "/Equipment/Index",
                maxRows: maxRows
            );
        }

        /// <summary>
        /// Build training compliance table (common use case)
        /// </summary>
        public static TableWidgetViewModel BuildTrainingComplianceTable(
            List<(int Id, string EmployeeName, string Department, int CompletedCourses, int TotalCourses, decimal ComplianceRate)> training,
            int maxRows = 10)
        {
            var columns = new List<TableColumnViewModel>
            {
                BuildColumn("Employee", "EmployeeName", ColumnType.Text),
                BuildColumn("Department", "Department", ColumnType.Text),
                BuildColumn("Completed", "Completed", ColumnType.Text, cssClass: "text-center"),
                BuildColumn("Compliance", "Compliance", ColumnType.Percentage, cssClass: "text-end"),
                BuildColumn("Status", "Status", ColumnType.Badge)
            };

            var rows = training.Take(maxRows).Select(t => BuildRow(
                data: new Dictionary<string, object>
                {
                    { "EmployeeName", t.EmployeeName },
                    { "Department", t.Department },
                    { "Completed", $"{t.CompletedCourses}/{t.TotalCourses}" },
                    { "Compliance", t.ComplianceRate },
                    { "Status", new TableBadgeData 
                        { 
                            Text = GetComplianceStatus(t.ComplianceRate), 
                            ColorClass = GetComplianceColor(t.ComplianceRate) 
                        } 
                    }
                },
                linkUrl: $"/Employee/Details/{t.Id}",
                id: t.Id.ToString()
            )).ToList();

            return BuildTableWidget(
                title: "Training Compliance",
                columns: columns,
                rows: rows,
                viewAllUrl: "/Training/Compliance",
                maxRows: maxRows
            );
        }

        /// <summary>
        /// Helper: Get color for severity
        /// </summary>
        private static string GetSeverityColor(string severity)
        {
            return severity.ToLower() switch
            {
                "fatal" => "danger",
                "major" => "danger",
                "minor" => "warning",
                "near miss" => "info",
                _ => "secondary"
            };
        }

        /// <summary>
        /// Helper: Get color for status
        /// </summary>
        private static string GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "open" or "pending" or "in progress" => "warning",
                "closed" or "completed" or "resolved" or "active" => "success",
                "overdue" or "critical" or "urgent" => "danger",
                "cancelled" or "rejected" or "inactive" => "secondary",
                "approved" => "success",
                "draft" => "info",
                _ => "primary"
            };
        }

        /// <summary>
        /// Helper: Get color for priority
        /// </summary>
        private static string GetPriorityColor(string priority)
        {
            return priority.ToLower() switch
            {
                "high" or "critical" or "urgent" => "danger",
                "medium" or "normal" => "warning",
                "low" => "info",
                _ => "secondary"
            };
        }

        /// <summary>
        /// Helper: Get color for condition
        /// </summary>
        private static string GetConditionColor(string condition)
        {
            return condition.ToLower() switch
            {
                "excellent" or "good" => "success",
                "fair" or "acceptable" => "warning",
                "poor" or "needs replacement" => "danger",
                _ => "secondary"
            };
        }

        /// <summary>
        /// Helper: Get color for compliance rate
        /// </summary>
        private static string GetComplianceColor(decimal rate)
        {
            return rate switch
            {
                >= 90 => "success",
                >= 70 => "warning",
                _ => "danger"
            };
        }

        /// <summary>
        /// Helper: Get status text for compliance rate
        /// </summary>
        private static string GetComplianceStatus(decimal rate)
        {
            return rate switch
            {
                >= 90 => "Compliant",
                >= 70 => "Partial",
                _ => "Non-Compliant"
            };
        }

        /// <summary>
        /// Format date for display
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            var today = DateTime.Today;
            if (date.Date == today)
                return "Today";
            if (date.Date == today.AddDays(-1))
                return "Yesterday";
            if (date.Date == today.AddDays(1))
                return "Tomorrow";
            
            return date.ToString("MMM dd, yyyy");
        }

        /// <summary>
        /// Format percentage for display
        /// </summary>
        public static string FormatPercentage(decimal percentage)
        {
            return $"{percentage:F1}%";
        }

        /// <summary>
        /// Format currency for display
        /// </summary>
        public static string FormatCurrency(decimal amount)
        {
            return $"${amount:N2}";
        }
    }
}
