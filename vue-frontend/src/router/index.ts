import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import HomeView from '../views/HomeView.vue'
import LoginView from '../views/LoginView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: { requiresAuth: true },
    },
  ],
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  // Initialize auth state if not done yet
  if (!authStore.token) {
    authStore.initAuth()
  }

  if (to.matched.some(record => record.meta.requiresAuth)) {
    // Route requires authentication
    if (!authStore.isAuthenticated) {
      next('/login')
    } else {
      next()
    }
  } else if (to.name === 'login' && authStore.isAuthenticated) {
    // If already authenticated and going to login, redirect to home
    next('/')
  } else {
    next()
  }
})

export default router
