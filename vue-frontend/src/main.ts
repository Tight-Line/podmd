import './assets/style.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { useAuthStore } from './stores/auth'
import axios from 'axios'

import App from './App.vue'
import router from './router'

import PrimeVue from 'primevue/config'
import { definePreset } from '@primeuix/themes'
import Aura from '@primeuix/themes/aura'
import ToastService from 'primevue/toastservice'
import Tooltip from 'primevue/tooltip'

const app = createApp(App)
const pinia = createPinia()

// Set up axios global interceptors for authentication
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor to handle authentication errors
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token is invalid or expired
      const authStore = useAuthStore(pinia)
      authStore.logout()
      router.push('/login')
    }
    return Promise.reject(error)
  }
)

app.use(pinia)
app.use(router)
app.directive('tooltip', Tooltip)

const CustomPreset = definePreset(Aura, {
  semantic: {
    // Define tightNav semantic color with full scale
    tightNav: {
      50: 'rgb(242, 245, 248)',
      100: 'rgb(213, 221, 230)',
      200: 'rgb(154, 169, 187)',
      300: 'rgb(113, 130, 149)',
      400: 'rgb(85, 99, 116)',
      500: 'rgb(5, 16, 30)',
      600: 'rgb(2, 9, 18)',
      700: 'rgb(1, 6, 12)',
      800: 'rgb(2, 8, 16)',
      900: 'rgb(1, 5, 9)',
      950: 'rgb(1, 3, 6)'
    },
    primary: {
      50: '{tightNav.50}',
      100: '{tightNav.100}',
      200: '{tightNav.200}',
      300: '{tightNav.300}',
      400: '{tightNav.400}',
      500: '{tightNav.500}',
      600: '{tightNav.600}',
      700: '{tightNav.700}',
      800: '{tightNav.800}',
      900: '{tightNav.900}',
      950: '{tightNav.950}'
    }
  }
})


app.use(PrimeVue, {
  theme: {
    preset: CustomPreset,
    options: {
      prefix: 'p',
      darkModeSelector: '.dark',
      cssLayer: {
        name: 'primevue',
        order: 'theme, base, primevue'
      }
    }
  }
})
app.use(ToastService)

app.mount('#app')
