# Task: Add Sidebar Navigation

## Overview

Add a professional sidebar with navigation and logout functionality to the PodMD application.

## Steps

### 1. Create AppSidebar.vue

- Component with dark theme and PodMD branding
- Dashboard navigation link with active state
- Logout link at bottom with red hover colors
- Use router-link for navigation and auth store logout

### 2. Create AppLayout.vue

- Layout wrapper component
- Full height flex layout with sidebar and scrollable content area
- Sidebar fixed width, main content takes remaining space

### 3. Update App.vue

- Import AppLayout component
- Show AppLayout for authenticated users, direct RouterView for login
- Keep existing Toast component

### 4. Support for Primeicons

- Add PrimeIcons CSS import: `import 'primeicons/primeicons.css'`

### 5. Update HomeView.vue

- Remove header section (PodMD branding no longer needed)
- Start content directly with page layout

## Testing

- Sidebar shows only for authenticated users
- Navigation links work and highlight active state
- Logout redirects to login page
- Main content scrolls, sidebar stays fixed
- No build errors
