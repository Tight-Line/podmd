<template>
  <div class="h-screen flex flex-col">
    <!-- Page Header -->
    <div class="flex justify-between items-center px-6 py-4 bg-white border-b border-slate-200">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Jenkins Servers</h1>
        <p class="text-slate-600 mt-1">Manage your configured Jenkins servers</p>
      </div>
      <Button
        @click="openCreateDialog"
        icon="pi pi-plus"
        label="Add Server"
        severity="primary"
      />
    </div>

    <!-- Main Content - Split Panel -->
    <div class="flex-1 overflow-hidden">
      <Splitter :minSizes="[25, 35]" :gutterSize="8" class="h-full">
        <!-- Left Panel - Server List -->
        <SplitterPanel :size="30" :minSize="25">
          <div class="h-full bg-white border-r border-slate-200">
            <JenkinsServersList
              :servers="servers"
              :loading="loading"
              :selected-server="selectedServer"
              @select-server="selectServer"
              @add-server="openCreateDialog"
              @delete-server="openDeleteDialog"
            />
          </div>
        </SplitterPanel>

        <!-- Right Panel - Server Details -->
        <SplitterPanel :size="70" :minSize="35">
          <div class="h-full bg-white">
            <div v-if="selectedServer || isCreating" class="h-full flex flex-col">
              <div class="p-4 border-b border-slate-200 flex items-center justify-between">
                <h2 class="text-lg font-semibold text-slate-900">
                  {{ isCreating ? 'Create Server' : isEditing ? 'Edit Server' : 'Server Details' }}
                </h2>
                <div v-if="!isEditing && !isCreating" class="flex gap-2">
                  <Button
                    @click="startEdit"
                    icon="pi pi-pencil"
                    label="Edit"
                    severity="secondary"
                    size="small"
                  />
                </div>
                <div v-if="isEditing || isCreating" class="flex gap-2">
                  <Button
                    @click="cancelEdit"
                    label="Cancel"
                    severity="secondary"
                    size="small"
                  />
                </div>
              </div>
              <div class="p-4 flex-1 overflow-auto">
                <JenkinsServersForm
                  v-if="selectedServer || isCreating"
                  :key="selectedServer?.id || 'creating'"
                  :initial-values="formData"
                  :is-editing="isEditing || isCreating"
                  :read-only="!isEditing && !isCreating"
                  :saving="saving"
                  @save-server="handleFormSave"
                />
              </div>
            </div>
            <div v-else class="h-full flex items-center justify-center">
              <div class="text-center">
                <i class="pi pi-server text-slate-400 text-4xl mb-4 block"></i>
                <h3 class="text-lg font-medium text-slate-900 mb-2">Select a server to view details</h3>
                <p class="text-slate-600">Choose a server from the left panel to see its configuration</p>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>

    <!-- Create Server Dialog -->
    <Dialog
      v-model:visible="createDialogVisible"
      header="Create New Jenkins Server"
      modal
      :style="{ width: '600px' }"
      :closable="true"
    >
      <JenkinsServersForm
        :is-editing="true"
        :read-only="false"
        :saving="saving"
        :initial-values="{
          name: '',
          server: '',
          username: '',
          apiToken: '',
          instructions: '',
          responseFormat: ''
        }"
        @save-server="handleCreateServer"
      />
    </Dialog>

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialogVisible"
      header="Confirm Delete"
      modal
      :style="{ width: '400px' }"
    >
      <div class="flex items-center gap-3">
        <i class="pi pi-exclamation-triangle text-amber-500 text-xl"></i>
        <div>
          <p class="font-medium text-slate-900 mb-1">Delete Jenkins Server</p>
          <p class="text-slate-600 text-sm">
            Are you sure you want to delete the Jenkins server <strong>{{ serverToDelete?.name }}</strong>?
            This action cannot be undone.
          </p>
        </div>
      </div>

      <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-200">
        <Button
          @click="closeDeleteDialog"
          label="Cancel"
          severity="secondary"
          size="small"
        />
        <Button
          @click="deleteServer"
          :loading="deleting"
          label="Delete"
          severity="danger"
          icon="pi pi-trash"
          size="small"
        />
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import Dialog from 'primevue/dialog'
import Splitter from 'primevue/splitter'
import SplitterPanel from 'primevue/splitterpanel'
import Button from 'primevue/button'
import type { JenkinsServersResponse, CreateJenkinsServersRequest, UpdateJenkinsServersRequest } from '../api/Api'
import { authenticatedApi } from '../api/authenticatedApi'
import JenkinsServersForm from '../components/JenkinsServersForm.vue'
import JenkinsServersList from '../components/JenkinsServersList.vue'

const toast = useToast()

// State
const servers = ref<JenkinsServersResponse[]>([])
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)

// Split panel state
const selectedServer = ref<JenkinsServersResponse | null>(null)
const isEditing = ref(false)
const isCreating = ref(false)

// Form data for right panel
const formData = computed(() => {
  if (selectedServer.value) {
    return {
      name: selectedServer.value.name || '',
      server: selectedServer.value.server || '',
      username: selectedServer.value.username || '',
      apiToken: '', // Can't populate for security
      instructions: selectedServer.value.instructions || '',
      responseFormat: selectedServer.value.responseFormat || ''
    }
  }
  return {
    name: '',
    server: '',
    username: '',
    apiToken: '',
    instructions: '',
    responseFormat: ''
  }
})

// Dialogs
const createDialogVisible = ref(false)
const deleteDialogVisible = ref(false)
const serverToDelete = ref<JenkinsServersResponse | null>(null)

// Data fetching
const fetchServers = async () => {
  loading.value = true
  try {
    const response = await authenticatedApi.api.v1JenkinsServersList({})
    servers.value = response.data || []

    // Auto-select first server if available
    if (servers.value.length > 0 && !selectedServer.value) {
      selectedServer.value = servers.value[0] || null
    }
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to load Jenkins servers',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// Server selection
const selectServer = (server: JenkinsServersResponse) => {
  selectedServer.value = server
  isEditing.value = false
  isCreating.value = false
}

// Edit mode toggles
const startEdit = () => {
  isEditing.value = true
}

const cancelEdit = () => {
  isEditing.value = false
  isCreating.value = false
}

const openCreateDialog = () => {
  createDialogVisible.value = true
}

const openDeleteDialog = (server: JenkinsServersResponse) => {
  serverToDelete.value = server
  deleteDialogVisible.value = true
}

const closeDeleteDialog = () => {
  deleteDialogVisible.value = false
  serverToDelete.value = null
}

const handleCreateServer = async ({ valid, values }: { valid: boolean, values: Record<string, unknown> }) => {
  if (!valid) return
  saving.value = true

  // Track existing server IDs before creation
  const existingIds = new Set(servers.value.map(c => c.id))

  try {
    const createData: CreateJenkinsServersRequest = {
      name: String(values.name).trim(),
      server: String(values.server).trim(),
      username: String(values.username).trim(),
      apiToken: String(values.apiToken).trim(),
      instructions: values.instructions ? String(values.instructions).trim() : undefined,
      responseFormat: values.responseFormat ? String(values.responseFormat).trim() : undefined
    }

    await authenticatedApi.api.v1JenkinsServersCreate(createData, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Jenkins server created successfully',
      life: 3000
    })

    createDialogVisible.value = false
    await fetchServers()

    // Auto-select the newly created server (find server not in existing IDs)
    const newServer = servers.value.find(server => server.id && !existingIds.has(server.id))
    if (newServer) {
      selectedServer.value = newServer
      isEditing.value = false
      isCreating.value = false
    }
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to save Jenkins server',
      life: 5000
    })
  } finally {
    saving.value = false
  }
}

// Form save handlers
const handleFormSave = async ({ valid, values }: { valid: boolean, values: Record<string, unknown> }) => {
  if (!valid) return

  if (isCreating.value && !selectedServer.value) {
    // Handle inline server creation in split panel
    return await handleCreateServer({ valid, values })
  } else if (selectedServer.value && isEditing.value) {
    // Update existing server
    saving.value = true
    try {
      const updateData: Partial<UpdateJenkinsServersRequest> = {}
      if (values.name && String(values.name).trim() !== selectedServer.value.name) {
        updateData.name = String(values.name).trim()
      }
      if (values.server && String(values.server).trim() !== selectedServer.value.server) {
        updateData.server = String(values.server).trim()
      }
      if (values.username && String(values.username).trim() !== selectedServer.value.username) {
        updateData.username = String(values.username).trim()
      }
      if (values.apiToken && String(values.apiToken).trim()) {
        updateData.apiToken = String(values.apiToken).trim()
      }
      if (values.instructions !== undefined) {
        const newInstructions = String(values.instructions).trim()
        if (newInstructions !== (selectedServer.value.instructions || '')) {
          updateData.instructions = newInstructions || undefined
        }
      }
      if (values.responseFormat !== undefined) {
        const newResponseFormat = String(values.responseFormat).trim()
        if (newResponseFormat !== (selectedServer.value.responseFormat || '')) {
          updateData.responseFormat = newResponseFormat || undefined
        }
      }

      if (Object.keys(updateData).length > 0) {
        await authenticatedApi.api.v1JenkinsServersUpdate(selectedServer.value.id!, updateData, {})
        toast.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Jenkins server updated successfully',
          life: 3000
        })
        await fetchServers()
      }

      isEditing.value = false
    } catch (error: unknown) {
      const err = error as { response?: { data?: { detail?: string } } }
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: err?.response?.data?.detail || 'Failed to save Jenkins server',
        life: 5000
      })
    } finally {
      saving.value = false
    }
  }
}

const deleteServer = async () => {
  if (!serverToDelete.value) return

  deleting.value = true

  try {
    await authenticatedApi.api.v1JenkinsServersDelete(serverToDelete.value.id!, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Jenkins server deleted successfully',
      life: 3000
    })

    // If deleted server was selected, deselect it
    if (selectedServer.value?.id === serverToDelete.value.id) {
      selectedServer.value = servers.value.length > 1 ? servers.value.find(s => s.id !== serverToDelete.value!.id) || null : null
    }

    closeDeleteDialog()
    await fetchServers()
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to delete Jenkins server',
      life: 5000
    })
  } finally {
    deleting.value = false
  }
}

onMounted(() => {
  fetchServers()
})
</script>
