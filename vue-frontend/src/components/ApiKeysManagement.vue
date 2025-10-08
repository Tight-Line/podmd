<template>
  <div>
    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center items-center py-12">
      <ProgressSpinner />
      <span class="ml-3 text-slate-600">Loading API keys...</span>
    </div>

    <!-- Empty State -->
    <div v-else-if="apiKeys.length === 0" class="text-center py-12">
      <i class="pi pi-key text-slate-400 text-4xl block mb-4"></i>
      <h3 class="text-lg font-medium text-slate-900 mb-2">No API keys found</h3>
      <p class="text-slate-600 mb-6">Create your first API key to get started with external access</p>
      <Button
        @click="$emit('create')"
        icon="pi pi-plus"
        label="Create API Key"
        severity="secondary"
        outlined
      />
    </div>

    <!-- API Keys List -->
    <div v-else>
      <DataTable
        :value="apiKeys"
        :loading="loading"
        class="p-datatable-sm"
        stripedRows
        showGridlines
        responsiveLayout="scroll"
        :paginator="apiKeys.length > 10"
        :rows="10"
        :rowsPerPageOptions="[5, 10, 20]"
      >


        <Column field="permissions" header="Permissions" style="width: 150px">
          <template #body="{ data }">
            <div class="flex flex-wrap gap-1">
              <Tag v-if="parsePermissions(data.permissions)?.read" value="Read" severity="info" class="text-xs px-2 py-0.5" />
              <Tag v-if="parsePermissions(data.permissions)?.write" value="Write" severity="warning" class="text-xs px-2 py-0.5" />
              <Tag v-if="parsePermissions(data.permissions)?.admin" value="Admin" severity="success" class="text-xs px-2 py-0.5" />
            </div>
          </template>
        </Column>

        <Column field="usageCount" header="Usage" style="width: 100px">
          <template #body="{ data }">
            <span class="text-sm">
              {{ data.usageCount || 0 }}
              <span v-if="data.usageLimit" class="text-slate-500">/{{ data.usageLimit }}</span>
            </span>
          </template>
        </Column>

        <Column field="status" header="Status" style="width: 100px">
          <template #body="{ data }">
            <Tag
              :value="formatStatus(data.status)"
              :severity="getStatusSeverity(data.status)"
              class="text-xs"
            />
          </template>
        </Column>

        <Column field="lastUsedAt" header="Last Used" style="width: 140px">
          <template #body="{ data }">
            <div v-if="data.lastUsedAt" class="text-sm text-slate-600">
              {{ formatDate(data.lastUsedAt) }}
            </div>
            <span v-else class="text-xs text-slate-400 italic">Never</span>
          </template>
        </Column>

        <Column field="createdAt" header="Created" style="width: 140px">
          <template #body="{ data }">
            <div class="text-sm text-slate-600">
              {{ formatDate(data.createdAt) }}
            </div>
          </template>
        </Column>

        <Column header="Actions" style="width: 120px">
          <template #body="{ data }">
            <div class="flex gap-1">
              <Button
                @click="editApiKey(data)"
                icon="pi pi-pencil"
                size="small"
                severity="secondary"
                v-tooltip="'Edit API key'"
                class="p-1"
              />
              <Button
                @click="deleteApiKey(data)"
                icon="pi pi-trash"
                size="small"
                severity="danger"
                v-tooltip="'Delete API key'"
                class="p-1"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <!-- Edit API Key Dialog -->
    <Dialog
      v-model:visible="editDialogVisible"
      header="Edit API Key"
      modal
      :style="{ width: '500px' }"
      :closable="true"
    >
      <ApiKeyFormDialog
        v-if="selectedApiKey"
        :visible="editDialogVisible"
        :apiKey="selectedApiKey"
        @save="handleEditApiKey"
        @cancel="closeEditDialog"
      />
    </Dialog>

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialogVisible"
      header="Delete API Key"
      modal
      :style="{ width: '400px' }"
    >
      <div class="flex items-center gap-3">
        <i class="pi pi-exclamation-triangle text-amber-500 text-xl"></i>
        <div>
          <p class="font-medium text-slate-900 mb-1">Delete API Key</p>
          <p class="text-slate-600 text-sm">
            Are you sure you want to delete this API key? This action cannot be undone.
          </p>
          <div class="mt-3 p-2 bg-slate-50 rounded text-xs font-mono text-slate-700 border">
            pk_live_{{ selectedApiKey?.id ? maskApiKey(selectedApiKey.id) : '' }}
          </div>
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
          @click="confirmDeleteApiKey"
          :loading="deleting"
          label="Delete API Key"
          severity="danger"
          icon="pi pi-trash"
          size="small"
        />
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, defineEmits } from 'vue'
import { useToast } from 'primevue/usetoast'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import ProgressSpinner from 'primevue/progressspinner'
import { authenticatedApi } from '../api/authenticatedApi'
import ApiKeyFormDialog from './ApiKeyFormDialog.vue'
import type { ApiKeyDto } from '../api/Api'

const emit = defineEmits<{
  create: []
}>()

const toast = useToast()

// State
const apiKeys = ref<ApiKeyDto[]>([])
const loading = ref(false)
const deleting = ref(false)

// Dialog state
const editDialogVisible = ref(false)
const deleteDialogVisible = ref(false)
const selectedApiKey = ref<ApiKeyDto | null>(null)

// Methods
const maskApiKey = (id: string): string => {
  return '****' + id.substring(id.length - 4)
}

const parsePermissions = (permissionsJson: string | null | undefined): { read?: boolean; write?: boolean; admin?: boolean } | null => {
  try {
    if (!permissionsJson) return null
    return JSON.parse(permissionsJson)
  } catch {
    return null
  }
}

const copyToClipboard = async (id: string) => {
  try {
    await navigator.clipboard.writeText(id)
    toast.add({
      severity: 'success',
      summary: 'Copied',
      detail: 'API key copied to clipboard',
      life: 2000
    })
  } catch (err) {
    console.error('Failed to copy:', err)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to copy API key',
      life: 3000
    })
  }
}

const getPermissionSeverity = (permissions?: string | null): string => {
  if (!permissions || permissions.toLowerCase().includes('read-only')) {
    return 'info'
  }
  if (permissions.toLowerCase().includes('write')) {
    return 'warning'
  }
  return 'success'
}

const formatStatus = (status: string): string => {
  switch (status.toLowerCase()) {
    case 'active':
      return 'Active'
    case 'disabled':
      return 'Disabled'
    case 'expired':
      return 'Expired'
    default:
      return status
  }
}

const getStatusSeverity = (status: string): string => {
  switch (status.toLowerCase()) {
    case 'active':
      return 'success'
    case 'disabled':
      return 'neutral'
    case 'expired':
      return 'danger'
    default:
      return 'neutral'
  }
}

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString()
}

// API methods
const loadApiKeys = async () => {
  loading.value = true
  try {
    const response = await authenticatedApi.api.apikeysList({})
    apiKeys.value = response.data || []
  } catch (error) {
    console.error('Error loading API keys:', error)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to load API keys',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// Event handlers
const editApiKey = (apiKey: ApiKeyDto) => {
  selectedApiKey.value = apiKey
  editDialogVisible.value = true
}

const deleteApiKey = (apiKey: ApiKeyDto) => {
  selectedApiKey.value = apiKey
  deleteDialogVisible.value = true
}

const closeEditDialog = () => {
  editDialogVisible.value = false
  selectedApiKey.value = null
}

const closeDeleteDialog = () => {
  deleteDialogVisible.value = false
  selectedApiKey.value = null
}

const handleEditApiKey = async (updatedApiKey: any) => {
  closeEditDialog()
  await loadApiKeys()
  toast.add({
    severity: 'success',
    summary: 'Success',
    detail: 'API key updated successfully',
    life: 3000
  })
}

const confirmDeleteApiKey = async () => {
  if (!selectedApiKey.value?.id) return

  deleting.value = true
  try {
    await authenticatedApi.api.apikeysDelete(selectedApiKey.value.id, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'API key deleted successfully',
      life: 3000
    })
    closeDeleteDialog()
    await loadApiKeys()
  } catch (error: any) {
    console.error('Error deleting API key:', error)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.response?.data?.detail || 'Failed to delete API key',
      life: 5000
    })
  } finally {
    deleting.value = false
  }
}

// Load data on mount
onMounted(() => {
  loadApiKeys()
})

// Expose load method for parent components
defineExpose({
  loadApiKeys
})
</script>
