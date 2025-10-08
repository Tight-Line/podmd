<template>
  <!-- Sidebar -->
  <aside class="w-48 bg-tight-nav shadow-lg flex flex-col h-full overflow-hidden">
    <!-- Top Section -->
    <div class="p-6 flex-shrink-0">
      <nav class="space-y-2">
        <router-link
          to="/"
          class="flex items-center px-4 py-3 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg transition-colors"
          :class="{ 'bg-slate-700 text-white': route.name === 'home' }"
        >
          <i class="pi pi-home mr-3"></i>
          Dashboard
        </router-link>

        <router-link
          to="/kube-clusters"
          class="flex items-center px-4 py-3 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg transition-colors"
          :class="{ 'bg-slate-700 text-white': route.name === 'kube-clusters' }"
        >
          <i class="pi pi-server mr-3"></i>
          Clusters
        </router-link>

        <router-link
          to="/jenkins-servers"
          class="flex items-center px-4 py-3 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg transition-colors"
          :class="{ 'bg-slate-700 text-white': route.name === 'jenkins-servers' }"
        >
          <i class="pi pi-cog mr-3"></i>
          Jenkins Servers
        </router-link>

        <router-link
          to="/knowledge-bases"
          class="flex items-center px-4 py-3 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg transition-colors"
          :class="{ 'bg-slate-700 text-white': route.name === 'knowledge-bases' }"
        >
          <i class="pi pi-database mr-3"></i>
          Knowledge Bases
        </router-link>

        <router-link
          to="/log-analysis"
          class="flex items-center px-4 py-3 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg transition-colors"
          :class="{ 'bg-slate-700 text-white': route.name === 'log-analysis' }"
        >
          <i class="pi pi-search mr-3"></i>
          Log Analysis
        </router-link>
      </nav>
    </div>


  </aside>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()
</script>
