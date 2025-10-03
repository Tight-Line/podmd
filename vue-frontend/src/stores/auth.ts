import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { Api, ContentType } from '../api/Api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const isAuthenticated = computed(() => !!token.value)

  // Initialize from localStorage on store creation
  const initAuth = () => {
    const storedToken = localStorage.getItem('auth_token')
    if (storedToken) {
      token.value = storedToken
    }
  }

  const login = async (email: string, password: string): Promise<boolean> => {
    try {
      const api = new Api({
        baseUrl: import.meta.env.VITE_API_BASE_URL,
      })

      // Manually add login method since swagger generation skipped auth endpoints
      const loginMethod = (request: { email: string, password: string }) =>
        api.request({
          path: `/api/v1/Auth/login`,
          method: "POST",
          body: request,
          type: ContentType.Json,
          format: "json",
        })

      const response = await loginMethod({
        email: email,
        password: password
      })

      if (response.status === 200 && response.data && response.data.token) {
        token.value = response.data.token
        localStorage.setItem('auth_token', response.data.token)
        console.log('Login successful, token stored')
        return true
      }

      console.log('Login failed - no token in response')
      return false
    } catch (error) {
      console.error('Login exception:', error)
      token.value = null
      localStorage.removeItem('auth_token')
      return false
    }
  }

  const logout = () => {
    token.value = null
    localStorage.removeItem('auth_token')
  }

  const getToken = (): string | null => {
    return token.value
  }

  return {
    token,
    isAuthenticated,
    initAuth,
    login,
    logout,
    getToken
  }
})
