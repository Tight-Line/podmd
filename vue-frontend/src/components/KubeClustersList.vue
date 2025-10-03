<template>
  <div class="flex-1 overflow-y-auto">
    <div v-if="loading" class="p-4 text-center">
      <i class="pi pi-spin pi-spinner text-slate-400"></i>
      <p class="text-slate-500 text-sm mt-2">Loading clusters...</p>
    </div>

    <div v-else-if="clusters.length === 0" class="p-6 text-center">
      <i class="pi pi-server text-slate-300 text-2xl mb-3 block"></i>
      <p class="text-slate-500 text-sm mb-4">No clusters configured</p>
      <Button
        @click="$emit('add-cluster')"
        icon="pi pi-plus"
        label="Add Cluster"
        size="small"
        severity="secondary"
      />
    </div>

    <div v-else class="divide-y divide-slate-200">
      <div
        v-for="cluster in clusters"
        :key="cluster.id"
        :class="[
          'p-4 cursor-pointer hover:bg-slate-50 transition-colors',
          selectedCluster?.id === cluster.id ? 'bg-blue-50' : ''
        ]"
        @click="$emit('select-cluster', cluster)"
      >
        <div class="flex items-center justify-between">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-1">
              <i class="pi pi-server text-slate-400 text-sm"></i>
              <h3 class="text-sm font-medium text-slate-900 truncate">{{ cluster.name }}</h3>
            </div>
            <div class="text-xs text-slate-600 flex items-center gap-1">
              <i class="pi pi-globe text-slate-400"></i>
              <span class="truncate">{{ cluster.server }}</span>
            </div>
          </div>

          <!-- Delete button -->
          <Button
            icon="pi pi-trash"
            severity="danger"
            size="small"
            text
            rounded
            @click.stop="$emit('delete-cluster', cluster)"
            aria-label="Delete cluster"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import type { KubeClusterResponse } from '../api/Api'

defineProps<{
  clusters: KubeClusterResponse[]
  loading: boolean
  selectedCluster: KubeClusterResponse | null
}>()

defineEmits<{
  'select-cluster': [cluster: KubeClusterResponse]
  'add-cluster': []
  'delete-cluster': [cluster: KubeClusterResponse]
}>()
</script>
