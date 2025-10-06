<template>
  <div class="h-full flex flex-col">
    <!-- Loading State -->
    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <i class="pi pi-spin pi-spinner text-slate-400 text-2xl mb-3 block"></i>
        <p class="text-slate-600 text-sm">Loading knowledge bases...</p>
      </div>
    </div>

    <!-- Knowledge Bases List -->
    <div v-else class="flex-1 overflow-y-auto">

      <!-- No Knowledge Bases Message -->
      <div v-if="knowledgeBases.length === 0" class="flex-1 flex items-center justify-center p-8">
        <div class="text-center">
          <i class="pi pi-database text-slate-400 text-3xl mb-4 block"></i>
          <h4 class="text-lg font-medium text-slate-900 mb-2">No knowledge bases yet</h4>
          <p class="text-slate-500 text-sm mb-4">Create your first knowledge base to get started with document storage</p>
          <Button
            @click="$emit('add-knowledgeBase')"
            icon="pi pi-plus"
            label="Create Knowledge Base"
            severity="secondary"
            size="small"
          />
        </div>
      </div>

      <!-- Knowledge Bases Grid -->
      <div v-else class="divide-y divide-slate-200">
        <div
          v-for="knowledgeBase in knowledgeBases"
          :key="knowledgeBase.id"
          :class="[
            'p-4 cursor-pointer hover:bg-slate-50 transition-colors',
            selectedKnowledgeBase?.id === knowledgeBase.id ? 'bg-blue-50' : ''
          ]"
          @click="selectKnowledgeBase(knowledgeBase)"
        >
          <div class="flex items-center justify-between">
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-1">
                <i class="pi pi-database text-slate-400 text-sm"></i>
                <h3 class="text-sm font-medium text-slate-900 truncate">{{ knowledgeBase.name }}</h3>
              </div>
              <div v-if="knowledgeBase.createdAt" class="text-xs text-slate-600 flex items-center gap-1">
                <i class="pi pi-calendar text-xs text-slate-400"></i>
                <span>{{ formatDate(knowledgeBase.createdAt) }}</span>
              </div>
            </div>

            <!-- Delete button -->
            <Button
              icon="pi pi-trash"
              severity="danger"
              size="small"
              text
              rounded
              @click.stop="handleDelete(knowledgeBase)"
              aria-label="Delete knowledge base"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import type { KnowledgeBaseDto } from '../api/Api'
import { formatDate } from '../utils/formatters'

interface Props {
  knowledgeBases: KnowledgeBaseDto[]
  loading: boolean
  selectedKnowledgeBase: KnowledgeBaseDto | null
}

defineProps<Props>()

const emit = defineEmits<{
  selectKnowledgeBase: [knowledgeBase: KnowledgeBaseDto]
  'add-knowledgeBase': []
  deleteKnowledgeBase: [knowledgeBase: KnowledgeBaseDto]
}>()

// Methods
const selectKnowledgeBase = (knowledgeBase: KnowledgeBaseDto) => {
  emit('selectKnowledgeBase', knowledgeBase)
}

const handleDelete = (knowledgeBase: KnowledgeBaseDto) => {
  emit('deleteKnowledgeBase', knowledgeBase)
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
