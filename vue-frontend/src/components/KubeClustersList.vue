<template>
  <div class="p-4">
    <div v-if="loading" class="text-center py-12">
      <i class="pi pi-spin pi-spinner text-slate-400 text-2xl"></i>
      <p class="text-slate-600 mt-2">Loading clusters...</p>
    </div>
    <div v-else-if="clusters.length === 0" class="text-center py-12">
      <div class="w-16 h-16 bg-slate-100 rounded-full flex items-center justify-center mx-auto mb-4">
        <i class="pi pi-server text-slate-400 text-2xl"></i>
      </div>
      <h3 class="text-lg font-medium text-slate-900 mb-2">No clusters configured</h3>
      <p class="text-slate-600 mb-6">Get started by adding your first Kubernetes cluster.</p>
      <Button
        @click="$emit('add-cluster')"
        icon="pi pi-plus"
        label="Add Your First Cluster"
        severity="primary"
      />
    </div>
    <div v-else>
      <DataView :value="clusters" layout="list">
        <template #list="slotProps">
          <div class="space-y-2">
            <div
              v-for="cluster in slotProps.items"
              :key="cluster.id"
              @click="$emit('select-cluster', cluster)"
              :class="[
                'p-3 rounded-lg cursor-pointer border transition-colors',
                selectedCluster?.id === cluster.id
                  ? 'bg-blue-50 border-blue-200 shadow-sm'
                  : 'bg-white border-slate-200 hover:bg-slate-50'
              ]"
            >
              <div class="flex items-center justify-between">
                <div class="flex-1 min-w-0">
                  <div class="font-medium text-slate-900 truncate">{{ cluster.name }}</div>
                  <div class="text-sm text-slate-500 mt-1">{{ formatDate(cluster.createdAt) }}</div>
                </div>
                <div class="flex gap-1 ml-2">
                  <Button
                    @click.stop="$emit('delete-cluster', cluster)"
                    icon="pi pi-trash"
                    severity="danger"
                    size="small"
                    v-tooltip="'Delete Cluster'"
                  />
                </div>
              </div>
            </div>
          </div>
        </template>
      </DataView>
    </div>
  </div>
</template>

<script setup lang="ts">
import DataView from 'primevue/dataview'
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

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}
</script>
