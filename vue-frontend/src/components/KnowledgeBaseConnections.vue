<template>
  <div v-if="sourceId">
    <!-- Connected Knowledge Bases List -->
    <div class="space-y-3">
      <div
        v-for="kb in knowledgeBases"
        :key="kb.id"
        class="flex items-center justify-between bg-white border border-slate-200 rounded-md px-4 py-3 hover:shadow-sm transition-colors"
      >
        <!-- Knowledge Base Info -->
        <div class="flex items-center gap-3 flex-1 min-w-0">
          <div class="flex-shrink-0">
            <i class="pi pi-book text-slate-400 text-lg"></i>
          </div>
          <div class="flex-1 min-w-0">
            <h5 class="text-sm font-medium text-slate-900 truncate">{{ kb.name }}</h5>
            <p v-if="kb.description" class="text-xs text-slate-600 mt-1 line-clamp-2">{{ kb.description }}</p>
          </div>
        </div>

        <!-- Remove Button - Only show when editing -->
        <div v-if="!readOnly" class="flex-shrink-0">
          <Button
            @click="confirmRemoveKnowledgeBase(kb)"
            icon="pi pi-minus"
            severity="danger"
            text
            size="small"
            v-tooltip="'Remove knowledge base association'"
            class="p-1"
          />
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="knowledgeBases.length === 0 && !loadingKnowledgeBases" class="text-center py-8">
      <i class="pi pi-book text-slate-400 text-4xl mb-3 block"></i>
      <h5 class="text-lg font-medium text-slate-900 mb-2">No knowledge bases connected</h5>
      <p class="text-slate-600">Connect knowledge bases to enhance AI analysis with contextual information</p>
    </div>

    <!-- Add Knowledge Base Dialog -->
    <Dialog
      v-model:visible="addKnowledgeBaseDialogVisible"
      header="Add Knowledge Base"
      modal
      :style="{ width: '600px', maxWidth: '90vw' }"
      :closable="true"
    >
      <div class="space-y-4">
        <p class="text-sm text-slate-600">
          Select a knowledge base to connect with this {{ sourceType }}. Only knowledge bases that are not already connected will be shown.
        </p>

        <!-- Knowledge Base Selection -->
        <div class="space-y-2">
          <label class="block text-sm font-medium text-slate-700">Available Knowledge Bases</label>
          <Listbox
            v-model="knowledgeBaseToAdd"
            :options="availableKnowledgeBases"
            optionLabel="name"
            optionValue="id"
            :loading="loadingAllKnowledgeBases"
            :emptyMessage="loadingAllKnowledgeBases ? 'Loading knowledge bases...' : 'No available knowledge bases to connect'"
            class="w-full"
            style="max-height: 300px;"
            :pt="{
              root: { class: 'border-slate-300 focus:border-blue-500' },
              option: { class: 'hover:bg-slate-50' }
            }"
          >
            <template #option="slotProps">
              <div class="flex items-center gap-3 py-2">
                <i class="pi pi-book text-slate-400 text-lg"></i>
                <div class="flex-1">
                  <div class="font-medium text-slate-900">{{ slotProps.option.name }}</div>
                  <div v-if="slotProps.option.description" class="text-xs text-slate-600 mt-1 line-clamp-1">
                    {{ slotProps.option.description }}
                  </div>
                </div>
              </div>
            </template>
          </Listbox>
        </div>
      </div>

      <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-200">
        <Button
          @click="cancelAddKnowledgeBase"
          label="Cancel"
          severity="secondary"
          size="small"
        />
        <Button
          @click="addKnowledgeBaseAssociation"
          :loading="associatingKnowledgeBase"
          :disabled="!knowledgeBaseToAdd"
          label="Add Knowledge Base"
          severity="primary"
          icon="pi pi-plus"
          size="small"
        />
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Listbox from 'primevue/listbox'
import type { KnowledgeBaseDto } from '../api/Api'
import { authenticatedApi } from '../api/authenticatedApi'

const props = defineProps<{
  sourceId: string
  sourceType: 'cluster' | 'jenkins-server'
  isEditing: boolean
  readOnly: boolean
}>()

// Knowledge bases state
const toast = useToast()
const knowledgeBases = ref<KnowledgeBaseDto[]>([])
const loadingKnowledgeBases = ref(false)
const allKnowledgeBases = ref<KnowledgeBaseDto[]>([])
const loadingAllKnowledgeBases = ref(false)
const addKnowledgeBaseDialogVisible = ref(false)
const associatingKnowledgeBase = ref(false)
const knowledgeBaseToAdd = ref<string | null>(null)

// Knowledge Base Management Methods
const availableKnowledgeBases = computed(() => {
  const connectedIds = knowledgeBases.value.map(kb => kb.id)
  return allKnowledgeBases.value.filter(kb => !connectedIds.includes(kb.id))
})

const fetchKnowledgeBases = async () => {
  if (!props.sourceId) return

  loadingKnowledgeBases.value = true
  try {
    const response = await authenticatedApi.api.v1SourcesKnowledgeBasesList(props.sourceId, {})
    knowledgeBases.value = response.data || []
  } catch {
    knowledgeBases.value = []
  } finally {
    loadingKnowledgeBases.value = false
  }
}

const fetchAllKnowledgeBases = async () => {
  loadingAllKnowledgeBases.value = true
  try {
    const response = await authenticatedApi.api.v1KnowledgeBasesList({})
    allKnowledgeBases.value = response.data || []
  } catch {
    allKnowledgeBases.value = []
  } finally {
    loadingAllKnowledgeBases.value = false
  }
}

const openAddKnowledgeBaseDialog = async () => {
  knowledgeBaseToAdd.value = null
  addKnowledgeBaseDialogVisible.value = true
  await fetchAllKnowledgeBases()
}

const cancelAddKnowledgeBase = () => {
  addKnowledgeBaseDialogVisible.value = false
  knowledgeBaseToAdd.value = null
}

const addKnowledgeBaseAssociation = async () => {
  const selectedKb = availableKnowledgeBases.value.find(kb => kb.id === knowledgeBaseToAdd.value)
  if (!selectedKb || !props.sourceId) return

  associatingKnowledgeBase.value = true
  try {
    await authenticatedApi.api.v1SourcesKnowledgeBasesCreate(
      props.sourceId,
      selectedKb.id!.toString(),
      {}
    )

    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Knowledge base connected successfully',
      life: 3000
    })

    await fetchKnowledgeBases()
    addKnowledgeBaseDialogVisible.value = false
    knowledgeBaseToAdd.value = null
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to connect knowledge base',
      life: 5000
    })
  } finally {
    associatingKnowledgeBase.value = false
  }
}

const confirmRemoveKnowledgeBase = async (knowledgeBase: KnowledgeBaseDto) => {
  if (!props.sourceId || !knowledgeBase.id) return

  try {
    await authenticatedApi.api.v1SourcesKnowledgeBasesDelete(
      props.sourceId,
      knowledgeBase.id.toString(),
      {}
    )

    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Knowledge base disconnected successfully',
      life: 3000
    })

    await fetchKnowledgeBases()
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to disconnect knowledge base',
      life: 5000
    })
  }
}

// Watch for source changes to load knowledge bases
watch(() => props.sourceId, (newId, oldId) => {
  if (newId && (!oldId || newId !== oldId)) {
    // Load knowledge bases for the new source
    fetchKnowledgeBases()
  } else if (!newId) {
    // Clear knowledge bases when no source is selected
    knowledgeBases.value = []
  }
})

onMounted(() => {
  // Load knowledge bases immediately if we have a source ID
  if (props.sourceId) {
    fetchKnowledgeBases()
  }
})

// Expose actions for parent components to use
defineExpose({
  openAddKnowledgeBaseDialog
})
</script>
