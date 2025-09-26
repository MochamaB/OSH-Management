# Velzon Theme Integration Guide

## Overview
The Velzon admin theme has been successfully integrated into the KTDA OSH Management System. This document provides guidance on using the theme assets and customizations.

## File Structure

### CSS Files
- `~/css/velzon/styles.css` - Main Velzon theme styles
- `~/css/velzon/icons.css` - Icon fonts and styles
- `~/css/velzon/velzon-theme.css` - KTDA-specific customizations

### JavaScript Files
- `~/js/velzon/main.js` - Core Velzon functionality
- `~/js/velzon/custom.js` - Velzon utilities and components
- `~/js/velzon/defaultmenu.min.js` - Menu system
- `~/js/velzon/custom-switcher.min.js` - Theme switcher
- `~/js/velzon/osh-custom.js` - OSH-specific JavaScript functionality

### Assets
- `~/fonts/` - Icon fonts (Bootstrap Icons, Boxicons, Feather, etc.)
- `~/images/velzon/` - Theme images and graphics
- `~/lib/velzon/` - Third-party libraries (Bootstrap, ApexCharts, etc.)

## KTDA Brand Customizations

### Color Scheme
The theme has been customized with KTDA brand colors:
- **Primary Green**: `#228B22` (KTDA corporate green)
- **Secondary Brown**: `#8B4513` (Tea brown)
- **Success**: `#32CD32` (Safety compliance)
- **Warning**: `#FF8C00` (Safety warnings)
- **Danger**: `#DC143C` (Safety critical)

### OSH-Specific Components

#### Safety Status Indicators
```html
<span class="safety-status-compliant">Compliant</span>
<span class="safety-status-warning">Warning</span>
<span class="safety-status-critical">Critical</span>
```

#### OSH Cards
```html
<div class="card osh-card">
    <div class="card-body">
        <!-- Content -->
    </div>
</div>
```

#### Compliance Meters
```html
<div class="compliance-meter">
    <div class="compliance-meter-fill" data-percentage="85"></div>
</div>
```

#### Risk Matrix Cells
```html
<div class="risk-cell risk-low">1</div>
<div class="risk-cell risk-medium">4</div>
<div class="risk-cell risk-high">9</div>
```

#### Action Items
```html
<div class="action-item action-pending">
    <h6>Action Title</h6>
    <p>Action description...</p>
</div>
```

## Layout Usage

### Basic Page Structure
```html
@{
    ViewData["Title"] = "Page Title";
    ViewData["Breadcrumb"] = "<li class='breadcrumb-item'><a href='/'>Home</a></li><li class='breadcrumb-item active'>Current Page</li>";
}

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header">
                <h4 class="card-title mb-0">Card Title</h4>
            </div>
            <div class="card-body">
                <!-- Content -->
            </div>
        </div>
    </div>
</div>
```

### Form Structure
```html
<form class="needs-validation" novalidate>
    <div class="osh-form-section">
        <h5>Section Title</h5>
        <div class="row">
            <div class="col-md-6">
                <div class="mb-3">
                    <label class="form-label required">Field Label</label>
                    <input type="text" class="form-control" required>
                    <div class="invalid-feedback">Please provide a valid value.</div>
                </div>
            </div>
        </div>
    </div>
    <div class="text-end">
        <button type="submit" class="btn btn-primary">Submit</button>
    </div>
</form>
```

## JavaScript Functionality

### OSH Custom Functions
The `osh-custom.js` file provides several utility functions:

```javascript
// Calculate risk rating
OSH.calculateRiskRating();

// Update action item status
OSH.updateActionItemStatus(element, 'completed');

// Show toast notification
OSH.showToast('Success message', 'success');

// Confirm critical actions
OSH.confirmAction('Are you sure?', callback);

// Validate form field
OSH.validateField(fieldElement);
```

### Form Validation
Forms with the `needs-validation` class will automatically have enhanced validation:
- Real-time validation on blur
- Visual feedback with Bootstrap classes
- Automatic focus on first invalid field

### File Upload Preview
File inputs will automatically show preview when files are selected:
```html
<input type="file" id="document-upload" multiple>
<div id="document-upload-preview"></div>
```

## Available Libraries

The following libraries are included and ready to use:

### Charts and Visualization
- **ApexCharts** - `~/lib/velzon/apexcharts/`
- **Chart.js** - Available via CDN or add to libs

### Form Components
- **Choices.js** - Enhanced select dropdowns
- **Flatpickr** - Date/time picker
- **Dropzone** - File upload with drag & drop

### UI Components
- **SweetAlert2** - Beautiful alerts and confirmations
- **Bootstrap** - Complete UI framework
- **GridJS** - Advanced data tables

### Utilities
- **SimpleBar** - Custom scrollbars
- **Node Waves** - Material design ripple effects

## Theme Customization

### Adding Custom Styles
Add custom styles to `~/css/site.css` or create new CSS files and reference them in the layout.

### Modifying Colors
Update the CSS custom properties in `velzon-theme.css`:
```css
:root {
  --primary-rgb: 34, 139, 34;
  --bs-primary: #228B22;
  /* Add more customizations */
}
```

### Adding New Components
Follow the existing pattern in `osh-custom.js` for new JavaScript functionality.

## Best Practices

1. **Use Semantic HTML** - Proper heading hierarchy, form labels, etc.
2. **Mobile First** - Design for mobile, enhance for desktop
3. **Accessibility** - Include ARIA labels, keyboard navigation
4. **Performance** - Only load required assets on each page
5. **Consistency** - Use established patterns and components

## Troubleshooting

### Common Issues

1. **Icons not showing** - Ensure `icons.css` is loaded and font files are accessible
2. **JavaScript errors** - Check that all required libraries are loaded in correct order
3. **Styling conflicts** - Check CSS specificity and load order
4. **Mobile layout issues** - Verify responsive classes and viewport meta tag

### Debug Mode
Add `?debug=true` to URLs to enable additional logging in `osh-custom.js`.

## Support

For theme-related issues:
1. Check browser console for JavaScript errors
2. Verify all CSS/JS files are loading correctly
3. Test in different browsers
4. Check responsive design on various screen sizes

## Updates

When updating the theme:
1. Backup current customizations
2. Update core Velzon files
3. Merge customizations carefully
4. Test all functionality thoroughly
5. Update this documentation as needed
