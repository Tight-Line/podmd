<template>
  <div class="h-screen flex flex-col">
    <!-- Page Header -->
    <div class="flex justify-between items-center px-6 py-4 bg-white border-b border-slate-200">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Settings</h1>
        <p class="text-slate-600 mt-1">Manage your API keys and account preferences</p>
      </div>
    </div>

    <!-- Main Content -->
    <div class="flex-1 overflow-auto bg-slate-50">
      <!-- API Keys Section -->
      <div class="max-w-7xl mx-auto px-6 py-8">
        <div class="bg-white shadow-sm rounded-lg border border-slate-200 overflow-hidden">
          <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex justify-between items-center">
            <div>
              <h2 class="text-lg font-semibold text-slate-900">API Keys</h2>
              <p class="text-sm text-slate-600 mt-1">Generate and manage API keys for external access</p>
            </div>
            <Button
              @click="openCreateApiKeyDialog"
              icon="pi pi-plus"
              label="Create API Key"
              size="small"
              class="bg-slate-800 hover:bg-slate-700 text-white"
            />
          </div>

          <!-- API Keys List -->
          <ApiKeysManagement
            ref="apiKeysRef"
            class="p-6"
          />
        </div>
      </div>
    </div>

    <!-- Create API Key Dialog -->
    <Dialog
      v-model:visible="createApiKeyDialogVisible"
      header="Create New API Key"
      modal
      :style="{ width: '500px' }"
      :closable="true"
    >
      <ApiKeyFormDialog
        :visible="createApiKeyDialogVisible"
        @save="handleCreateApiKey"
        @cancel="closeCreateApiKeyDialog"
      />
    </Dialog>

    <!-- Display Generated API Key Modal -->
    <Dialog
      v-model:visible="showGeneratedKeyModal"
      header="API Key Created Successfully"
      modal
      :style="{ width: '600px' }"
      :closable="false"
      :closeOnEscape="false"
    >
      <div class="space-y-6">
        <div class="flex items-start gap-4">
          <i class="pi pi-check-circle text-green-500 text-2xl mt-1"></i>
          <div>
            <h3 class="text-lg font-semibold text-slate-900 mb-2">API Key Generated</h3>
            <p class="text-slate-600 text-sm mb-4">
              Your API key has been created successfully. <strong>Copy this key now as it will never be shown again.</strong>
            </p>
            <div class="bg-slate-50 border border-slate-300 rounded-lg p-4">
              <div class="flex items-center justify-between">
                <div class="flex-1">
                  <div class="text-xs text-slate-500 font-medium mb-1">API Key</div>
                  <div class="font-mono text-sm bg-white p-3 border rounded select-all break-all">
                    {{ generatedApiKey }}
                  </div>
                </div>
                <Button
                  @click="copyDisplayedKey"
                  icon="pi pi-copy"
                  label="Copy Key"
                  severity="primary"
                  class="ml-4"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="flex justify-end pt-4 border-t border-slate-200">
          <Button
            @click="closeGeneratedKeyModal"
            label="Done"
            severity="primary"
          />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import ApiKeysManagement from '../components/ApiKeysManagement.vue'
import ApiKeyFormDialog from '../components/ApiKeyFormDialog.vue'

const toast = useToast()

// Dialog state
const createApiKeyDialogVisible = ref(false)
const showGeneratedKeyModal = ref(false)

// Component refs
const apiKeysRef = ref<InstanceType<typeof ApiKeysManagement> | null>(null)

// State for generated key display
const generatedApiKey = ref<string>('')

// Dialog handlers
const openCreateApiKeyDialog = () => {
  createApiKeyDialogVisible.value = true
}

const closeCreateApiKeyDialog = () => {
  createApiKeyDialogVisible.value = false
}

const handleCreateApiKey = (apiKeyData: { id: string; key: string; [key: string]: any }) => {
  createApiKeyDialogVisible.value = false

  // Store the generated key for display
  generatedApiKey.value = apiKeyData.key
  showGeneratedKeyModal.value = true

  // Refresh the API keys list
  if (apiKeysRef.value) {
    apiKeysRef.value.loadApiKeys()
  }
}

const copyDisplayedKey = async () => {
  if (generatedApiKey.value) {
    try {
      await navigator.clipboard.writeText(generatedApiKey.value)
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
}

const closeGeneratedKeyModal = () => {
  showGeneratedKeyModal.value = false
  generatedApiKey.value = ''
}
</script>
