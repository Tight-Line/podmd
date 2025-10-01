<template>
  <div class="flex h-screen bg-slate-50 overflow-hidden">
    <AppSidebar @logout="handleLogout" />

    <!-- Main Content Area - Scrollable -->
    <main class="flex-1 min-w-0 overflow-y-auto">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import AppSidebar from './AppSidebar.vue'

const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>
