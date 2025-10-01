<template>
  <div id="app" class="min-h-screen">
    <AppLayout v-if="authStore.isAuthenticated">
      <RouterView />
    </AppLayout>

    <RouterView v-else />
  </div>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import { useAuthStore } from './stores/auth'
import AppLayout from './components/AppLayout.vue'

const authStore = useAuthStore()
</script>

<style scoped>
#app {
  /* Basic app container */
  min-height: 100vh;
}
</style>
