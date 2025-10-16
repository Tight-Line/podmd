<template>
  <div class="h-screen flex flex-col">
    <!-- Page Header -->
    <div class="flex justify-between items-center px-6 py-4 bg-white border-b border-slate-200">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Kubernetes Clusters</h1>
        <p class="text-slate-600 mt-1">Manage your configured Kubernetes clusters</p>
      </div>
      <Button
        @click="openCreateDialog"
        icon="pi pi-plus"
        label="Add Cluster"
        severity="primary"
      />
    </div>

    <!-- Main Content - Split Panel -->
    <div class="flex-1 overflow-hidden">
      <Splitter :minSizes="[25, 35]" :gutterSize="8" class="h-full">
        <!-- Left Panel - Cluster List -->
        <SplitterPanel :size="30" :minSize="25">
          <div class="h-full bg-white border-r border-slate-200">
            <KubeClustersList
              :clusters="clusters"
              :loading="loading"
              :selected-cluster="selectedCluster"
              @select-cluster="selectCluster"
              @add-cluster="openCreateDialog"
              @delete-cluster="openDeleteDialog"
            />
          </div>
        </SplitterPanel>

        <!-- Right Panel - Cluster Details -->
        <SplitterPanel :size="70" :minSize="35">
          <div class="h-full bg-white">
            <div v-if="selectedCluster || isCreating" class="h-full relative">
              <!-- Main Content - Tabs (fills remaining space) -->
              <Tabs v-model:value="activeTab" @tab-change="onTabChange" class="h-full flex flex-col pb-14">
                <TabList>
                  <Tab value="0">Configuration</Tab>
                  <Tab value="1">Knowledge Bases</Tab>
                </TabList>
                <TabPanels class="flex-1 overflow-hidden">
                  <TabPanel value="0" class="h-full overflow-hidden">
                    <div class="h-full overflow-auto p-4">
                      <KubeClustersForm
                        v-if="selectedCluster || isCreating"
                        :key="selectedCluster?.id || 'creating'"
                        :visible="true"
                        :initial-values="formData"
                        :is-editing="isEditing || isCreating"
                        :read-only="!isEditing && !isCreating"
                        :saving="saving"
                        @save-cluster="handleFormSave"
                      />
                    </div>
                  </TabPanel>
                  <TabPanel value="1" class="h-full overflow-hidden">
                    <div class="h-full overflow-auto p-4">
                      <KnowledgeBaseConnections
                        v-if="selectedCluster"
                        ref="kbConnectionsRef"
                        :source-id="selectedCluster.id!"
                        :source-type="'cluster'"
                        :is-editing="false"
                        :read-only="false"
                      />
                    </div>
                  </TabPanel>
                </TabPanels>
              </Tabs>

              <!-- Footer Actions (locked to bottom) -->
              <div class="absolute bottom-0 left-0 right-0 bg-white border-t border-slate-200">
                <!-- Configuration tab actions -->
                <div v-if="activeTab === '0'" class="px-4 py-3 flex justify-end">
                  <div v-if="!isEditing && !isCreating" class="flex gap-2">
                    <Button
                      @click="startEdit"
                      icon="pi pi-pencil"
                      label="Edit Cluster"
                      severity="secondary"
                      size="small"
                    />
                  </div>
                  <div v-else class="flex gap-2">
                    <Button
                      @click="cancelEdit"
                      label="Cancel"
                      severity="secondary"
                      size="small"
                    />
                  </div>
                </div>

                <!-- Knowledge Bases tab actions -->
                <div v-if="activeTab === '1'" class="px-4 py-3 flex justify-end">
                  <Button
                    @click="addKnowledgeBase"
                    icon="pi pi-plus"
                    label="Add Knowledge Base"
                    severity="secondary"
                    size="small"
                  />
                </div>
              </div>
            </div>
            <div v-else class="h-full flex items-center justify-center">
              <div class="text-center">
                <i class="pi pi-server text-slate-400 text-4xl mb-4 block"></i>
                <h3 class="text-lg font-medium text-slate-900 mb-2">Select a cluster to view details</h3>
                <p class="text-slate-600">Choose a cluster from the left panel to see its configuration</p>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>

    <!-- Create Cluster Dialog -->
    <Dialog
      v-model:visible="createDialogVisible"
      header="Create New Cluster"
      modal
      :style="{ width: '600px' }"
      :closable="true"
    >
      <KubeClustersForm
        :is-editing="true"
        :saving="saving"
        :initial-values="{
          name: '',
          server: '',
          bearerToken: '',
          certificateAuthorityPem: '',
          insecureSkipTlsVerify: false,
          defaultNamespace: ''
        }"
        @save-cluster="handleCreateCluster"
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
import { ref, onMounted, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import Dialog from 'primevue/dialog'
import Splitter from 'primevue/splitter'
import SplitterPanel from 'primevue/splitterpanel'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import type { KubeClusterResponse } from '../api/Api'
import { authenticatedApi } from '../api/authenticatedApi'
import KubeClustersForm from '../components/KubeClustersForm.vue'
import KubeClustersList from '../components/KubeClustersList.vue'
import KnowledgeBaseConnections from '../components/KnowledgeBaseConnections.vue'

const toast = useToast()

// State
const clusters = ref<KubeClusterResponse[]>([])
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)

// Split panel state
const selectedCluster = ref<KubeClusterResponse | null>(null)
const isEditing = ref(false)
const isCreating = ref(false)
const activeTab = ref('0')

// Component refs
const kbConnectionsRef = ref<InstanceType<typeof KnowledgeBaseConnections> | null>(null)

// Form data for right panel
const formData = computed(() => {
  if (selectedCluster.value) {
    return {
      id: selectedCluster.value.id, // Needed for knowledge base management
      name: selectedCluster.value.name || '',
      server: selectedCluster.value.server || '',
      bearerToken: '', // Can't populate for security
      certificateAuthorityPem: '', // Can't populate for security
      insecureSkipTlsVerify: Boolean(selectedCluster.value.insecure_skip_tls_verify),
      defaultNamespace: selectedCluster.value.default_namespace || '',
      instructions: selectedCluster.value.instructions || '',
      responseFormat: selectedCluster.value.response_format || '',
      hasBearerToken: Boolean(selectedCluster.value.bearer_token_enc),
      hasCertificateAuthority: Boolean(selectedCluster.value.certificate_authority_pem)
    }
  }
  return {
    id: undefined, // No ID when creating
    name: '',
    server: '',
    bearerToken: '',
    certificateAuthorityPem: '',
    insecureSkipTlsVerify: false,
    defaultNamespace: '',
    instructions: '',
    responseFormat: '',
    hasBearerToken: false,
    hasCertificateAuthority: false
  }
})

// Dialogs
const createDialogVisible = ref(false)
const deleteDialogVisible = ref(false)
const clusterToDelete = ref<KubeClusterResponse | null>(null)

// Data fetching
const fetchClusters = async () => {
  loading.value = true
  try {
    const response = await authenticatedApi.kubeClusters.listKubeClustersKubeClustersGet({})
    clusters.value = response.data?.kube_clusters || []

    // Auto-select first cluster if available
    if (clusters.value.length > 0 && !selectedCluster.value) {
      selectedCluster.value = clusters.value[0]!
    }
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

// Cluster selection
const selectCluster = (cluster: KubeClusterResponse) => {
  selectedCluster.value = cluster
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

// Tab change handler
const onTabChange = (event: { value?: number | string }) => {
  activeTab.value = event.value?.toString() || '0'
}

// Add knowledge base handler
const addKnowledgeBase = () => {
  if (kbConnectionsRef.value) {
    kbConnectionsRef.value.openAddKnowledgeBaseDialog()
  }
}

const openCreateDialog = () => {
  createDialogVisible.value = true
}

const openDeleteDialog = (cluster: KubeClusterResponse) => {
  clusterToDelete.value = cluster
  deleteDialogVisible.value = true
}

const closeDeleteDialog = () => {
  deleteDialogVisible.value = false
  clusterToDelete.value = null
}

const handleCreateCluster = async ({ valid, values }: { valid: boolean, values: Record<string, unknown> }) => {
  if (!valid) return
  saving.value = true

  // Track existing cluster IDs before creation
  const existingIds = new Set(clusters.value.map(c => c.id))

  try {
    const createData = {
      name: String(values.name).trim(),
      server: String(values.server).trim(),
      bearer_token: String(values.bearerToken).trim(),
      certificate_authority_pem: values.certificateAuthorityPem ? String(values.certificateAuthorityPem).trim() : undefined,
      insecure_skip_tls_verify: Boolean(values.insecureSkipTlsVerify),
      default_namespace: values.defaultNamespace ? String(values.defaultNamespace).trim() : undefined,
      instructions: values.instructions ? String(values.instructions).trim() : undefined,
      response_format: values.responseFormat ? String(values.responseFormat).trim() : undefined
    }

    await authenticatedApi.kubeClusters.createKubeClusterKubeClustersPost(createData, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Cluster created successfully',
      life: 3000
    })

    createDialogVisible.value = false
    await fetchClusters()

    // Auto-select the newly created cluster (find cluster not in existing IDs)
    const newCluster = clusters.value.find(cluster => cluster.id && !existingIds.has(cluster.id))
    if (newCluster) {
      selectedCluster.value = newCluster
      isEditing.value = false
      isCreating.value = false
    }
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

// Form save handlers
const handleFormSave = async ({ valid, values }: { valid: boolean, values: Record<string, unknown> }) => {
  if (!valid) return

  if (isCreating.value && !selectedCluster.value) {
    // Handle inline cluster creation in split panel
    return await handleCreateCluster({ valid, values })
  } else if (selectedCluster.value && isEditing.value) {
    // Update existing cluster
    saving.value = true
    try {
      const updateData: any = {}
      if (values.name && String(values.name).trim() !== selectedCluster.value.name) {
        updateData.name = String(values.name).trim()
      }
      if (values.server && String(values.server).trim() !== selectedCluster.value.server) {
        updateData.server = String(values.server).trim()
      }
      if (values.bearerToken && String(values.bearerToken).trim()) {
        updateData.bearer_token = String(values.bearerToken).trim()
      }
      if (values.certificateAuthorityPem && String(values.certificateAuthorityPem).trim()) {
        updateData.certificate_authority_pem = String(values.certificateAuthorityPem).trim()
      }
      if (values.insecureSkipTlsVerify !== undefined && Boolean(values.insecureSkipTlsVerify) !== Boolean(selectedCluster.value.insecure_skip_tls_verify)) {
        updateData.insecure_skip_tls_verify = Boolean(values.insecureSkipTlsVerify)
      }
      if (values.defaultNamespace !== undefined) {
        const newNamespace = String(values.defaultNamespace).trim()
        if (newNamespace !== (selectedCluster.value.default_namespace || '')) {
          updateData.default_namespace = newNamespace || undefined
        }
      }
      if (values.instructions !== undefined) {
        const newInstructions = String(values.instructions).trim()
        if (newInstructions !== (selectedCluster.value.instructions || '')) {
          updateData.instructions = newInstructions || undefined
        }
      }
      if (values.responseFormat !== undefined) {
        const newResponseFormat = String(values.responseFormat).trim()
        if (newResponseFormat !== (selectedCluster.value.response_format || '')) {
          updateData.response_format = newResponseFormat || undefined
        }
      }

      if (Object.keys(updateData).length > 0) {
        await authenticatedApi.kubeClusters.updateKubeClusterKubeClustersClusterIdPut(selectedCluster.value.id!, updateData, {})
        toast.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Cluster updated successfully',
          life: 3000
        })
        await fetchClusters()
      }

      isEditing.value = false
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
}


const deleteCluster = async () => {
  if (!clusterToDelete.value) return

  deleting.value = true

  try {
    await authenticatedApi.kubeClusters.deleteKubeClusterKubeClustersClusterIdDelete(clusterToDelete.value.id!, {})
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Cluster deleted successfully',
      life: 3000
    })

    // If deleted cluster was selected, deselect it
    if (selectedCluster.value?.id === clusterToDelete.value.id) {
      selectedCluster.value = clusters.value.length > 1 ? clusters.value.find(c => c.id !== clusterToDelete.value!.id) || null : null
    }

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

onMounted(() => {
  fetchClusters()
})
</script>
