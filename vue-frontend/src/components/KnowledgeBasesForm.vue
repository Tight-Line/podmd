<template>
  <div class="h-full">
    <form @submit.prevent="handleSubmit" class="h-full flex flex-col">
      <!-- Basic Form Fields -->
      <div class="flex-1 overflow-y-auto space-y-6 px-4">
        <!-- Name Field -->
        <div class="space-y-1">
          <label for="name" class="block text-sm font-medium text-slate-900">
            Name <span class="text-red-500">*</span>
          </label>
          <InputText
            id="name"
            v-model="form.name"
            :readonly="readOnly"
            :class="{ 'bg-slate-50': readOnly }"
            class="w-full"
            placeholder="Enter knowledge base name"
            :pt="{ root: { class: 'border-slate-300 focus:border-blue-500' } }"
          />
          <small class="text-slate-500">A descriptive name for your knowledge base</small>
        </div>

        <!-- Description Field -->
        <div class="space-y-1">
          <label for="description" class="block text-sm font-medium text-slate-900">
            Description
          </label>
          <Textarea
            id="description"
            v-model="form.description"
            :readonly="readOnly"
            :class="{ 'bg-slate-50': readOnly }"
            class="w-full"
            placeholder="Enter a description for your knowledge base"
            rows="3"
            :pt="{ root: { class: 'border-slate-300 focus:border-blue-500 resize-none' } }"
          />
          <small class="text-slate-500">Optional description to help identify the knowledge base</small>
        </div>

        <!-- File Management Section - Only show when editing existing KB -->
        <div v-if="!isCreating && initialValues.id" class="space-y-4">
          <div class="border-t border-slate-200 pt-6">
            <h3 class="text-lg font-semibold text-slate-900 mb-4">Files</h3>

            <!-- File List Component -->
            <div class="space-y-4">
              <div v-if="!loadingFiles && existingFiles.length > 0" class="space-y-1">
                <div
                  v-for="file in existingFiles"
                  :key="file.id"
                  class="bg-white border border-slate-200 rounded-md px-3 py-2 hover:shadow-sm transition-colors"
                >
                  <!-- File Info and Actions -->
                  <div class="flex items-center justify-between">
                    <!-- File Info -->
                    <div class="flex items-center gap-3 flex-1 min-w-0">
                      <div class="flex-shrink-0">
                        <i class="pi pi-file text-slate-400 text-lg"></i>
                      </div>
                      <div class="flex-1 min-w-0">
                        <h5 class="text-sm font-medium text-slate-900 truncate">{{ file.fileName }}</h5>
                        <div class="flex items-center gap-4 text-xs text-slate-500 mt-1">
                          <span>{{ formatFileSize(file.fileSize || 0) }}</span>
                          <span>{{ getFileTypeDisplay(file.contentType || '') }}</span>
                          <span v-if="file.createdAt">Uploaded {{ formatDate(file.createdAt) }}</span>
                        </div>
                      </div>
                    </div>

                    <!-- Actions -->
                    <div class="flex items-center gap-2 flex-shrink-0">
                      <!-- Replace FileUpload -->
                      <FileUpload
                        mode="basic"
                        name="file"
                        :multiple="false"
                        accept=".pdf,.txt,.json,.md,.doc,.docx"
                        :maxFileSize="10485760"
                        customUpload
                        @uploader="(event) => handleFileReplace(event, file)"
                        :disabled="!!replacingFile"
                        auto
                        chooseLabel=" "
                        chooseIcon="pi pi-upload"
                        class="text-slate-500 hover:text-slate-700 p-1 rounded transition-colors border-0 bg-transparent hover:bg-slate-100"
                      />
                      <!-- Delete Button -->
                      <Button
                        @click="confirmDeleteFile(file)"
                        icon="pi pi-trash"
                        severity="danger"
                        text
                        size="small"
                        v-tooltip="'Delete file'"
                        class="p-1"
                        :disabled="!!replacingFile"
                      />
                    </div>
                  </div>

                  <!-- Replacement Progress (shows only for the file being replaced) -->
                  <div v-if="replacingFile && replacingFile.id === file.id" class="mt-2">
                    <div class="bg-blue-50 rounded-lg p-2 border border-blue-200">
                      <div class="flex items-center gap-2">
                        <i class="pi pi-spin pi-spinner text-blue-600 text-sm"></i>
                        <span class="text-sm text-blue-900">Replacing file...</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Empty State -->
              <div v-else-if="!loadingFiles" class="text-center py-8">
                <i class="pi pi-files text-slate-400 text-4xl mb-3 block"></i>
                <h5 class="text-lg font-medium text-slate-900 mb-2">No files uploaded yet</h5>
                <p class="text-slate-600">Upload your first file to get started with document storage</p>
              </div>

              <!-- Loading State for Files -->
              <div v-else class="text-center py-8">
                <i class="pi pi-spin pi-spinner text-slate-400 text-2xl mb-3 block"></i>
                <p class="text-slate-600">Loading files...</p>
              </div>

              <!-- File Upload Component -->
              <KnowledgeBaseFileUpload
                :knowledge-base-id="initialValues.id!"
                @file-uploaded="handleFileUploaded"
              />
            </div>
          </div>
        </div>

        <!-- Help Text for Creating -->
        <div v-if="isCreating" class="rounded-md bg-blue-50 p-4 border border-blue-200">
          <div class="flex">
            <i class="pi pi-info-circle text-blue-400 mt-0.5 mr-3"></i>
            <div class="text-sm text-blue-700">
              <p class="font-medium mb-1">Create your knowledge base first</p>
              <p>File upload will be available after the knowledge base is created.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div v-if="!readOnly" class="flex justify-between items-center pt-6 border-t border-slate-200 mt-6 px-4">
        <!-- Upload Files button - show for existing KB -->
        <div v-if="!isCreating && initialValues.id">
          <Button
            @click="openUploadDialog"
            icon="pi pi-upload"
            label="Upload Files"
            severity="secondary"
            size="small"
          />
        </div>
        <div v-else></div> <!-- Spacer -->

        <!-- Save button -->
        <Button
          type="submit"
          :label="saving ? 'Saving...' : 'Save'"
          severity="primary"
          :loading="saving"
          :disabled="!isValid"
        />
      </div>
    </form>

    <!-- File Upload Dialog -->
    <Dialog
      v-model:visible="uploadDialogVisible"
      header="Upload Files"
      modal
      :style="{ width: '700px' }"
      :closable="true"
    >
      <KnowledgeBaseFileUpload
        :knowledge-base-id="initialValues.id!"
        @file-uploaded="handleFileUploaded"
        :show-upload-area="true"
        :show-file-list="false"
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
          <p class="font-medium text-slate-900 mb-1">Delete File</p>
          <p class="text-slate-600 text-sm">
            Are you sure you want to delete <strong>{{ fileToDelete?.fileName }}</strong>?
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
          @click="deleteFile"
          :loading="deletingFile"
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
import { ref, computed, onMounted, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useVuelidate } from '@vuelidate/core'
import { required, maxLength } from '@vuelidate/validators'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import FileUpload from 'primevue/fileupload'
import KnowledgeBaseFileUpload from './KnowledgeBaseFileUpload.vue'
import type { KnowledgeFileDto } from '../api/Api'
import { authenticatedApi } from '../api/authenticatedApi'
import { formatDate, formatFileSize, getFileTypeDisplay } from '../utils/formatters'

const toast = useToast()

interface Props {
  initialValues: {
    id?: string
    name?: string
    description?: string
  }
  isEditing: boolean
  readOnly: boolean
  saving: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'save-knowledgeBase': [payload: { valid: boolean, values: Record<string, unknown> }]
}>()

// Form state
const form = ref({
  name: props.initialValues.name || '',
  description: props.initialValues.description || ''
})

// Validation rules
const rules = computed(() => ({
  name: { required, maxLength: maxLength(100) },
  description: { maxLength: maxLength(1000) }
}))

const v$ = useVuelidate(rules, form)

// Computed properties
const isValid = computed(() => !v$.value.$invalid)
const isCreating = computed(() => props.isEditing && !props.initialValues.id)

// Watch for changes to initialValues
watch(() => props.initialValues, (newValues) => {
  form.value = {
    name: newValues.name || '',
    description: newValues.description || ''
  }
}, { deep: true })

// State
const uploadDialogVisible = ref(false)
const loadingFiles = ref(false)
const existingFiles = ref<KnowledgeFileDto[]>([])
const deletingFile = ref(false)
const replacingFile = ref<KnowledgeFileDto | null>(null)

// Dialogs
const deleteDialogVisible = ref(false)
const fileToDelete = ref<KnowledgeFileDto | null>(null)

// Methods
const handleSubmit = async () => {
  const isFormValid = await v$.value.$validate()
  if (!isFormValid) return

  emit('save-knowledgeBase', {
    valid: true,
    values: {
      name: form.value.name.trim(),
      description: form.value.description.trim() || undefined
    }
  })
}

const handleFileUploaded = async () => {
  // Refresh the files list after successful upload
  await fetchFiles()
  uploadDialogVisible.value = false
}

const openUploadDialog = () => {
  uploadDialogVisible.value = true
  fetchFiles() // Load files when opening upload dialog
}

// Methods for file management
const fetchFiles = async () => {
  if (!props.initialValues.id) return

  loadingFiles.value = true
  try {
    const response = await authenticatedApi.api.v1KnowledgebasesFilesList(props.initialValues.id!, {})
    existingFiles.value = response.data || []
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to load files',
      life: 3000
    })
  } finally {
    loadingFiles.value = false
  }
}

const confirmDeleteFile = (file: KnowledgeFileDto) => {
  fileToDelete.value = file
  deleteDialogVisible.value = true
}

const closeDeleteDialog = () => {
  deleteDialogVisible.value = false
  fileToDelete.value = null
}

const handleFileReplace = async (event: any, oldFile: KnowledgeFileDto) => { // eslint-disable-line @typescript-eslint/no-explicit-any
  if (!event.files?.[0]) return

  const newFile = event.files[0]
  replacingFile.value = oldFile

  try {
    // Create FormData with the uploaded file
    const formData = new FormData()
    formData.append('file', newFile)

    // Use the replace API endpoint
    await authenticatedApi.api.v1FilesReplaceUpdate(
      oldFile.id!,
      formData
    )

    // Show detailed success message
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: `File "${oldFile.fileName}" successfully replaced with "${newFile.name}"`,
      life: 5000
    })

    await fetchFiles()
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to replace file',
      life: 5000
    })
  } finally {
    replacingFile.value = null
  }
}

const deleteFile = async () => {
  if (!fileToDelete.value) return

  deletingFile.value = true
  try {
    await authenticatedApi.api.v1FilesDelete(fileToDelete.value.id!, {})

    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'File deleted successfully',
      life: 3000
    })

    closeDeleteDialog()
    await fetchFiles()

    // Don't emit save event for file operations - stay in edit mode
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to delete file',
      life: 5000
    })
  } finally {
    deletingFile.value = false
  }
}

// Watch for changes to initialValues to load files when KB changes
watch(() => props.initialValues.id, (newId) => {
  if (newId && !isCreating.value) {
    fetchFiles()
  } else {
    existingFiles.value = []
  }
})

onMounted(() => {
  // Initialize form validation
  v$.value.$touch()

  // Load files if we have a KB ID
  if (props.initialValues.id && !isCreating.value) {
    fetchFiles()
  }
})
</script>
