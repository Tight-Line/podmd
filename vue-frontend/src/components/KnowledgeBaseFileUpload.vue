<template>
  <div class="space-y-4">
    <!-- File Upload Section -->
    <div v-if="showUploadArea !== false" class="border-2 border-dashed border-slate-300 rounded-lg p-6">
      <FileUpload
        ref="fileUploadRef"
        name="files"
        :multiple="true"
        accept=".pdf,.txt,.json,.md,.doc,.docx"
        :maxFileSize="10485760"
        customUpload
        @uploader="onFileUpload($event)"
        :auto="false"
      >
        <template #empty>
          <div class="flex flex-col items-center justify-center py-8 text-center">
            <i class="pi pi-cloud-upload text-slate-400 text-4xl mb-3"></i>
            <h4 class="text-lg font-medium text-slate-900 mb-2">Upload Files</h4>
            <p class="text-slate-600 text-sm mb-4">
              Drag and drop files here, or click to browse
            </p>
            <p class="text-slate-500 text-xs">
              Supports: PDF, TXT, JSON, MD, DOC, DOCX (max 10MB each)
            </p>
          </div>
        </template>
      </FileUpload>
    </div>

    <!-- Existing Files List -->
    <div v-if="(showFileList !== false && !loadingFiles && existingFiles.length > 0)" class="space-y-4">
      <div class="border-t border-slate-200 pt-4">
        <h4 class="text-lg font-medium text-slate-900 mb-4">Existing Files</h4>

        <div class="space-y-2">
          <div
            v-for="file in existingFiles"
            :key="file.id"
            class="flex items-center justify-between bg-white border border-slate-200 rounded-lg p-4 hover:shadow-sm transition-shadow"
          >
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
              <!-- Delete Button -->
              <Button
                @click="confirmDeleteFile(file)"
                icon="pi pi-trash"
                severity="danger"
                text
                size="small"
                v-tooltip="'Delete file'"
                class="p-2"
              />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="showFileList !== false && !loadingFiles && existingFiles.length === 0" class="text-center py-8">
      <i class="pi pi-files text-slate-400 text-4xl mb-3 block"></i>
      <h5 class="text-lg font-medium text-slate-900 mb-2">No files uploaded yet</h5>
      <p class="text-slate-600">Upload your first file to get started with document storage</p>
    </div>

    <!-- Loading State for Files -->
    <div v-if="showFileList !== false && loadingFiles" class="text-center py-8">
      <i class="pi pi-spin pi-spinner text-slate-400 text-2xl mb-3 block"></i>
      <p class="text-slate-600">Loading files...</p>
    </div>



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
import { ref, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import FileUpload from 'primevue/fileupload'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import type { KnowledgeFileDto } from '../api/Api'
import { authenticatedApi } from '../api/authenticatedApi'

interface Props {
  knowledgeBaseId: string
  showUploadArea?: boolean
  showFileList?: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'file-uploaded': []
}>()

const toast = useToast()

// File Upload component refs
const fileUploadRef = ref() // eslint-disable-line @typescript-eslint/no-explicit-any

// State
const loadingFiles = ref(false)
const existingFiles = ref<KnowledgeFileDto[]>([])
const selectedFiles = ref<File[]>([])
const uploading = ref(false)

// File operations state
const deletingFile = ref(false)

// Dialogs
const deleteDialogVisible = ref(false)
const fileToDelete = ref<KnowledgeFileDto | null>(null)

// Methods
const fetchFiles = async () => {
  loadingFiles.value = true
  try {
    const response = await authenticatedApi.api.v1KnowledgebasesFilesList(props.knowledgeBaseId, {})
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

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const getFileTypeDisplay = (contentType: string) => {
  const typeMap: Record<string, string> = {
    'application/pdf': 'PDF',
    'text/plain': 'TXT',
    'application/json': 'JSON',
    'text/markdown': 'MD',
    'application/msword': 'DOC',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'DOCX'
  }
  return typeMap[contentType] || contentType
}

const formatDate = (dateString: string) => {
  if (!dateString) return ''
  try {
    const date = new Date(dateString)
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch {
    return dateString
  }
}

const onFileUpload = async (event: any) => { // eslint-disable-line @typescript-eslint/no-explicit-any
  uploading.value = true
  try {
    // Create FormData and append files
    const formData = new FormData()
    event.files.forEach((file: File) => {
      formData.append('files', file)
    })

    // Use the generated API method with FormData directly
    await authenticatedApi.api.v1KnowledgebasesFilesCreate(
      props.knowledgeBaseId,
      formData
    )

    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: `${event.files.length} file(s) uploaded successfully`,
      life: 3000
    })

    // Clear selected files and refresh list
    selectedFiles.value = []
    fileUploadRef.value?.clear()
    await fetchFiles()

    emit('file-uploaded')
  } catch (error: unknown) {
    const err = error as { message?: string }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.message || 'Failed to upload files',
      life: 5000
    })
  } finally {
    uploading.value = false
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

    emit('file-uploaded')
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to delete file',
      life: 5000
    })
  } finally {
    deletingFile.value = false
  }
}

onMounted(() => {
  fetchFiles()
})
</script>

<style scoped>
.custom-file-upload :deep(.p-fileupload-files) {
  display: none !important;
}

/* Center the content and add proper spacing */
.custom-file-upload :deep(.p-fileupload-content) {
  display: flex;
  flex-direction: column;
  min-height: 200px;
}
</style>
