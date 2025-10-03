<template>
  <div class="h-full flex flex-col">
    <!-- List Content -->
    <div class="flex-1 overflow-y-auto">
      <div v-if="loading" class="p-4 text-center">
        <i class="pi pi-spin pi-spinner text-slate-400"></i>
        <p class="text-slate-500 text-sm mt-2">Loading servers...</p>
      </div>

      <div v-else-if="servers.length === 0" class="p-6 text-center">
        <i class="pi pi-server text-slate-300 text-2xl mb-3 block"></i>
        <p class="text-slate-500 text-sm mb-4">No Jenkins servers configured</p>
        <Button
          @click="$emit('add-server')"
          icon="pi pi-plus"
          label="Add Server"
          size="small"
          severity="secondary"
        />
      </div>

      <div v-else class="divide-y divide-slate-200">
        <div
          v-for="server in servers"
          :key="server.id"
          :class="[
            'p-4 cursor-pointer hover:bg-slate-50 transition-colors',
            selectedServer?.id === server.id ? 'bg-blue-50' : ''
          ]"
          @click="$emit('select-server', server)"
        >
          <div class="flex items-center justify-between">
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-1">
                <i class="pi pi-server text-slate-400 text-sm"></i>
                <h3 class="text-sm font-medium text-slate-900 truncate">{{ server.name }}</h3>
              </div>
              <div class="text-xs text-slate-600 space-y-1">
                <div class="flex items-center gap-1">
                  <i class="pi pi-globe text-slate-400"></i>
                  <span class="truncate">{{ server.server }}</span>
                </div>
                <div class="flex items-center gap-1">
                  <i class="pi pi-user text-slate-400"></i>
                  <span>{{ server.username }}</span>
                </div>
              </div>
            </div>

            <!-- Delete button -->
            <Button
              icon="pi pi-trash"
              severity="danger"
              size="small"
              text
              rounded
              @click.stop="$emit('delete-server', server)"
              aria-label="Delete server"
            />
          </div>
        </div>
      </div>
    </div>


  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import type { JenkinsServersResponse } from '../api/Api'

// Props
defineProps<{
  servers: JenkinsServersResponse[]
  loading: boolean
  selectedServer: JenkinsServersResponse | null
}>()

// Emits
defineEmits<{
  'select-server': [server: JenkinsServersResponse]
  'add-server': []
  'delete-server': [server: JenkinsServersResponse]
}>()
</script>
