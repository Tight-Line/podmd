# Implement Vue.js Frontend Authentication with JWT

## Objective

Add a login page at the start of the PodMD Vue.js application that authenticates users via JWT tokens from the .NET backend API. Users must login before accessing any other part of the application.

## Context

- Backend: ASP.NET Core API with JWT authentication endpoints
- Frontend: Vue.js 3 + TypeScript + PrimeVue + Pinia
- Current state: Application starts directly at dashboard without authentication

## Prerequisites

- Backend API running with auth endpoints (`POST /api/v1/Auth/login`)
- Swagger/OpenAPI specs available for API client generation
- Environment setup with API base URL configuration

## Implementation Steps

### 1. Generate API Client

Use swagger-typescript-api to create typed client from backend OpenAPI spec.

**Command:**

```bash
cd vue-frontend
npx swagger-typescript-api generate -p http://localhost:8080/swagger/v1/swagger.json -o ./src/api -n Api.ts --axios
```

### 2. Set Up Authentication Store

Create Pinia store for managing JWT tokens and authentication state.

**File:** `src/stores/auth.ts`

**Requirements:**

- Store JWT token in localStorage for persistence
- Provide login method calling backend API
- Provide logout method clearing token
- Computed property for authentication status
- Initialize authentication state from localStorage

### 3. Configure Global Axios Interceptors

Add request interceptor to include JWT in Authorization header for all API calls.

**File:** `src/main.ts`

**Requirements:**

- Attach JWT token from localStorage to outgoing requests
- Handle 401 responses by clearing auth state and redirecting to login
- Set up interceptors in app initialization

### 4. Set Up Router Guards

Implement navigation guards to protect routes and manage auth flow.

**File:** `src/router/index.ts`

**Requirements:**

- Add `/login` route as public access point
- Mark existing routes as requiring authentication
- Redirect unauthenticated users to login
- Redirect authenticated users away from login page
- Initialize auth state in guard logic

### 5. Create Login Component

Build the login page with form and authentication logic.

**File:** `src/views/LoginView.vue`

**Requirements:**

- Email and password input fields with validation
- Form submission calling auth store login method
- Loading states during authentication
- Error message display for failed logins
- Automatic redirect on successful login
- Redirect to dashboard if already authenticated
- Responsive design using Tailwind CSS
- PrimeVue components for inputs and buttons

### 6. Environment Configuration

Ensure API base URL is properly configured via environment variables.

**File:** `vue-frontend/.env`

**Content:**

```
VITE_API_BASE_URL=http://localhost:8080
```

## Verification Tests

### Functional Tests

- [ ] Accessing root URL redirects to login when not authenticated
- [ ] Login with valid credentials redirects to dashboard
- [ ] Login with invalid credentials shows error message
- [ ] Authentication persists across page refreshes
- [ ] Logout clears authentication and redirects to login
- [ ] Protected routes inaccessible without authentication
- [ ] API requests include proper JWT authorization headers

### Technical Tests

- [ ] TypeScript type checking passes
- [ ] Application builds successfully
- [ ] Development server starts without errors
- [ ] API client generates correctly from Swagger spec
- [ ] No console errors during authentication flow

## Error Scenarios to Handle

- Network failures during login attempts
- Invalid JSON responses from API
- Token expiration during user session
- Backend server unavailability
- Invalid form inputs (missing fields, wrong format)

## Security Notes

- JWT tokens stored in localStorage for same-origin accessibility
- Automatic cleanup of expired/invalid tokens
- No sensitive data beyond tokens stored client-side
- Server-side validation remains primary security layer

## Implementation Notes

- Follow Vue 3 Composition API patterns
- Use reactive state management with Pinia
- Maintain consistent error handling patterns
- Ensure responsive mobile-friendly design
- Integrate seamlessly with existing PrimeVue theme
