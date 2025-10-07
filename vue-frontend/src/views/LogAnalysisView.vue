<template>
  <div class="w-full h-full flex">
    <!-- Left Panel - Analysis Parameters -->
    <div class="w-2/5 border-r border-slate-200 flex flex-col">
      <div class="p-6 border-b border-slate-200 flex-shrink-0">
        <div class="flex items-center mb-6">
          <i class="pi pi-search text-tight-blue mr-3"></i>
          <h1 class="text-xl font-semibold text-slate-900">Log Analysis</h1>
        </div>

        <div class="space-y-4">
          <!-- Cluster Selection -->
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-2">
              Select Cluster
            </label>
            <Dropdown
              v-model="selectedCluster"
              :options="clusters"
              optionLabel="name"
              optionValue="id"
              placeholder="Choose a cluster..."
              class="w-full"
              :loading="loadingClusters"
            />
            <p class="text-xs text-slate-500 mt-1">
              Choose the Kubernetes cluster to analyze logs from
            </p>
          </div>
        </div>
      </div>

      <!-- Analysis Form -->
      <div class="flex-1 p-6 overflow-y-auto">
        <div v-if="selectedCluster">
          <TabView v-model:activeIndex="activeTab" class="mb-6">
            <TabPanel header="Pod Analysis" value="0">
              <div class="space-y-4">
                <!-- Pod Parameters -->
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-2">
                      Namespace
                    </label>
                    <InputText
                      v-model="podAnalysis.namespace"
                      placeholder="e.g., default"
                      class="w-full"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-2">
                      Pod Name
                    </label>
                    <InputText
                      v-model="podAnalysis.podName"
                      placeholder="e.g., my-app-pod"
                      class="w-full"
                    />
                  </div>
                </div>

                <div>
                  <label class="block text-sm font-medium text-slate-700 mb-2">
                    Container Name (Optional)
                  </label>
                  <InputText
                    v-model="podAnalysis.containerName"
                    placeholder="e.g., app"
                    class="w-full"
                  />
                </div>

                <!-- Log Filters -->
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-2">
                      Last Lines
                    </label>
                    <InputNumber
                      v-model="podAnalysis.tailLines"
                      :min="1"
                      :max="1000"
                      placeholder="100"
                      class="w-full"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-2">
                      Since Seconds
                    </label>
                    <InputNumber
                      v-model="podAnalysis.sinceSeconds"
                      :min="1"
                      placeholder="300"
                      class="w-full"
                    />
                  </div>
                </div>

                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-2">
                      Limit Bytes
                    </label>
                    <InputNumber
                      v-model="podAnalysis.limitBytes"
                      :min="1"
                      placeholder="4096"
                      class="w-full"
                    />
                  </div>
                  <div class="flex items-center">
                    <Checkbox
                      id="previous"
                      v-model="podAnalysis.previous"
                      class="mr-2"
                    />
                    <label for="previous" class="text-sm text-slate-700">
                      Previous logs
                    </label>
                  </div>
                </div>

                <div class="flex items-center">
                  <Checkbox
                    id="podIncludeDescription"
                    v-model="podAnalysis.includeDescription"
                    class="mr-2"
                  />
                  <label for="podIncludeDescription" class="text-sm text-slate-700">
                    Include pod description in analysis
                  </label>
                </div>

                <Button
                  @click="runPodAnalysis"
                  :loading="analyzing"
                  icon="pi pi-bolt"
                  label="Analyze Pod Logs"
                  class="w-full mt-6"
                  severity="success"
                  :disabled="!isPodAnalysisValid"
                />
              </div>
            </TabPanel>

            <TabPanel header="Deployment Analysis" value="1">
              <div class="space-y-4">
                <!-- Deployment Parameters -->
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-2">
                      Namespace
                    </label>
                    <InputText
                      v-model="deploymentAnalysis.namespace"
                      placeholder="e.g., default"
                      class="w-full"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-2">
                      Deployment Name
                    </label>
                    <InputText
                      v-model="deploymentAnalysis.deploymentName"
                      placeholder="e.g., my-app"
                      class="w-full"
                    />
                  </div>
                </div>

                <div class="flex items-center">
                  <Checkbox
                    id="fallback"
                    v-model="deploymentAnalysis.fallback"
                    class="mr-2"
                  />
                  <label for="fallback" class="text-sm text-slate-700">
                    Fallback to pods if deployment not found
                  </label>
                </div>

                <div class="flex items-center">
                  <Checkbox
                    id="deploymentIncludeDescription"
                    v-model="deploymentAnalysis.includeDescription"
                    class="mr-2"
                  />
                  <label for="deploymentIncludeDescription" class="text-sm text-slate-700">
                    Include deployment description in analysis
                  </label>
                </div>

                <Button
                  @click="runDeploymentAnalysis"
                  :loading="analyzing"
                  icon="pi pi-bolt"
                  label="Analyze Deployment Logs"
                  class="w-full mt-6"
                  severity="success"
                  :disabled="!isDeploymentAnalysisValid"
                />
              </div>
            </TabPanel>
          </TabView>
        </div>

        <div v-else class="text-center py-12">
          <i class="pi pi-info-circle text-slate-400 text-3xl mb-4 block"></i>
          <p class="text-slate-600">
            Please select a cluster to begin log analysis
          </p>
        </div>
      </div>
    </div>

    <!-- Right Panel - Analysis Results -->
    <div class="w-3/5 flex flex-col">
      <div class="p-6 border-b border-slate-200 flex-shrink-0">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-semibold text-slate-900 flex items-center">
            <i class="pi pi-chart-line mr-3 text-tight-blue"></i>
            Analysis Results
          </h2>

          <Button
            v-if="analysisResult"
            @click="exportAnalysis"
            icon="pi pi-download"
            size="small"
            text
            label="Export"
          />
        </div>
      </div>

      <div class="flex-1 p-6 overflow-y-auto">
        <!-- Loading State -->
        <div v-if="analyzing" class="flex flex-col items-center justify-center py-12">
          <div class="w-16 h-16 border-4 border-tight-blue border-t-transparent rounded-full animate-spin mb-4"></div>
          <p class="text-slate-600 font-medium">Analyzing logs with AI...</p>
          <p class="text-sm text-slate-500 mt-1">This may take a few moments</p>
        </div>

        <!-- No Results State -->
        <div v-else-if="!analysisResult" class="flex flex-col items-center justify-center py-12">
          <i class="pi pi-search text-slate-300 text-5xl mb-4"></i>
          <p class="text-slate-600 font-medium">No analysis results yet</p>
          <p class="text-sm text-slate-500 mt-1">
            Select a cluster and run an analysis to see AI-powered recommendations
          </p>
        </div>

        <!-- Analysis Results -->
        <div v-else class="space-y-6">
          <!-- Success/Message Banner -->
          <div class="rounded-lg border p-4"
               :class="analysisResult.success ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'">
            <div class="flex items-start">
              <i :class="analysisResult.success ? 'pi pi-check-circle text-green-600' : 'pi pi-exclamation-triangle text-red-600'"
                 class="mr-3 mt-0.5"></i>
              <div>
                <p class="font-medium"
                   :class="analysisResult.success ? 'text-green-800' : 'text-red-800'">
                  {{ analysisResult.message }}
                </p>
                <p v-if="analysisResult.success" class="text-sm text-green-700 mt-1">
                  Found {{ analysisResult.data?.errors?.length || 0 }} issue{{ (analysisResult.data?.errors?.length || 0) !== 1 ? 's' : '' }}
                </p>
              </div>
            </div>
          </div>

          <!-- Error Analysis -->
          <div v-if="analysisResult.success && analysisResult.data?.errors?.length"
               class="space-y-4">
            <div v-for="(error, index) in analysisResult.data.errors" :key="index"
                 class="border border-slate-200 rounded-lg overflow-hidden">
              <div class="bg-slate-50 px-4 py-3 border-b border-slate-200">
                <h3 class="font-semibold text-slate-900 flex items-center">
                  <i class="pi pi-exclamation-triangle text-red-500 mr-2"></i>
                  {{ error.generalMessage }}
                </h3>
              </div>

              <div class="p-4 space-y-4">
                <!-- Occurrences -->
                <div>
                  <h4 class="text-sm font-semibold text-slate-700 mb-2">Occurrences:</h4>
                  <div class="bg-slate-100 rounded p-3 font-mono text-sm text-slate-800 overflow-x-auto">
                    <div v-for="occurrence in error.occurrences" :key="occurrence"
                         class="whitespace-pre-wrap">
                      {{ occurrence }}
                    </div>
                  </div>
                </div>

                <!-- Solutions -->
                <div v-if="error.solutions?.length">
                  <h4 class="text-sm font-semibold text-slate-700 mb-2">Solutions:</h4>
                  <div class="space-y-3">
                    <div v-for="(solution, solutionIndex) in error.solutions" :key="solutionIndex"
                         class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                      <p class="text-sm text-slate-800 mb-3">{{ solution.description }}</p>

                      <div v-if="solution.steps?.length" class="space-y-2">
                        <h5 class="text-xs font-semibold text-blue-700 uppercase tracking-wide">Steps:</h5>
                        <div class="space-y-2">
                          <div v-for="(step, stepIndex) in solution.steps" :key="stepIndex"
                               class="flex items-start">
                            <span class="inline-flex items-center justify-center w-5 h-5 bg-blue-500 text-white text-xs rounded-full mr-2 flex-shrink-0 mt-0.5">
                              {{ stepIndex + 1 }}
                            </span>
                            <div class="flex-1">
                              <p class="text-sm font-medium text-slate-800">{{ step.title }}</p>
                              <p class="text-xs text-slate-600 mt-1">{{ step.explanation }}</p>
                              <div v-if="step.command"
                                   class="bg-slate-800 text-slate-100 rounded px-2 py-1 text-xs font-mono mt-2">
                                {{ step.command }}
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { authenticatedApi } from '../api/authenticatedApi'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import TabView from 'primevue/tabview'
import TabPanel from 'primevue/tabpanel'

// State
const selectedCluster = ref<string>('')
const activeTab = ref(0)
const analysisResult = ref<any>(null)
const analyzing = ref(false)
const loadingClusters = ref(false)
const clusters = ref<any[]>([])

// Analysis forms
const podAnalysis = ref({
  namespace: '',
  podName: '',
  containerName: '',
  tailLines: 100,
  sinceSeconds: 300,
  previous: false,
  limitBytes: 4096,
  includeDescription: true
})

const deploymentAnalysis = ref({
  namespace: '',
  deploymentName: '',
  fallback: true,
  includeDescription: true
})

// Validation
const isPodAnalysisValid = computed(() =>
  selectedCluster.value &&
  podAnalysis.value.namespace &&
  podAnalysis.value.podName
)

const isDeploymentAnalysisValid = computed(() =>
  selectedCluster.value &&
  deploymentAnalysis.value.namespace &&
  deploymentAnalysis.value.deploymentName
)

// Methods
const loadClusters = async () => {
  try {
    loadingClusters.value = true
    const response = await authenticatedApi.api.v1ClustersList()
    clusters.value = response.data || []
  } catch (error) {
    console.error('Failed to load clusters:', error)
  } finally {
    loadingClusters.value = false
  }
}

const runPodAnalysis = async () => {
  if (!selectedCluster.value || analyzing.value) return

  try {
    analyzing.value = true
    analysisResult.value = null

    const request = {
      namespace: podAnalysis.value.namespace,
      podName: podAnalysis.value.podName,
      containerName: podAnalysis.value.containerName || undefined,
      tailLines: podAnalysis.value.tailLines || undefined,
      sinceSeconds: podAnalysis.value.sinceSeconds || undefined,
      previous: podAnalysis.value.previous || undefined,
      limitBytes: podAnalysis.value.limitBytes || undefined,
      includeDescription: podAnalysis.value.includeDescription || undefined
    }

    const response = await authenticatedApi.api.v1ClustersAnalysisPodsCreate(selectedCluster.value, request)
    analysisResult.value = response.data
  } catch (error: any) {
    console.error('Pod analysis failed:', error)
    analysisResult.value = {
      success: false,
      message: error?.response?.data?.detail || 'Analysis failed',
      data: null
    }
  } finally {
    analyzing.value = false
  }
}

const runDeploymentAnalysis = async () => {
  if (!selectedCluster.value || analyzing.value) return

  try {
    analyzing.value = true
    analysisResult.value = null

    const request = {
      namespace: deploymentAnalysis.value.namespace,
      deploymentName: deploymentAnalysis.value.deploymentName,
      fallback: deploymentAnalysis.value.fallback || undefined,
      includeDescription: deploymentAnalysis.value.includeDescription || undefined
    }

    const response = await authenticatedApi.api.v1ClustersAnalysisDeploymentsCreate(selectedCluster.value, request)
    analysisResult.value = response.data
  } catch (error: any) {
    console.error('Deployment analysis failed:', error)
    analysisResult.value = {
      success: false,
      message: error?.response?.data?.detail || 'Analysis failed',
      data: null
    }
  } finally {
    analyzing.value = false
  }
}

const exportAnalysis = () => {
  if (!analysisResult.value) return

  const data = {
    cluster: selectedCluster.value,
    timestamp: new Date().toISOString(),
    analysis: analysisResult.value
  }

  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `podmd-analysis-${Date.now()}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

onMounted(() => {
  loadClusters()
})
</script>
