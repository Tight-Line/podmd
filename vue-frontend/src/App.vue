<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>

<template>
  <div id="app" class="min-h-screen bg-gray-100">
    <!-- Main content -->
    <main class="flex-1">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
#app {
  /* Basic app container */
  min-height: 100vh;
}
</style>
