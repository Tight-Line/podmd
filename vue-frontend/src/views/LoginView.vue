<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Card from 'primevue/card'
import Message from 'primevue/message'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

onMounted(() => {
  // If already authenticated, redirect to home
  if (authStore.isAuthenticated) {
    router.push('/')
  }
})

const handleLogin = async () => {
  if (!email.value || !password.value) {
    errorMessage.value = 'Please enter both email and password'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    const success = await authStore.login(email.value, password.value)

    if (success) {
      router.push('/')
    } else {
      errorMessage.value = 'Invalid login credentials'
    }
  } catch (error) {
    console.error('Login error:', error)
    errorMessage.value = 'An error occurred during login'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-50 flex items-center justify-center p-4">
    <Card class="w-full max-w-md shadow-lg">
      <template #title>
        <div class="text-center">
          <div class="w-16 h-16 mx-auto mb-4 bg-blue-500 rounded-full flex items-center justify-center shadow-lg">
            <i class="pi pi-lock text-white text-2xl"></i>
          </div>
          <h1 class="text-2xl font-bold text-slate-900">Welcome to PodMD</h1>
          <p class="text-slate-600 mt-2">Sign in to access your dashboard</p>
        </div>
      </template>

      <template #content>
        <form @submit.prevent="handleLogin" class="space-y-4">
          <div class="space-y-2">
            <label for="email" class="block text-sm font-medium text-slate-700">
              Email Address
            </label>
            <InputText
              id="email"
              v-model="email"
              type="email"
              placeholder="Enter your email"
              class="w-full"
              :disabled="loading"
              required
            />
          </div>

          <div class="space-y-2">
            <label for="password" class="block text-sm font-medium text-slate-700">
              Password
            </label>
            <Password
              id="password"
              v-model="password"
              placeholder="Enter your password"
              :disabled="loading"
              toggleMask
              :feedback="false"
              class="w-full"
              inputClass="w-full"
            />
          </div>

          <Message
            v-if="errorMessage"
            severity="error"
            :closable="false"
            class="w-full"
          >
            {{ errorMessage }}
          </Message>

          <Button
            type="submit"
            label="Sign In"
            class="w-full"
            :loading="loading"
            :disabled="loading"
          />
        </form>
      </template>
    </Card>
  </div>
</template>

<style scoped>
/* Additional custom styles if needed */
</style>
