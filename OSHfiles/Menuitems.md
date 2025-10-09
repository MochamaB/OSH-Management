# KTDA OSH Management System - Menu Items & Navigation Structure

**Document Version:** 1.0  
**Date:** September 28, 2025  
**Purpose:** Define the complete navigation menu structure based on PRD functional requirements

---

## Menu Structure Overview

The navigation menu is organized into logical groups that align with the core OSH management functions and user workflows. Each menu item corresponds to specific functional requirements from the PRD.

---

## 1. DASHBOARD SECTION

### 1.1 Main Dashboard
- **Label:** Dashboard
- **Icon:** Home/Dashboard icon
- **URL:** `/Dashboard/Index`
- **Description:** Executive overview with key safety metrics
- **PRD Reference:** UI-5, UI-6, UI-7, UI-8

#### 1.1.1 Dashboard Submenus
- **OSH Overview**
  - URL: `/Dashboard/OSHOverview`
  - Description: Station-specific safety performance overview
  - Metrics: Incident rates, compliance status, risk levels

- **Safety Analytics**
  - URL: `/Dashboard/SafetyAnalytics`
  - Description: Advanced analytics and trend analysis
  - Features: Charts, graphs, predictive insights

- **Compliance Dashboard**
  - URL: `/Dashboard/Compliance`
  - Description: Regulatory compliance monitoring
  - PRD Reference: FR-8.1.1, FR-8.1.2, FR-8.1.3

---



---

## 2. TEAM MANAGEMENT SECTION

### 2.1 Team Management
- **Label:** Team Management
- **Icon:** Team/Users icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **PRD Reference:** FR-2.1.1, FR-2.2.1, FR-2.3.1

#### 2.1.1 Team Management Submenus
- **Teams Overview**
  - URL: `/Teams/Index`
  - Description: View all teams across stations
  - PRD Reference: FR-2.1.1, FR-2.1.4

- **Create Team**
  - URL: `/Teams/Create`
  - Description: Create new teams with composition rules
  - PRD Reference: FR-2.1.2, FR-2.1.3

- **Team Members**
  - URL: `/Teams/Members`
  - Description: Manage team member assignments
  - PRD Reference: FR-2.2.1, FR-2.2.2, FR-2.2.3, FR-2.2.4

- **Team Types Configuration**
  - URL: `/Teams/Configuration`
  - Description: Configure team types and rules
  - PRD Reference: FR-2.3.1, FR-2.3.2, FR-2.3.3, FR-2.3.4

---

## 3. OSH POLICY MANAGEMENT SECTION

### 3.1 OSH Policies
- **Label:** OSH Policies
- **Icon:** Document/Policy icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **PRD Reference:** FR-3.1.1, FR-3.2.1

#### 3.1.1 OSH Policy Submenus
- **Policy Management**
  - URL: `/OSHPolicy/Index`
  - Description: View and manage OSH policies
  - PRD Reference: FR-3.1.1, FR-3.1.2, FR-3.1.3, FR-3.1.4

- **Create Policy**
  - URL: `/OSHPolicy/Create`
  - Description: Create new OSH policy for station
  - PRD Reference: FR-3.1.1

- **Responsibilities**
  - URL: `/OSHPolicy/Responsibilities`
  - Description: Define and track responsibilities
  - PRD Reference: FR-3.2.1, FR-3.2.2, FR-3.2.3, FR-3.2.4

- **Policy Compliance**
  - URL: `/OSHPolicy/Compliance`
  - Description: Monitor policy implementation status
  - PRD Reference: FR-3.1.3, FR-3.2.3

---



---

## 5. RISK ASSESSMENT SECTION

### 5.1 Risk Assessment
- **Label:** Risk Assessment
- **Icon:** Risk/Warning icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **PRD Reference:** FR-5.1.1, FR-5.2.1

#### 5.1.1 Risk Assessment Submenus
- **Risk Overview**
  - URL: `/RiskAssessment/Index`
  - Description: View all risk assessments and status
  - PRD Reference: FR-5.1.4, FR-5.3.2

- **Create Assessment**
  - URL: `/RiskAssessment/Create`
  - Description: Create new risk assessment
  - PRD Reference: FR-5.1.1, FR-5.1.2

- **Hazard Identification**
  - URL: `/RiskAssessment/Hazards`
  - Description: Identify and categorize hazards
  - PRD Reference: FR-5.2.1, FR-5.2.2, FR-5.2.3, FR-5.2.4

- **Risk Matrix**
  - URL: `/RiskAssessment/Matrix`
  - Description: Risk calculation and prioritization
  - PRD Reference: FR-5.3.1, FR-5.3.2, FR-5.3.3, FR-5.3.4

- **Mitigation Plans**
  - URL: `/RiskAssessment/Mitigation`
  - Description: Create and track mitigation plans
  - PRD Reference: FR-5.4.1, FR-5.4.2, FR-5.4.3, FR-5.4.4

---

## 6. INCIDENT MANAGEMENT SECTION

### 6.1 Incident Management
- **Label:** Incident Management
- **Icon:** Alert/Incident icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **PRD Reference:** FR-6.1.1, FR-6.2.1

#### 6.1.1 Incident Management Submenus
- **Incident Dashboard**
  - URL: `/Incident/Dashboard`
  - Description: Overview of all incidents and status
  - PRD Reference: FR-6.1.4, FR-6.2.4

- **Report Incident**
  - URL: `/Incident/Create`
  - Description: Report new incident or near miss
  - PRD Reference: FR-6.1.1, FR-6.1.2

- **View Incidents**
  - URL: `/Incident/Index`
  - Description: View and manage all incidents
  - PRD Reference: FR-6.1.3, FR-6.1.4

- **Investigation Management**
  - URL: `/Incident/Investigation`
  - Description: Manage investigation workflows
  - PRD Reference: FR-6.2.1, FR-6.2.2, FR-6.2.3, FR-6.2.4

- **Corrective Actions**
  - URL: `/Incident/Actions`
  - Description: Track corrective actions and implementation
  - PRD Reference: FR-6.3.1, FR-6.3.2, FR-6.3.3, FR-6.3.4

- **Lessons Learned**
  - URL: `/Incident/Lessons`
  - Description: Capture and share lessons learned
  - PRD Reference: FR-6.4.1, FR-6.4.2, FR-6.4.3, FR-6.4.4

---

## 7. DOCUMENT MANAGEMENT SECTION

### 7.1 Document Management
- **Label:** Documents
- **Icon:** Folder/Document icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **PRD Reference:** FR-7.1.1, FR-7.2.1

#### 8.1.1 Document Management Submenus
- **Document Library**
  - URL: `/Documents/Index`
  - Description: Centralized document storage and access
  - PRD Reference: FR-7.1.1, FR-7.1.4

- **Upload Documents**
  - URL: `/Documents/Upload`
  - Description: Upload new documents with categorization
  - PRD Reference: FR-7.1.2, FR-7.1.4

- **Document Categories**
  - URL: `/Documents/Categories`
  - Description: Manage document categories and tags
  - PRD Reference: FR-7.1.4

- **Access Control**
  - URL: `/Documents/Access`
  - Description: Manage document permissions and sharing
  - PRD Reference: FR-7.2.1, FR-7.2.2, FR-7.2.3

- **Document Audit**
  - URL: `/Documents/Audit`
  - Description: Track document access and changes
  - PRD Reference: FR-7.2.3, FR-7.2.4

---

## 9. REPORTING & ANALYTICS SECTION

### 9.1 Reports & Analytics
- **Label:** Reports & Analytics
- **Icon:** Chart/Analytics icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **PRD Reference:** FR-8.1.1, FR-8.2.1

#### 9.1.1 Reports & Analytics Submenus
- **Compliance Reports**
  - URL: `/Reports/Compliance`
  - Description: Generate regulatory compliance reports
  - PRD Reference: FR-8.1.1, FR-8.1.2, FR-8.1.3, FR-8.1.4

- **Performance Analytics**
  - URL: `/Reports/Performance`
  - Description: Safety performance metrics and KPIs
  - PRD Reference: FR-8.2.1, FR-8.2.2, FR-8.2.3, FR-8.2.4

- **Custom Reports**
  - URL: `/Reports/Custom`
  - Description: Build custom reports with drag-and-drop
  - PRD Reference: UI-13, UI-14, UI-15, UI-16

- **Scheduled Reports**
  - URL: `/Reports/Scheduled`
  - Description: Manage automated report generation
  - PRD Reference: UI-16

---
## 10. ORGANIZATIONAL MANAGEMENT SECTION

### 10.1 Organizational Structure
- **Label:** Organization
- **Icon:** Organization/Building icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **PRD Reference:** FR-1.1.1, FR-1.2.1, FR-1.3.1

#### 10.1.1 Organization Submenus
- **Stations Management**
  - URL: `/Organization/Stations`
  - Description: Manage hierarchical station structure
  - PRD Reference: FR-1.1.1, FR-1.1.2, FR-1.1.3, FR-1.1.4

- **Departments**
  - URL: `/Organization/Departments`
  - Description: Department hierarchy and management
  - PRD Reference: FR-1.2.1, FR-1.2.2, FR-1.2.3

- **Employees**
  - URL: `/Organization/Employees`
  - Description: Employee master data management
  - PRD Reference: FR-1.3.1, FR-1.3.2, FR-1.3.3, FR-1.3.4
## 10. SYSTEM ADMINISTRATION SECTION

### 10.1 System Administration
- **Label:** Administration
- **Icon:** Settings/Admin icon
- **URL:** `javascript:void(0);`
- **HasSubMenu:** true
- **Access:** System Administrator only
- **PRD Reference:** SEC-9, SEC-10

#### 11.1.1 Administration Submenus
- **User Management**
  - URL: `/Admin/Users`
  - Description: Manage user accounts and permissions
  - PRD Reference: SEC-9, SEC-5, SEC-6

- **Role Management**
  - URL: `/Admin/Roles`
  - Description: Configure roles and permissions
  - PRD Reference: SEC-5, SEC-7, SEC-8

- **System Configuration**
  - URL: `/Admin/Configuration`
  - Description: System settings and parameters
  - PRD Reference: SEC-11, SEC-12

- **Audit Logs**
  - URL: `/Admin/AuditLogs`
  - Description: View system audit trails
  - PRD Reference: SEC-37, SEC-38, SEC-39

- **Integration Management**
  - URL: `/Admin/Integration`
  - Description: Manage external system integrations
  - PRD Reference: IN-1, IN-9, IN-13, IN-17

---

## BOTTOM MENU ITEMS

### Profile & Settings
- **Profile Settings**
  - URL: `/Profile/Settings`
  - Icon: User profile icon
  - Description: User profile and personal settings

- **Theme Settings**
  - URL: `javascript:void(0);`
  - Icon: Theme/Dark mode icon
  - CssClass: `theme-toggle`
  - Description: Toggle between light/dark themes

- **Help & Support**
  - URL: `/Help/Index`
  - Icon: Help/Question icon
  - Description: User documentation and support

- **Logout**
  - URL: `/Account/Logout`
  - Icon: Logout icon
  - Description: Sign out of the system

---

## ROLE-BASED MENU VISIBILITY

### System Administrator
- Full access to all menu items including Administration section

### Regional Manager
- Dashboard, Organization, Teams, OSH modules, Reports
- Cross-station visibility within assigned region

### Station Manager (Factory Manager)
- Dashboard, Teams, OSH modules, Documents, Reports
- Station-specific data access only

### Department Head
- Dashboard, Teams (department), Risk Assessment, Incident Management
- Department-specific access

### OSH Committee Member
- Dashboard, OSH Committee, Risk Assessment, Incident Management, Documents
- Committee-specific functions

### Employee/Team Member
- Dashboard (personal), Incident Reporting, Documents (view only)
- Limited access based on assignments

---

## NAVIGATION BEHAVIOR

### Active State Management
- Highlight current page in navigation
- Expand parent menu for active submenu items
- Breadcrumb navigation for deep pages

### Responsive Design
- Collapsible sidebar for mobile devices
- Touch-friendly menu interactions
- Consistent navigation across all screen sizes

### Performance Considerations
- Lazy loading for submenu items
- Cached menu structure per user role
- Minimal DOM manipulation for menu state changes

---

## IMPLEMENTATION NOTES

### Menu Configuration
- Menu structure stored in JSON configuration file
- Role-based filtering applied at runtime
- Dynamic menu generation based on user permissions

### URL Structure
- RESTful URL patterns for consistency
- Area-based routing for logical grouping
- SEO-friendly URLs where applicable

### Icons and Styling
- Consistent icon library (Phosphor Icons)
- KTDA brand colors and styling
- Accessibility compliance (WCAG 2.1 Level AA)

---

**End of Document**
