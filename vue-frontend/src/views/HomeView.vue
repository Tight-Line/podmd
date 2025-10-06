<template>
  <div>

    <!-- Page Content -->
    <div class="w-full py-8 px-6">
      <div class="max-w-7xl mx-auto">


        <!-- Status Overview -->
        <div class="mb-8">
          <h2 class="text-xl font-semibold text-slate-900 mb-6">System Status</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
            <!-- API Status Card -->
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div v-if="backendStatus === 'connected'"
                       class="w-12 h-12 bg-green-500 rounded-full flex items-center justify-center shadow-lg">
                    <i class="pi pi-check text-white text-lg"></i>
                  </div>
                  <div v-else-if="backendStatus === 'error'"
                       class="w-12 h-12 bg-red-500 rounded-full flex items-center justify-center shadow-lg">
                    <i class="pi pi-times text-white text-lg"></i>
                  </div>
                  <div v-else
                       class="w-12 h-12 bg-amber-500 rounded-full flex items-center justify-center shadow-lg">
                    <i class="pi pi-spinner pi-spin text-white text-lg"></i>
                  </div>
                </div>
                <div class="ml-4">
                  <dt class="text-sm font-medium text-slate-500 mb-1">Backend API</dt>
                  <dd class="text-lg font-semibold text-slate-900">{{ backendMessage }}</dd>
                </div>
              </div>
            </div>

            <!-- Frontend Card -->
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center shadow-lg">
                    <i class="pi pi-code text-white text-lg"></i>
                  </div>
                </div>
                <div class="ml-4">
                  <dt class="text-sm font-medium text-slate-500 mb-1">Frontend</dt>
                  <dd class="text-lg font-semibold text-slate-900">Vue.js 3 + TS</dd>
                </div>
              </div>
            </div>

            <!-- AI Engine Card -->
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div class="w-12 h-12 bg-purple-500 rounded-full flex items-center justify-center shadow-lg">
                    <i class="pi pi-bolt text-white text-lg"></i>
                  </div>
                </div>
                <div class="ml-4">
                  <dt class="text-sm font-medium text-slate-500 mb-1">AI Engine</dt>
                  <dd class="text-lg font-semibold text-slate-900">OpenAI Ready</dd>
                </div>
              </div>
            </div>

            <!-- Database Card -->
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div class="w-12 h-12 bg-emerald-500 rounded-full flex items-center justify-center shadow-lg">
                    <i class="pi pi-database text-white text-lg"></i>
                  </div>
                </div>
                <div class="ml-4">
                  <dt class="text-sm font-medium text-slate-500 mb-1">Database</dt>
                  <dd class="text-lg font-semibold text-slate-900">MySQL 8.0</dd>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Main Content Area -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <!-- Left Column - Features -->
          <div class="lg:col-span-2 space-y-6">
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
              <h3 class="text-lg font-semibold text-slate-900 mb-4 flex items-center">
                <i class="pi pi-star text-yellow-500 mr-2"></i>
                Key Features
              </h3>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="flex items-start space-x-3">
                  <i class="pi pi-check-circle text-green-500 mt-0.5"></i>
                  <div>
                    <h4 class="font-medium text-slate-900">AI-Powered Analysis</h4>
                    <p class="text-sm text-slate-600 mt-1">Intelligent log analysis with GPT models</p>
                  </div>
                </div>
                <div class="flex items-start space-x-3">
                  <i class="pi pi-check-circle text-green-500 mt-0.5"></i>
                  <div>
                    <h4 class="font-medium text-slate-900">Kubernetes Integration</h4>
                    <p class="text-sm text-slate-600 mt-1">Native support for pods and deployments</p>
                  </div>
                </div>
                <div class="flex items-start space-x-3">
                  <i class="pi pi-check-circle text-green-500 mt-0.5"></i>
                  <div>
                    <h4 class="font-medium text-slate-900">Secure & Compliant</h4>
                    <p class="text-sm text-slate-600 mt-1">AES encryption for sensitive data</p>
                  </div>
                </div>
                <div class="flex items-start space-x-3">
                  <i class="pi pi-check-circle text-green-500 mt-0.5"></i>
                  <div>
                    <h4 class="font-medium text-slate-900">REST API</h4>
                    <p class="text-sm text-slate-600 mt-1">OpenAPI documentation available</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Getting Started -->
            <div class="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl border border-blue-200 p-6">
              <div class="flex items-start justify-between">
                <div>
                  <h3 class="text-lg font-semibold text-slate-900 mb-2">Getting Started</h3>
                  <p class="text-slate-700 mb-4">
                    Your PodMD frontend is ready for development. Start building authentication and cluster management features.
                  </p>
                  <div class="flex flex-wrap gap-3">
                    <Button @click="checkBackendStatus" icon="pi pi-refresh" size="small" label="Check API Status" />
                    <Button severity="secondary" size="small" label="View API Docs" />
                  </div>
                </div>
                <div class="hidden sm:block">
                  <div class="w-16 h-16 bg-blue-500 rounded-full flex items-center justify-center">
                    <i class="pi pi-rocket text-white text-2xl"></i>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Right Column - Info -->
          <div class="space-y-6">
            <!-- Tech Stack -->
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
              <h3 class="text-lg font-semibold text-slate-900 mb-4">Tech Stack</h3>
              <div class="space-y-3">
                <div class="flex items-center justify-between py-2">
                  <span class="text-sm text-slate-600">Framework</span>
                  <span class="text-sm font-medium text-slate-900">Vue.js 3</span>
                </div>
                <div class="flex items-center justify-between py-2">
                  <span class="text-sm text-slate-600">UI Library</span>
                  <span class="text-sm font-medium text-slate-900">PrimeVue 4</span>
                </div>
                <div class="flex items-center justify-between py-2">
                  <span class="text-sm text-slate-600">Styling</span>
                  <span class="text-sm font-medium text-slate-900">Tailwind CSS 4</span>
                </div>
                <div class="flex items-center justify-between py-2">
                  <span class="text-sm text-slate-600">State Management</span>
                  <span class="text-sm font-medium text-slate-900">Pinia</span>
                </div>
                <div class="flex items-center justify-between py-2">
                  <span class="text-sm text-slate-600">TypeScript</span>
                  <span class="text-sm font-medium text-slate-900">v5.9</span>
                </div>
                <div class="flex items-center justify-between py-2 border-t border-slate-200 pt-2">
                  <span class="text-sm text-slate-600">API Client</span>
                  <span class="text-sm font-medium text-slate-900">Auto-generated</span>
                </div>
              </div>
            </div>

            <!-- Quick Links -->
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
              <h3 class="text-lg font-semibold text-slate-900 mb-4">Quick Links</h3>
              <div class="space-y-2">
                <a href="http://localhost:8080/swagger" target="_blank"
                   class="flex items-center p-2 rounded-lg hover:bg-slate-50 text-slate-700 hover:text-slate-900 transition-colors">
                  <i class="pi pi-external-link text-slate-400 mr-3"></i>
                  API Documentation
                </a>
                <a href="https://primevue.org" target="_blank"
                   class="flex items-center p-2 rounded-lg hover:bg-slate-50 text-slate-700 hover:text-slate-900 transition-colors">
                  <i class="pi pi-external-link text-slate-400 mr-3"></i>
                  PrimeVue Docs
                </a>
                <a href="https://pinia.vuejs.org" target="_blank"
                   class="flex items-center p-2 rounded-lg hover:bg-slate-50 text-slate-700 hover:text-slate-900 transition-colors">
                  <i class="pi pi-external-link text-slate-400 mr-3"></i>
                  Pinia Docs
                </a>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <footer class="mt-12 pt-8 border-t border-slate-200">
          <div class="text-center">
            <p class="text-slate-500 text-sm">
              PodMD - Transforming Kubernetes troubleshooting with AI-powered intelligence
            </p>
            <p class="text-slate-400 text-xs mt-2">
              Built with Vue.js, PrimeVue, and OpenAI
            </p>
          </div>
        </footer>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import Button from 'primevue/button'

const router = useRouter()
const authStore = useAuthStore()

const backendStatus = ref<'checking' | 'connected' | 'error'>('checking')
const backendMessage = ref('')

const checkBackendStatus = async () => {
  try {
    const response = await fetch('http://localhost:8080/health')
    if (response.ok) {
      backendStatus.value = 'connected'
      backendMessage.value = 'Backend API is running'
    } else {
      throw new Error('Health check failed')
    }
  } catch {
    backendStatus.value = 'error'
    backendMessage.value = 'Backend API not available'
  }
}

onMounted(() => {
  checkBackendStatus()
})
</script>
