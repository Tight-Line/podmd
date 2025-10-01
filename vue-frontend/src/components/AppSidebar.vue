<template>
  <!-- Sidebar -->
  <aside class="w-64 bg-tight-nav shadow-lg flex flex-col h-full overflow-hidden">
    <!-- Top Section -->
    <div class="p-6 flex-shrink-0">
      <h2 class="text-xl font-bold text-tight-white mb-8">
        <span class="text-tight-white">Pod</span><span class="text-tight-blue">MD</span>
      </h2>

      <nav class="space-y-2">
        <router-link
          to="/"
          class="flex items-center px-4 py-3 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg transition-colors"
          :class="{ 'bg-slate-700 text-white': route.name === 'home' }"
        >
          <i class="pi pi-home mr-3"></i>
          Dashboard
        </router-link>
      </nav>
    </div>

    <!-- Bottom Section - Sticks to bottom -->
    <div class="mt-auto p-6">
      <nav class="space-y-2">
        <router-link
          to="/login"
          @click="authStore.logout()"
          class="flex items-center px-4 py-3 text-slate-300 hover:text-red-500 hover:bg-slate-700 rounded-lg transition-colors w-full"
          title="Logout"
        >
          <i class="pi pi-sign-out mr-3"></i>
          Logout
        </router-link>
      </nav>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const authStore = useAuthStore()
</script>
