# Vertical Form Wizard Component - Usage Guide

## 📋 Overview

The Vertical Form Wizard displays steps on the **left side** with form content on the **right side**. Perfect for edit forms and desktop-focused workflows.

---

## 🎨 Visual Layout

```
┌────────────────────────────────────────────────────────┐
│  Card Title                                            │
├──────────────┬─────────────────────────────────────────┤
│              │                                         │
│  Progress    │   Step Header                          │
│  25%         │   ┌─────────────────────────┐           │
│  [====    ]  │   │ 📋 Personal Information │           │
│              │   └─────────────────────────┘           │
│  ① Personal  │                                         │
│  ❱ Active    │   Form Fields:                         │
│              │   [Field 1]  [Field 2]                  │
│  ② Work      │   [Field 3]  [Field 4]                  │
│  ○ Pending   │                                         │
│              │                                         │
│  ③ Details   │   [Cancel]    [Previous] [Next]        │
│  ○ Pending   │                                         │
│              │                                         │
└──────────────┴─────────────────────────────────────────┘
```

---

## 🚀 Basic Usage

### **Example: Employee Edit with Vertical Wizard**

```csharp
@using OSHManagement.Extensions
@model OSHManagement.Models.ViewModels.EmployeeViewModel
@{
    ViewData["Title"] = "Edit Employee";

    // Build options (same as Create)
    var stationOptions = /* ... */;
    var departmentOptions = /* ... */;

    // Configure Vertical Wizard
    var wizardConfig = new OSHManagement.Models.ViewModels.FormWizardConfig
    {
        WizardId = "editEmployeeWizard",
        ActionUrl = $"/Employee/Edit/{Model.EmployeeId}",
        Method = "POST",
        Type = WizardType.Vertical, // ← KEY: Set to Vertical
        CardTitle = $"Edit Employee: {Model.FullName}",
        CardSubtitle = "Update employee information",
        ValidateOnStepChange = true,
        ShowProgressBar = true,
        ShowStepNumbers = true,
        CancelUrl = "/Employee/Index",

        Steps = new List<OSHManagement.Models.ViewModels.WizardStepConfig>
        {
            // Step 1: Personal Information
            new OSHManagement.Models.ViewModels.WizardStepConfig
            {
                StepId = "personalInfo",
                Title = "Personal Information",
                Icon = "ri-user-line",
                Description = "Basic personal details", // Shows under title on left
                FieldsPerRow = 2,
                Fields = new List<OSHManagement.Models.ViewModels.FormFieldConfig>
                {
                    new FormFieldConfig
                    {
                        Name = "PayrollNo",
                        Label = "Payroll Number",
                        Type = FieldType.Text,
                        Value = Model.PayrollNo, // ← Pre-fill for edit
                        Required = true
                    },
                    // ... more fields
                }
            },

            // Step 2: Work Assignment
            new WizardStepConfig
            {
                StepId = "workAssignment",
                Title = "Work Assignment",
                Icon = "ri-building-line",
                Description = "Station and department",
                Fields = new List<FormFieldConfig>
                {
                    // ... fields with pre-filled values
                }
            },

            // Step 3: Employment Details
            new WizardStepConfig
            {
                StepId = "employmentDetails",
                Title = "Employment Details",
                Icon = "ri-calendar-line",
                Fields = new List<FormFieldConfig> { /* ... */ }
            },

            // Step 4: Reporting Structure
            new WizardStepConfig
            {
                StepId = "reportingStructure",
                Title = "Reporting Structure",
                Icon = "ri-team-line",
                Fields = new List<FormFieldConfig> { /* ... */ }
            }
        }
    };

    var wizard = wizardConfig.BuildWizard();
}

<!-- Render Vertical Wizard -->
<div class="row">
    <div class="col-xl-12">
        <partial name="~/Views/Shared/Components/Wizards/VerticalFormWizard.cshtml" model="wizard" />
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />

    <script>
        $(document).ready(function() {
            // Add any page-specific JavaScript here
            // Wizard initializes automatically via form-wizard.js
        });
    </script>
}
```

---

## 🎯 Key Features

### **1. Sticky Left Navigation**
- Steps stay visible when scrolling content
- Always know where you are in the process

### **2. Visual Progress Indicators**
- ✅ **Completed Steps**: Green checkmark
- 🔵 **Active Step**: Blue highlight
- ⭕ **Pending Steps**: Gray circle

### **3. Step Descriptions**
- Show context under each step title
- Hidden on mobile to save space

### **4. Progress Bar (Optional)**
```csharp
ShowProgressBar = true  // Shows percentage and visual bar at top
```

### **5. Responsive Design**
- **Desktop**: Steps on left, content on right
- **Tablet**: Steps on left (narrower)
- **Mobile**: Horizontal scrollable steps at top

---

## 🆚 Horizontal vs Vertical Wizard

| Feature | Horizontal | Vertical |
|---------|-----------|----------|
| **Layout** | Tabs on top | Steps on left |
| **Best For** | Create forms | Edit forms |
| **Mobile** | ✅ Excellent | ✅ Good |
| **Desktop** | ✅ Good | ✅ Excellent |
| **Step Info** | Icons only | Icons + descriptions |
| **Visual Progress** | Tab highlighting | Checkmarks + connecting lines |
| **Space Usage** | More vertical | More horizontal |

---

## 📝 Usage Recommendations

### **Use Vertical Wizard For:**
- ✅ Edit forms (users need context)
- ✅ Desktop-heavy workflows
- ✅ Complex forms with detailed step descriptions
- ✅ Admin panels
- ✅ Data review/approval workflows

### **Use Horizontal Wizard For:**
- ✅ Create forms (first-time entry)
- ✅ Mobile-first experiences
- ✅ Simple step-by-step processes
- ✅ User onboarding
- ✅ Public-facing forms

---

## 🎨 Customization

### **Custom Step Icons**
```csharp
Icon = "ri-user-line"        // Personal
Icon = "ri-building-line"    // Work
Icon = "ri-calendar-line"    // Dates
Icon = "ri-team-line"        // Reporting
Icon = "ri-shield-check-line" // Security
Icon = "ri-settings-3-line"   // Settings
```

### **Custom Column Layouts**
```csharp
FieldsPerRow = 1,              // Single column
FieldColumnClass = "col-12"

FieldsPerRow = 2,              // Two columns (default)
FieldColumnClass = "col-md-6"

FieldsPerRow = 3,              // Three columns
FieldColumnClass = "col-md-4"
```

---

## 🔧 Advanced Features

### **Conditional Steps**
```csharp
// In your view logic
var steps = new List<WizardStepConfig>
{
    step1,
    step2,
};

if (Model.EmployeeType == "Contract")
{
    steps.Add(contractDetailsStep);
}

Steps = steps
```

### **Custom Validation**
```javascript
// In @section Scripts
$(document).ready(function() {
    // Access wizard API
    var wizardApi = window.FormWizard.initialize('editEmployeeWizard', {
        onStepChange: function(stepNumber, totalSteps) {
            console.log('Moved to step', stepNumber);
            // Custom logic here
        },
        onComplete: function(form) {
            // Custom pre-submit validation
            return confirm('Are you sure you want to save changes?');
        }
    });
});
```

---

## 🐛 Troubleshooting

### **Steps not showing?**
- Check that `WizardType.Vertical` is set
- Verify `data-wizard-type="vertical"` in form element

### **Progress not updating?**
- Ensure `ShowProgressBar = true`
- Check `data-show-progress-bar="true"` attribute

### **Validation not working?**
- Verify fields have `Required = true`
- Check `ValidateOnStepChange = true`

---

## ✅ Complete Example Files

**Files to reference:**
1. `Views/Shared/Components/Wizards/VerticalFormWizard.cshtml` - Main component
2. `Views/Shared/Components/Wizards/_VerticalFormWizardContent.cshtml` - Content template
3. `wwwroot/js/form-wizard.js` - Generic JavaScript (works for both)
4. `Views/Employee/Create.cshtml` - Horizontal wizard example

---

## 🎉 Summary

**Vertical Wizard = Perfect for Edit Forms!**

- ✅ 30 minutes to implement for any model
- ✅ Zero JavaScript needed (auto-initializes)
- ✅ Fully responsive
- ✅ Consistent with horizontal wizard
- ✅ Beautiful, professional UI

**Just set `Type = WizardType.Vertical` and you're done!** 🚀
