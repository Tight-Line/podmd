# PodMD Frontend

A modern Vue.js 3 frontend application for PodMD - AI-powered Kubernetes and CI/CD log analysis platform.

## 🚀 Features

- **Vue.js 3** with Composition API
- **TypeScript** for type safety
- **PrimeVue 4** UI component library
- **Tailwind CSS 4** for styling
- **Pinia** for state management
- **Vue Router** for navigation
- **Axios** for API communication
- **Vite** for fast development and building

## 🛠️ Tech Stack

- **Framework**: Vue 3.5 with Composition API
- **Language**: TypeScript 5.9
- **Build Tool**: Vite 7.1
- **Component Library**: PrimeVue 4.3 with Aura theme
- **Styling**: Tailwind CSS 4.1
- **State Management**: Pinia 3.0
- **HTTP Client**: Axios 1.12
- **API Client**: Auto-generated from OpenAPI/Swagger
- **Development**: ESLint + Prettier + Husky

## 📁 Project Structure

```
vue-frontend/
├── src/
│   ├── api/                    # API client and types
│   │   ├── apiClient.ts       # Axios client with interceptors
│   │   └── api-generated.ts   # Auto-generated API client
│   ├── components/
│   │   ├── base/             # Base UI components
│   │   ├── features/         # Feature-specific components
│   │   └── ui/               # PrimeVue components
│   ├── composables/          # Vue composables
│   ├── stores/               # Pinia stores
│   │   ├── auth.ts           # Authentication state
│   │   └── cluster.ts        # Cluster management state
│   ├── types/                # TypeScript type definitions
│   ├── utils/                # Utility functions
│   ├── views/                # Page components
│   ├── router/               # Vue Router configuration
│   ├── assets/               # Static assets
│   ├── App.vue               # Root component
│   └── main.ts               # Application entry point
├── dist/                      # Production build output
├── .env                       # Environment variables
├── package.json
├── vite.config.ts
├── tailwind.config.js
├── tsconfig.json
└── README.md
```

## 🚦 Getting Started

### Prerequisites

- Node.js 20.19+ or 22.12+
- Backend API running (see dotnet-backend/README.md)

### Installation

1. **Clone and navigate to the frontend directory:**

   ```bash
   cd vue-frontend
   ```

2. **Install dependencies:**

   ```bash
   npm install
   ```

3. **Environment setup:**
   - Ensure `.env` file exists with correct API URL
   - Backend should be running on `http://localhost:8080`

4. **Start development server:**

   ```bash
   npm run dev
   ```

5. **Build for production:**
   ```bash
   npm run build
   ```

## 🔧 Available Scripts

| Command              | Description                  |
| -------------------- | ---------------------------- |
| `npm run dev`        | Start development server     |
| `npm run build`      | Build for production         |
| `npm run preview`    | Preview production build     |
| `npm run type-check` | Run TypeScript type checking |
| `npm run lint`       | Run ESLint                   |
| `npm run format`     | Format code with Prettier    |

## 🌐 Environment Variables

Create a `.env` file in the root directory:

```env
# API Configuration
VITE_API_BASE_URL=http://localhost:8080/api/v1
```

## 🔑 Authentication

The application uses JWT tokens for authentication:

- **Login**: `POST /api/v1/Auth/login`
- **Profile**: `GET /api/v1/Auth/me`
- **Clusters**: `GET /api/v1/Clusters`
- **Analysis**: `POST /api/v1/clusters/{id}/Analysis/pods`

Tokens are automatically managed and refreshed by the API client.

## 🎨 Styling

The application uses Tailwind CSS v4 with PrimeVue components:

- **Primary color**: Blue (configurable via Tailwind config)
- **Component library**: PrimeVue with Aura theme
- **Dark mode**: Configurable via CSS layer
- **Responsive**: Mobile-first design approach

## 📊 State Management

Using Pinia for predictable state management:

- **Auth Store**: User authentication and profile
- **Cluster Store**: Kubernetes cluster management
- **Strict Types**: All state is properly typed

## 🏗️ Architecture

### Component Architecture

- **Atomic Design**: Base components → Feature components → Views
- **Composition API**: All components use Vue 3 Composition API
- **TypeScript**: Strict typing throughout the application

### API Layer

- **Generated Client**: Auto-generated from OpenAPI specification
- **Interceptors**: Automatic token handling and error management
- **Type Safety**: Full TypeScript typing for API responses

### Development Standards

- **ESLint**: Vue.js and TypeScript rules enforced
- **Prettier**: Consistent code formatting
- **Conventional Commits**: Version control best practices
- **Clean Architecture**: Separation of concerns maintained

## 🔌 Backend Integration

The frontend integrates with the PodMD .NET backend API:

- **Health Check**: `GET /health`
- **Swagger UI**: `http://localhost:8080/swagger`
- **CORS**: Configured for frontend development
- **OpenAPI**: Auto-generated API client ensures compatibility

## 🚀 Deployment

### Development

```bash
npm run dev
```

### Production Build

```bash
npm run build
npm run preview
```

### Docker (Optional)

```dockerfile
FROM nginx:alpine
COPY dist/ /usr/share/nginx/html/
EXPOSE 80
```

## 📝 Development Guidelines

### Component Creation

1. **Use Composition API** with `<script setup>` syntax
2. **Type all props and emits** explicitly
3. **Follow atomic design** principles
4. **Document component usage** in comments

### API Calls

1. **Use the API client** for all HTTP requests
2. **Handle errors gracefully** with user feedback
3. **Leverage TypeScript types** for request/response data
4. **Implement loading states** for better UX

### State Management

1. **Use appropriate stores** for different domains
2. **Keep state normalized** and predictable
3. **Handle async operations** within store actions
4. **Expose readonly state** to components

## 🐛 Troubleshooting

### Common Issues

1. **Backend not running**: Ensure `dotnet-backend` is started
2. **CORS errors**: Check backend CORS configuration
3. **Type errors**: Run `npm run type-check`
4. **Build issues**: Clear `node_modules` and reinstall

### Debug Commands

```bash
# Clear cache and reinstall
rm -rf node_modules dist
npm install

# Check TypeScript errors
npm run type-check

# Lint and fix issues
npm run lint
```

## 📚 Resources

- [Vue.js 3 Documentation](https://vuejs.org/)
- [PrimeVue Documentation](https://primevue.org/)
- [Tailwind CSS Documentation](https://tailwindcss.com/)
- [Vite Documentation](https://vitejs.dev/)
- [Pinia Documentation](https://pinia.vuejs.org/)

## 🤝 Contributing

1. Follow the established coding standards
2. Write tests for new components
3. Update documentation as needed
4. Ensure builds pass before submitting PRs

## 📄 License

[Add license information here]
