import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { Api } from '../api/Api'

interface AuthResponse {
  token: string
  expiration: string
}

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
        baseURL: import.meta.env.VITE_API_BASE_URL,
      })
      const loginRequest = {
        email,
        password
      }

      const response = await api.api.v1AuthLoginCreate(loginRequest)

      if (response.status === 200) {
        // Parse the response data (assuming axios response has data property)
        const authData: AuthResponse = response.data as unknown as AuthResponse
        token.value = authData.token
        localStorage.setItem('auth_token', authData.token)

        return true
      }

      return false
    } catch (error) {
      console.error('Login failed:', error)
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
