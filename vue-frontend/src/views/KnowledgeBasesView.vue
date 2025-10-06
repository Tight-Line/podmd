<template>
  <div class="h-screen flex flex-col">
    <!-- Page Header -->
    <div class="flex justify-between items-center px-6 py-4 bg-white border-b border-slate-200">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Knowledge Bases</h1>
        <p class="text-slate-600 mt-1">Manage your knowledge bases for log analysis</p>
      </div>
      <Button
        @click="openCreateDialog"
        icon="pi pi-plus"
        label="Create Knowledge Base"
        severity="primary"
      />
    </div>

    <!-- Main Content - Split Panel -->
    <div class="flex-1 overflow-hidden">
      <Splitter :minSizes="[25, 35]" :gutterSize="8" class="h-full">
        <!-- Left Panel - Knowledge Bases List -->
        <SplitterPanel :size="30" :minSize="25">
          <div class="h-full bg-white border-r border-slate-200">
            <KnowledgeBasesList
              :knowledgeBases="knowledgeBases"
              :loading="loading"
              :selectedKnowledgeBase="selectedKnowledgeBase"
              @select-knowledgeBase="selectKnowledgeBase"
              @add-knowledgeBase="openCreateDialog"
              @delete-knowledgeBase="openDeleteDialog"
            />
          </div>
        </SplitterPanel>

        <!-- Right Panel - Knowledge Base Details -->
        <SplitterPanel :size="70" :minSize="35">
          <div class="h-full bg-white">
            <div v-if="selectedKnowledgeBase || isCreating" class="h-full flex flex-col">
              <div class="p-4 border-b border-slate-200 flex items-center justify-between">
                <h2 class="text-lg font-semibold text-slate-900">
                  {{ isCreating ? 'Create Knowledge Base' : isEditing ? 'Edit Knowledge Base' : 'Knowledge Base Details' }}
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
                <KnowledgeBasesForm
                  v-if="selectedKnowledgeBase || isCreating"
                  :key="selectedKnowledgeBase?.id || 'creating'"
                  :initial-values="formData"
                  :is-editing="isEditing || isCreating"
                  :read-only="!isEditing && !isCreating"
                  :saving="saving"
                  @save-knowledgeBase="handleFormSave"
                />
              </div>
            </div>
            <div v-else class="h-full flex items-center justify-center">
              <div class="text-center">
                <i class="pi pi-database text-slate-400 text-4xl mb-4 block"></i>
                <h3 class="text-lg font-medium text-slate-900 mb-2">Select a knowledge base to view details</h3>
                <p class="text-slate-600">Choose a knowledge base from the left panel to see its configuration</p>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>

    <!-- Create Knowledge Base Dialog -->
    <Dialog
      v-model:visible="createDialogVisible"
      header="Create New Knowledge Base"
      modal
      :style="{ width: '600px' }"
      :closable="true"
    >
      <KnowledgeBasesForm
        :is-editing="true"
        :read-only="false"
        :saving="saving"
        :initial-values="{
          name: '',
          description: ''
        }"
        @save-knowledgeBase="handleCreateKnowledgeBase"
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
          <p class="font-medium text-slate-900 mb-1">Delete Knowledge Base</p>
          <p class="text-slate-600 text-sm">
            Are you sure you want to delete the knowledge base <strong>{{ knowledgeBaseToDelete?.name }}</strong>?
            This will also remove all associated files. This action cannot be undone.
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
          @click="deleteKnowledgeBase"
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
import type { KnowledgeBaseDto, CreateKnowledgeBaseDto, UpdateKnowledgeBaseDto } from '../api/Api'
import { authenticatedApi } from '../api/authenticatedApi'
import KnowledgeBasesForm from '../components/KnowledgeBasesForm.vue'
import KnowledgeBasesList from '../components/KnowledgeBasesList.vue'

const toast = useToast()

// State
const knowledgeBases = ref<KnowledgeBaseDto[]>([])
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)

// Split panel state
const selectedKnowledgeBase = ref<KnowledgeBaseDto | null>(null)
const isEditing = ref(false)
const isCreating = ref(false)

// Form data for right panel
const formData = computed(() => {
  if (selectedKnowledgeBase.value) {
    return {
      id: selectedKnowledgeBase.value.id,
      name: selectedKnowledgeBase.value.name || '',
      description: selectedKnowledgeBase.value.description || ''
    }
  }
  return {
    name: '',
    description: ''
  }
})

// Dialogs
const createDialogVisible = ref(false)
const deleteDialogVisible = ref(false)
const knowledgeBaseToDelete = ref<KnowledgeBaseDto | null>(null)

// Data fetching
const fetchKnowledgeBases = async () => {
  loading.value = true
  try {
    const response = await authenticatedApi.api.v1KnowledgeBasesList({})
    knowledgeBases.value = response.data || []

    // Auto-select first knowledge base if available
    if (knowledgeBases.value.length > 0 && !selectedKnowledgeBase.value) {
      selectedKnowledgeBase.value = knowledgeBases.value[0] || null
    }
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to load knowledge bases',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// Knowledge base selection
const selectKnowledgeBase = (knowledgeBase: KnowledgeBaseDto) => {
  selectedKnowledgeBase.value = knowledgeBase
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

const openDeleteDialog = (knowledgeBase: KnowledgeBaseDto) => {
  knowledgeBaseToDelete.value = knowledgeBase
  deleteDialogVisible.value = true
}

const closeDeleteDialog = () => {
  deleteDialogVisible.value = false
  knowledgeBaseToDelete.value = null
}

const handleCreateKnowledgeBase = async ({ valid, values }: { valid: boolean, values: Record<string, unknown> }) => {
  if (!valid) return
  saving.value = true

  // Track existing knowledge base IDs before creation
  const existingIds = new Set(knowledgeBases.value.map(kb => kb.id))

  try {
    const createData: CreateKnowledgeBaseDto = {
      name: String(values.name).trim(),
      description: values.description ? String(values.description).trim() : undefined
    }

    await authenticatedApi.api.v1KnowledgeBasesCreate(createData, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Knowledge base created successfully',
      life: 3000
    })

    createDialogVisible.value = false
    await fetchKnowledgeBases()

    // Auto-select the newly created knowledge base (find knowledge base not in existing IDs)
    const newKnowledgeBase = knowledgeBases.value.find(kb => kb.id && !existingIds.has(kb.id))
    if (newKnowledgeBase) {
      selectedKnowledgeBase.value = newKnowledgeBase
      isEditing.value = false
      isCreating.value = false
    }
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to save knowledge base',
      life: 5000
    })
  } finally {
    saving.value = false
  }
}

// Form save handlers
const handleFormSave = async ({ valid, values }: { valid: boolean, values: Record<string, unknown> }) => {
  if (!valid) return

  if (isCreating.value && !selectedKnowledgeBase.value) {
    // Handle inline knowledge base creation in split panel
    return await handleCreateKnowledgeBase({ valid, values })
  } else if (selectedKnowledgeBase.value && isEditing.value) {
    // Update existing knowledge base
    saving.value = true
    try {
      const updateData: UpdateKnowledgeBaseDto = {
        id: selectedKnowledgeBase.value.id!
      }

      if (values.name && String(values.name).trim() !== selectedKnowledgeBase.value.name) {
        updateData.name = String(values.name).trim()
      }

      if (values.description !== undefined) {
        const newDescription = values.description ? String(values.description).trim() : ''
        if (newDescription !== (selectedKnowledgeBase.value.description || '')) {
          updateData.description = newDescription || undefined
        }
      }

      if (Object.keys(updateData).length > 1) { // More than just id
        await authenticatedApi.api.v1KnowledgeBasesUpdate(selectedKnowledgeBase.value.id!, updateData, {})
        toast.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Knowledge base updated successfully',
          life: 3000
        })
        await fetchKnowledgeBases()
      }

      isEditing.value = false
    } catch (error: unknown) {
      const err = error as { response?: { data?: { detail?: string } } }
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: err?.response?.data?.detail || 'Failed to save knowledge base',
        life: 5000
      })
    } finally {
      saving.value = false
    }
  }
}

const deleteKnowledgeBase = async () => {
  if (!knowledgeBaseToDelete.value) return

  deleting.value = true

  try {
    await authenticatedApi.api.v1KnowledgeBasesDelete(knowledgeBaseToDelete.value.id!, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Knowledge base deleted successfully',
      life: 3000
    })

    // If deleted knowledge base was selected, deselect it
    if (selectedKnowledgeBase.value?.id === knowledgeBaseToDelete.value.id) {
      selectedKnowledgeBase.value = knowledgeBases.value.length > 1 ? knowledgeBases.value.find(kb => kb.id !== knowledgeBaseToDelete.value!.id) || null : null
    }

    closeDeleteDialog()
    await fetchKnowledgeBases()
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to delete knowledge base',
      life: 5000
    })
  } finally {
    deleting.value = false
  }
}

onMounted(() => {
  fetchKnowledgeBases()
})
</script>
