<template>
  <div class="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
    <DataTable
      :value="clusters"
      :loading="loading"
      :paginator="true"
      :rows="10"
      :rowsPerPageOptions="[5, 10, 25, 50]"
      stripedRows
      showGridlines
      responsiveLayout="scroll"
      tableStyle="min-width: 50rem"
      paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
      currentPageReportTemplate="{first} to {last} of {totalRecords}"
    >
      <Column field="name" header="Name" style="min-width: 12rem">
        <template #body="{ data }">
          <div class="flex items-center">
            <i class="pi pi-server text-slate-500 mr-2"></i>
            <span class="font-medium">{{ data.name }}</span>
          </div>
        </template>
      </Column>

      <Column field="server" header="Server URL" style="min-width: 18rem">
        <template #body="{ data }">
          <code class="bg-slate-100 px-2 py-1 rounded text-sm">{{ data.server }}</code>
        </template>
      </Column>

            <Column field="hasCertificateAuthority" header="Certificate" style="min-width: 8rem">
              <template #body="{ data }">
                <span
                  :class="data.hasCertificateAuthority ? 'text-green-600 bg-green-100' : 'text-slate-400 bg-slate-100'"
                  class="px-2 py-1 rounded-full text-xs font-medium"
                >
                  {{ data.hasCertificateAuthority ? 'Set' : 'Not set' }}
                </span>
              </template>
            </Column>

            <Column field="insecureSkipTlsVerify" header="Skip TLS" style="min-width: 6rem">
              <template #body="{ data }">
                <span
                  :class="data.insecureSkipTlsVerify ? 'text-amber-600 bg-amber-100' : 'text-green-600 bg-green-100'"
                  class="px-2 py-1 rounded-full text-xs font-medium"
                >
                  {{ data.insecureSkipTlsVerify ? 'Yes' : 'No' }}
                </span>
              </template>
            </Column>

      <Column field="createdAt" header="Created" style="min-width: 12rem">
        <template #body="{ data }">
          <span class="text-sm text-slate-600">{{ formatDate(data.createdAt) }}</span>
        </template>
      </Column>

      <Column header="Actions" style="min-width: 8rem">
        <template #body="{ data }">
          <div class="flex gap-1">
            <Button
              @click="$emit('edit-cluster', data)"
              icon="pi pi-pencil"
              severity="secondary"
              size="small"
              v-tooltip="'Edit Cluster'"
            />
            <Button
              @click="$emit('delete-cluster', data)"
              icon="pi pi-trash"
              severity="danger"
              size="small"
              v-tooltip="'Delete Cluster'"
            />
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import type { KubeClusterResponse } from '../api/Api'

defineProps<{
  clusters: KubeClusterResponse[]
  loading: boolean
}>()

defineEmits<{
  'edit-cluster': [cluster: KubeClusterResponse]
  'delete-cluster': [cluster: KubeClusterResponse]
}>()

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}
</script>
