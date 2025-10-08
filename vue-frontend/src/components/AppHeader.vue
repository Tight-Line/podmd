<template>
  <!-- Application Header -->
  <header class="h-16 bg-tight-nav shadow-lg flex items-center justify-between px-6 border-b border-slate-700">
    <!-- Left side - Branding -->
    <div class="flex items-center">
      <h1 class="text-xl font-bold text-tight-white">
        <span class="text-tight-white">Pod</span><span class="text-tight-blue">MD</span>
      </h1>
    </div>

    <!-- Right side - User Menu -->
    <div class="flex items-center">
      <HeaderUserMenu />
    </div>
  </header>
</template>

<script setup lang="ts">
import HeaderUserMenu from './HeaderUserMenu.vue'
</script>

<style scoped>
/* Header styling is handled by Tailwind classes */
</style>
