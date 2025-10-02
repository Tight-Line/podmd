import { Api } from './Api'

/**
 * Shared authenticated API instance
 *
 * This singleton provides a single Api instance with centralized JWT authentication
 * for all backend API calls that require authentication.
 */
const authenticatedApi = new Api({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  securityWorker: () => {
    const authToken = localStorage.getItem('auth_token')
    if (authToken) {
      return {
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      }
    }
    return {}
  },
})

// Export the singleton instance
export { authenticatedApi }
