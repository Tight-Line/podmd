<template>
  <!-- User Menu Dropdown -->
  <div class="relative">
    <Button
      @click="toggleMenu"
      class="px-3 py-2 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg flex items-center gap-2 transition-colors"
      :class="{ 'bg-slate-700 text-white': isMenuOpen }"
    >
      <i class="pi pi-user text-lg"></i>
      <span class="hidden sm:block text-sm font-medium">Account</span>
      <i class="pi pi-chevron-down text-xs transition-transform" :class="{ 'rotate-180': isMenuOpen }"></i>
    </Button>

    <!-- Dropdown Menu -->
    <div
      v-if="isMenuOpen"
      class="absolute right-0 top-full mt-2 w-48 bg-white border border-slate-200 rounded-lg shadow-lg z-50"
    >
      <div class="px-4 py-3 bg-slate-50 border-b border-slate-200">
        <p class="text-sm font-medium text-slate-900">User Menu</p>
        <p class="text-xs text-slate-600">Manage your account</p>
      </div>

      <div class="py-1">
        <button
          @click="goToSettings"
          class="w-full px-4 py-2 text-left text-sm text-slate-700 hover:bg-slate-100 hover:text-slate-900 flex items-center gap-2"
        >
          <i class="pi pi-cog"></i>
          Settings
        </button>

        <div class="border-t border-slate-200 my-1"></div>

        <button
          @click="logout"
          class="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-slate-100 hover:text-red-700 flex items-center gap-2"
        >
          <i class="pi pi-sign-out"></i>
          Sign Out
        </button>
      </div>
    </div>

    <!-- Click outside overlay -->
    <div
      v-if="isMenuOpen"
      @click="closeMenu"
      class="fixed inset-0 z-40"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import Button from 'primevue/button'

const router = useRouter()
const authStore = useAuthStore()
const isMenuOpen = ref(false)

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}

const closeMenu = () => {
  isMenuOpen.value = false
}

const goToSettings = () => {
  closeMenu()
  router.push('/settings')
}

const logout = () => {
  closeMenu()
  authStore.logout()
  router.push('/login')
}

// Close menu on escape key
const handleKeyDown = (event: KeyboardEvent) => {
  if (event.key === 'Escape' && isMenuOpen.value) {
    closeMenu()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleKeyDown)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeyDown)
})
</script>
