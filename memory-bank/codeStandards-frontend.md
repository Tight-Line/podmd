# Frontend Code Standards - Vue.js/TypeScript

## Purpose

These standards define coding, structure, and development conventions for **Vue.js frontend applications** with TypeScript, focusing on component architecture, state management, API integration, and modern web development practices.

---

## 1. Vue.js Development Standards

### Component Architecture

- **Composition API**: Prefer Composition API over Options API for new components
- **Component Naming**: PascalCase for component files and registrations
- **Single Responsibility**: Each component should have one clear purpose
- **Component Communication**: Use props for parent-to-child, emit for child-to-parent
- **Slots**: Use slots for flexible component composition

### Script Setup Syntax

```vue
<script setup lang="ts">
// Preferred syntax for Composition API
import { ref, computed } from "vue";

const props = defineProps<{
  title: string;
  count?: number;
}>();

const emit = defineEmits<{
  update: [count: number];
}>();

// Component logic here
</script>
```

### Component Organization

- **Template**: HTML structure at the top
- **Script**: TypeScript logic in `<script setup>`
- **Style**: Scoped styles at the bottom
- **Composition**: Group related logic with composables

---

## 2. TypeScript Standards

### Type Definitions

- **Interface vs Type**: Use `interface` for object shapes, `type` for unions/aliases
- **Optional Properties**: Use `?:` for optional properties, avoid `| undefined`
- **Generic Constraints**: Use generics for reusable component props
- **Utility Types**: Leverage `Partial<T>`, `Pick<T>`, `Omit<T>` appropriately

### API Type Safety

- **Generated Types**: Use swagger-typescript-api generated types
- **Request/Response Types**: Define specific types for API contracts
- **Error Types**: Create union types for different error scenarios
- **Loading States**: Type loading states appropriately

### Component Props

```ts
// Good: Specific types
interface UserCardProps {
  user: User;
  showActions: boolean;
  onEdit: (user: User) => void;
}

// Avoid: Generic any types
interface BadProps {
  data: any;
  callback: (data: any) => void;
}
```

---

## 3. State Management Standards

### Pinia Store Patterns

- **Store Naming**: Use descriptive names (useUserStore, useAppStore)
- **State Structure**: Group related state logically
- **Actions**: Use async actions for API calls
- **Getters**: Create computed getters for derived state

### Store Organization

```ts
// stores/userStore.ts
export const useUserStore = defineStore("user", () => {
  // State
  const users = ref<User[]>([]);
  const loading = ref(false);

  // Getters
  const activeUsers = computed(() => users.value.filter((u) => u.isActive));

  // Actions
  const fetchUsers = async () => {
    loading.value = true;
    try {
      users.value = await api.getUsers();
    } finally {
      loading.value = false;
    }
  };

  return {
    users,
    loading,
    activeUsers,
    fetchUsers,
  };
});
```

### State Composition

- **Feature Stores**: Separate stores for different domains
- **Shared State**: Use app store for global application state
- **Persistence**: Persist critical state to localStorage when needed

---

## 4. Component Design Standards

### Atomic Design Principles

- **Atoms**: Basic UI elements (Button, Input, Icon)
- **Molecules**: Combinations of atoms (FormField, Card)
- **Organisms**: Complex components (UserProfile, DataTable)
- **Templates**: Page layouts
- **Pages**: Complete views

### Component Patterns

- **Base Components**: Generic, reusable components in `components/base/`
- **Feature Components**: Specific to features in `components/features/`
- **Layout Components**: Page structure components
- **Composition**: Build complex components from simpler ones

### Props Design

- **Required vs Optional**: Make required props explicit
- **Default Values**: Provide sensible defaults where appropriate
- **Validation**: Use prop validators for complex requirements
- **Type Safety**: Leverage TypeScript for prop type checking

---

## 5. Styling Standards

### Tailwind CSS Patterns

- **Utility Classes**: Use Tailwind utilities for styling
- **Component Classes**: Create component-specific classes when needed
- **Responsive Design**: Use responsive prefixes (sm:, md:, lg:)
- **Dark Mode**: Support dark mode with dark: prefix

### CSS Organization

- **Scoped Styles**: Use `<style scoped>` for component-specific styles
- **CSS Variables**: Define design tokens as CSS variables
- **Class Naming**: Use descriptive, component-prefixed class names

### Design System

- **Color Palette**: Define consistent color variables
- **Typography**: Standardize font sizes and weights
- **Spacing**: Use consistent spacing scale
- **Component Variants**: Define standard component variations

---

## 6. API Integration Standards

### Client Architecture

- **Generated Client**: Use swagger-typescript-api generated client
- **Base Configuration**: Set up base URL and default headers
- **Interceptors**: Add request/response interceptors for auth and error handling
- **Error Handling**: Centralized error handling and user feedback

### Request Patterns

```ts
// services/apiService.ts
import { api } from "@/api/api-generated";

export const userService = {
  async getUsers() {
    try {
      const response = await api.users.getUsers();
      return response.data;
    } catch (error) {
      // Handle error centrally
      throw new Error("Failed to fetch users");
    }
  },
};
```

### Authentication

- **Token Storage**: Store JWT in localStorage with expiration
- **Request Headers**: Automatically add auth headers to requests
- **Token Refresh**: Implement automatic token refresh logic
- **Logout Handling**: Clear tokens and redirect on auth failures

---

## 7. Testing Standards

### Vitest Configuration

- **Test Files**: Place alongside components (`Component.test.ts`)
- **Test Structure**: Arrange-Act-Assert pattern
- **Mocking**: Use vitest mocks for API calls and dependencies
- **Coverage**: Aim for high coverage on critical components

### Component Testing

```ts
// components/UserCard.test.ts
import { describe, it, expect } from "vitest";
import { mount } from "@vue/test-utils";
import UserCard from "./UserCard.vue";

describe("UserCard", () => {
  it("displays user name", () => {
    const user = { name: "John Doe", email: "john@example.com" };
    const wrapper = mount(UserCard, {
      props: { user },
    });

    expect(wrapper.text()).toContain("John Doe");
  });
});
```

### Testing Best Practices

- **Unit Tests**: Test component logic in isolation
- **Integration Tests**: Test component interactions
- **E2E Tests**: Test complete user workflows
- **Mock External Dependencies**: API calls, router, stores

---

## 8. Performance Standards

### Bundle Optimization

- **Code Splitting**: Use dynamic imports for route-based splitting
- **Lazy Loading**: Lazy load components and routes
- **Tree Shaking**: Ensure unused code is removed
- **Bundle Analysis**: Regularly analyze bundle sizes

### Runtime Performance

- **Reactivity**: Use computed properties for expensive calculations
- **Memoization**: Cache expensive operations
- **Virtual Scrolling**: For large lists
- **Image Optimization**: Use appropriate image formats and lazy loading

### Development Performance

- **Fast Refresh**: Ensure HMR works correctly
- **Build Speed**: Optimize build times
- **Dev Server**: Configure for fast development iteration

---

## 9. File Structure Standards

### Vue.js Project Layout

```
src/
├── api/                 # Generated API client
├── components/
│   ├── base/           # Reusable base components
│   ├── features/       # Feature-specific components
│   └── ui/             # UI library components
├── composables/        # Vue composables
├── stores/             # Pinia stores
├── types/              # TypeScript type definitions
├── utils/              # Utility functions
├── views/              # Page components
├── router/             # Vue Router configuration
├── styles/             # Global styles and Tailwind config
└── main.ts             # Application entry point
```

### File Naming Conventions

- **Components**: PascalCase (`UserCard.vue`)
- **Composables**: camelCase with `use` prefix (`useUser.ts`)
- **Stores**: camelCase with `use` prefix (`useUserStore.ts`)
- **Types**: PascalCase (`User.ts`, `ApiResponse.ts`)
- **Utils**: camelCase (`formatDate.ts`)

---

## 10. Development Workflow Standards

### Code Quality

- **ESLint**: Configure for Vue.js and TypeScript
- **Prettier**: Use for consistent code formatting
- **Husky**: Pre-commit hooks for linting and formatting
- **Commitlint**: Enforce conventional commit messages

### Development Tools

- **Vue DevTools**: Use for debugging component state
- **Vite Dev Server**: Fast development with HMR
- **TypeScript Compiler**: Strict mode enabled
- **Browser DevTools**: For debugging and performance analysis

### Deployment

- **Build Optimization**: Production builds with minification
- **Asset Optimization**: Compress images and fonts
- **CDN Integration**: For static assets
- **Environment Variables**: Proper env var handling for production
