<template>
  <div>
    <!-- Page Content -->
    <div class="w-full py-8 px-6">
      <div class="max-w-7xl mx-auto">
        <!-- Page Header -->
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div>
            <h1 class="text-2xl font-bold text-slate-900">Kubernetes Clusters</h1>
            <p class="text-slate-600 mt-1">Manage your configured Kubernetes clusters</p>
          </div>
          <Button
            @click="openCreateDialog"
            icon="pi pi-plus"
            label="Add Cluster"
            class="hover:text-slate-800 hover:bg-slate-100 border-slate-300"
          />
        </div>

        <!-- Clusters DataTable -->
        <KubeClustersTable
          :clusters="clusters"
          :loading="loading"
          @edit-cluster="openEditDialog"
          @delete-cluster="openDeleteDialog"
        />

        <!-- Empty State -->
        <div v-if="!loading && clusters.length === 0" class="text-center py-12">
          <div class="w-16 h-16 bg-slate-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <i class="pi pi-server text-slate-400 text-2xl"></i>
          </div>
          <h3 class="text-lg font-medium text-slate-900 mb-2">No clusters configured</h3>
          <p class="text-slate-600 mb-6">Get started by adding your first Kubernetes cluster.</p>
          <Button
            @click="openCreateDialog"
            icon="pi pi-plus"
            label="Add Your First Cluster"
            class="hover:text-slate-800 hover:bg-slate-100 border-slate-300"
          />
        </div>
      </div>
    </div>

    <!-- Kube Cluster Form Dialog -->
    <Dialog
      v-model:visible="clusterDialogVisible"
      :header="isEditing ? 'Edit Cluster' : 'Add New Cluster'"
      modal
      :style="{ width: '600px' }"
      :closable="true"
    >
      <KubeClustersForm
        :visible="clusterDialogVisible"
        :is-editing="isEditing"
        :initial-values="initialValues"
        :saving="saving"
        @save-cluster="handleSaveCluster"
        @close-dialog="closeDialog"
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
          <p class="font-medium text-slate-900 mb-1">Delete Cluster</p>
          <p class="text-slate-600 text-sm">
            Are you sure you want to delete the cluster <strong>{{ clusterToDelete?.name }}</strong>?
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
          @click="deleteCluster"
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
import { ref, reactive, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import KubeClustersTable from '../components/KubeClustersTable.vue'
import KubeClustersForm from '../components/KubeClustersForm.vue'
import type { KubeClusterResponse, CreateKubeClusterRequest, UpdateKubeClusterRequest } from '../api/Api'
import { authenticatedApi } from '../api/authenticatedApi'

const toast = useToast()

// Reactive form data for PrimeVue Forms
const initialValues = reactive({
  id: undefined as string | undefined,
  name: '',
  server: '',
  bearerToken: '',
  certificateAuthorityPem: '',
  insecureSkipTlsVerify: false,
  defaultNamespace: ''
})

// State
const clusters = ref<KubeClusterResponse[]>([])
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)

// Dialogs
const clusterDialogVisible = ref(false)
const deleteDialogVisible = ref(false)
const isEditing = ref(false)
const clusterToDelete = ref<KubeClusterResponse | null>(null)
const originalClusterData = ref<KubeClusterResponse | null>(null)

// Form reset for PrimeVue Forms
const resetFormData = () => {
  initialValues.id = undefined
  initialValues.name = ''
  initialValues.server = ''
  initialValues.bearerToken = ''
  initialValues.certificateAuthorityPem = ''
  initialValues.insecureSkipTlsVerify = false
  initialValues.defaultNamespace = ''
}

// Data fetching
const fetchClusters = async () => {
  loading.value = true
  try {
    const response = await authenticatedApi.api.v1ClustersList({})
    clusters.value = response.data || []
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Failed to load clusters',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// Dialog actions
const openCreateDialog = () => {
  resetFormData()
  isEditing.value = false
  clusterDialogVisible.value = true
}

const openEditDialog = (cluster: KubeClusterResponse) => {
  // Store original cluster data for comparison during update
  originalClusterData.value = { ...cluster }

  // Reset form data first
  resetFormData()

  // Populate with cluster data for editing
  initialValues.id = cluster.id || undefined
  initialValues.name = cluster.name || ''
  initialValues.server = cluster.server || ''
  initialValues.bearerToken = cluster.hasBearerToken ? '' : '' // Can't populate for security
  initialValues.certificateAuthorityPem = cluster.hasCertificateAuthority ? '' : '' // Can't populate for security
  initialValues.insecureSkipTlsVerify = Boolean(cluster.insecureSkipTlsVerify)
  initialValues.defaultNamespace = cluster.defaultNamespace || ''

  isEditing.value = true
  clusterDialogVisible.value = true
}

const closeDialog = () => {
  clusterDialogVisible.value = false
}

const openDeleteDialog = (cluster: KubeClusterResponse) => {
  clusterToDelete.value = cluster
  deleteDialogVisible.value = true
}

const closeDeleteDialog = () => {
  deleteDialogVisible.value = false
  clusterToDelete.value = null
}

// CRUD operations - PrimeVue Forms submit event
const handleSaveCluster = async ({ valid, values }: { valid: boolean, values: Record<string, unknown> }) => {
  if (!valid) return
  saving.value = true

  try {
    // Clean and trim the values
    const processedValues: Record<string, unknown> = {}
    if (values.name) processedValues.name = String(values.name).trim()
    if (values.server) processedValues.server = String(values.server).trim()
    if (values.bearerToken) processedValues.bearerToken = String(values.bearerToken).trim()
    if (values.certificateAuthorityPem) processedValues.certificateAuthorityPem = String(values.certificateAuthorityPem).trim()
    if (values.insecureSkipTlsVerify !== undefined) processedValues.insecureSkipTlsVerify = Boolean(values.insecureSkipTlsVerify)
    if (values.defaultNamespace) processedValues.defaultNamespace = String(values.defaultNamespace).trim()

    if (isEditing.value && initialValues.id) {
      // For updates, only send fields that have values (partial update)
      const updateData: Partial<UpdateKubeClusterRequest> = {}

      if (processedValues.name) updateData.name = processedValues.name as string
      if (processedValues.server) updateData.server = processedValues.server as string
      if (processedValues.bearerToken) updateData.bearerToken = processedValues.bearerToken as string
      if (processedValues.certificateAuthorityPem) updateData.certificateAuthorityPem = processedValues.certificateAuthorityPem as string
      if (processedValues.insecureSkipTlsVerify !== undefined) updateData.insecureSkipTlsVerify = processedValues.insecureSkipTlsVerify as boolean
      if (processedValues.defaultNamespace) updateData.defaultNamespace = processedValues.defaultNamespace as string

      await authenticatedApi.api.v1ClustersUpdate(initialValues.id, updateData, {})
      toast.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Cluster updated successfully',
        life: 3000
      })
    } else {
      // For create, all required fields must be present (handled by validation)
      await authenticatedApi.api.v1ClustersCreate(processedValues as CreateKubeClusterRequest, {})
      toast.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Cluster created successfully',
        life: 3000
      })
    }

    closeDialog()
    await fetchClusters()
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to save cluster',
      life: 5000
    })
  } finally {
    saving.value = false
  }
}

const deleteCluster = async () => {
  if (!clusterToDelete.value) return

  deleting.value = true

  try {
    await authenticatedApi.api.v1ClustersDelete(clusterToDelete.value.id!, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Cluster deleted successfully',
      life: 3000
    })
    closeDeleteDialog()
    await fetchClusters()
  } catch (error: unknown) {
    const err = error as { response?: { data?: { detail?: string } } }
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.detail || 'Failed to delete cluster',
      life: 5000
    })
  } finally {
    deleting.value = false
  }
}

// Lifecycle
onMounted(() => {
  fetchClusters()
})
</script>
