<template>
  <div class="h-screen bg-slate-50 overflow-hidden flex flex-col">
    <!-- Application Header -->
    <AppHeader />

    <!-- Main Layout Area -->
    <div class="flex flex-1 overflow-hidden">
      <AppSidebar @logout="handleLogout" />

      <!-- Main Content Area - Scrollable -->
      <main class="flex-1 min-w-0 overflow-y-auto">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import AppHeader from './AppHeader.vue'
import AppSidebar from './AppSidebar.vue'

const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>
