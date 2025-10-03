<template>
  <div class="h-full flex flex-col">
    <!-- Scrollable Form Content -->
    <div class="flex-1 overflow-y-auto">
      <div class="space-y-6 pb-4">
        <!-- Basic Information -->
        <div class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <!-- Name Field -->
            <div class="space-y-2">
              <label for="name" class="block text-sm font-medium text-slate-700">
                Server Name <span class="text-red-500">*</span>
              </label>
              <InputText
                id="name"
                v-model="formData.name"
                :disabled="!isEditing"
                :class="{ 'p-invalid': validationErrors.name }"
                placeholder="e.g., Production Jenkins"
                class="w-full"
              />
              <small v-if="validationErrors.name" class="p-error">
                {{ validationErrors.name }}
              </small>
            </div>

            <!-- Server URL Field -->
            <div class="space-y-2">
              <label for="server" class="block text-sm font-medium text-slate-700">
                Server URL <span class="text-red-500">*</span>
              </label>
              <InputText
                id="server"
                v-model="formData.server"
                :disabled="!isEditing"
                :class="{ 'p-invalid': validationErrors.server }"
                placeholder="https://jenkins.company.com"
                class="w-full"
              />
              <small v-if="validationErrors.server" class="p-error">
                {{ validationErrors.server }}
              </small>
            </div>
          </div>
        </div>

        <Divider />

        <!-- Authentication -->
        <div class="space-y-4">
          <!-- Username and API Token in same line -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <!-- Username Field -->
            <div class="flex flex-col gap-1">
              <label for="username" class="block text-sm font-medium text-slate-700">
                Username <span class="text-red-500">*</span>
              </label>
              <InputText
                id="username"
                v-model="formData.username"
                :disabled="!isEditing"
                :class="{ 'p-invalid': validationErrors.username }"
                placeholder="jenkins_user"
                class="w-full"
              />
              <small v-if="validationErrors.username" class="p-error">
                {{ validationErrors.username }}
              </small>
            </div>

            <!-- API Token Field - Only show when editing -->
            <div v-if="!readOnly" class="flex flex-col gap-1">
              <label for="apiToken" class="block text-sm font-medium text-slate-700">
                API Token <span class="text-red-500">*</span>
                <span v-if="isEditing && hasExistingToken" class="text-amber-600 text-xs">(leave empty to keep current)</span>
              </label>
              <Password
                id="apiToken"
                v-model="formData.apiToken"
                :disabled="!isEditing"
                :class="{ 'p-invalid': validationErrors.apiToken }"
                placeholder="Enter API token"
                :toggleMask="true"
                :feedback="false"
                class="w-full"
                inputClass="w-full"
              />
              <small v-if="validationErrors.apiToken" class="p-error">
                {{ validationErrors.apiToken }}
              </small>
              <small class="text-slate-500 text-xs mt-1">
                Generate an API token from Jenkins user settings
              </small>
            </div>
          </div>
        </div>

        <Divider />

        <!-- Configuration -->
        <div class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <!-- Instructions Field -->
            <div class="space-y-2">
              <label for="instructions" class="block text-sm font-medium text-slate-700">
                Instructions
              </label>
              <InputGroup v-if="isEditing">
                <Textarea
                  id="instructions"
                  v-model="formData.instructions"
                  :disabled="!isEditing"
                  rows="5"
                  placeholder="Optional instructions for log analysis..."
                  class="border-r-0 rounded-r-none"
                  style="flex: 1;"
                />
                <InputGroupAddon>
                  <Button
                    icon="pi pi-window-maximize"
                    severity="secondary"
                    size="small"
                    text
                    @click="openExpandDialog('instructions', 'Instructions')"
                    v-tooltip="'Expand for more space'"
                  />
                </InputGroupAddon>
              </InputGroup>
              <Textarea
                v-else
                id="instructions"
                v-model="formData.instructions"
                :disabled="!isEditing"
                rows="5"
                placeholder="Optional instructions for log analysis..."
                class="w-full"
              />
              <small class="text-slate-500 text-xs">
                Instructions for AI analysis of Jenkins logs from this server
              </small>
            </div>

            <!-- Response Format Field -->
            <div class="space-y-2">
              <label for="responseFormat" class="block text-sm font-medium text-slate-700">
                Response Format
              </label>
              <InputGroup v-if="isEditing">
                <Textarea
                  id="responseFormat"
                  v-model="formData.responseFormat"
                  :disabled="!isEditing"
                  rows="5"
                  placeholder="Optional format for analysis responses..."
                  class="border-r-0 rounded-r-none"
                  style="flex: 1;"
                />
                <InputGroupAddon>
                  <Button
                    icon="pi pi-window-maximize"
                    severity="secondary"
                    size="small"
                    text
                    @click="openExpandDialog('responseFormat', 'Response Format')"
                    v-tooltip="'Expand for more space'"
                  />
                </InputGroupAddon>
              </InputGroup>
              <Textarea
                v-else
                id="responseFormat"
                v-model="formData.responseFormat"
                :disabled="!isEditing"
                rows="5"
                placeholder="Optional format for analysis responses..."
                class="w-full"
              />
              <small class="text-slate-500 text-xs">
                Expected response format for AI analysis results
              </small>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- Fixed Footer - Action Buttons and Metadata -->
    <div class="flex-shrink-0">
      <!-- Action Buttons -->
      <div v-if="isEditing" class="flex justify-end gap-3 pt-4 border-t border-slate-200">
        <Button
          @click="resetForm"
          label="Reset"
          severity="secondary"
          size="small"
        />
        <Button
          @click="validateAndSave"
          :loading="saving"
          label="Save Server"
          severity="primary"
          size="small"
          icon="pi pi-save"
        />
      </div>

      <!-- Read-only metadata -->
      <div v-if="!isEditing && initialValues.id" class="pt-4 border-t border-slate-200">
        <div class="text-xs text-slate-500 space-y-1">
          <div>Created: {{ formatDateTime(initialValues.createdAt) }}</div>
          <div>Updated: {{ formatDateTime(initialValues.updatedAt) }}</div>
        </div>
      </div>
    </div>

    <!-- Expand Dialog for Large Text Editing -->
    <Dialog
      v-model:visible="expandDialogVisible"
      :header="expandedFieldName"
      modal
      :style="{ width: '800px', maxWidth: '90vw' }"
      :closable="true"
    >
      <div class="flex flex-col gap-4">
        <Textarea
          v-model="expandedFieldContent"
          rows="20"
          class="w-full"
          :placeholder="`Enter ${expandedFieldName.toLowerCase()}...`"
        />
      </div>

      <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-200">
        <Button
          @click="cancelExpandedContent"
          label="Cancel"
          severity="secondary"
          size="small"
        />
        <Button
          @click="saveExpandedContent"
          label="Apply"
          severity="primary"
          size="small"
        />
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useVuelidate } from '@vuelidate/core'
import { required, minLength, maxLength } from '@vuelidate/validators'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import Divider from 'primevue/divider'
import Dialog from 'primevue/dialog'
import InputGroup from 'primevue/inputgroup'
import InputGroupAddon from 'primevue/inputgroupaddon'

// Props
const props = defineProps<{
  initialValues: Record<string, any>
  isEditing: boolean
  readOnly: boolean
  saving: boolean
}>()

// Emits
const emit = defineEmits<{
  'save-server': [payload: { valid: boolean, values: Record<string, unknown> }]
}>()

// Form data
const formData = ref({
  name: '',
  server: '',
  username: '',
  apiToken: '',
  instructions: '',
  responseFormat: ''
})

// Dialog state for expanded text editing
const expandDialogVisible = ref(false)
const expandedFieldName = ref('')
const expandedFieldContent = ref('')

// Track if there's an existing token for security handling
const hasExistingToken = computed(() => {
  return props.initialValues.hasExistingToken || false
})

// Simple URL validator
const simpleUrl = (value: string) => {
  if (!value) return true // Let required validator handle empty values
  return value.startsWith('http://') || value.startsWith('https://')
}

// Validation rules
const validationRules = computed(() => ({
  name: { required, minLength: minLength(1), maxLength: maxLength(100) },
  server: { required, simpleUrl },
  username: { required, minLength: minLength(1), maxLength: maxLength(100) },
  apiToken: {
    required: !hasExistingToken.value,
    minLength: minLength(hasExistingToken.value ? 0 : 1)
  },
  instructions: {},
  responseFormat: {}
}))

const v$ = useVuelidate(validationRules, formData)

// Validation errors
const validationErrors = computed(() => ({
  name: v$.value.name.$errors?.length ? (v$.value.name.$errors[0]?.$message || 'Invalid value') : '',
  server: v$.value.server.$errors?.length ? (v$.value.server.$errors[0]?.$message || 'Invalid value') : '',
  username: v$.value.username.$errors?.length ? (v$.value.username.$errors[0]?.$message || 'Invalid value') : '',
  apiToken: v$.value.apiToken.$errors?.length ? (v$.value.apiToken.$errors[0]?.$message || 'Invalid value') : '',
  instructions: '',
  responseFormat: ''
}))

// Initialize form data when initial values change
watch(() => props.initialValues, (newValues) => {
  if (newValues) {
    formData.value = {
      name: newValues.name || '',
      server: newValues.server || '',
      username: newValues.username || '',
      apiToken: '', // Never pre-populate for security
      instructions: newValues.instructions || '',
      responseFormat: newValues.responseFormat || ''
    }
  }
}, { immediate: true })

// Actions
const openExpandDialog = (fieldName: string, fieldLabel: string) => {
  expandedFieldName.value = fieldLabel
  expandedFieldContent.value = formData.value[fieldName as keyof typeof formData.value] as string
  expandDialogVisible.value = true
}

const saveExpandedContent = () => {
  const fieldName = expandedFieldName.value === 'Instructions' ? 'instructions' : 'responseFormat'
  formData.value[fieldName as keyof typeof formData.value] = expandedFieldContent.value
  expandDialogVisible.value = false
}

const cancelExpandedContent = () => {
  expandDialogVisible.value = false
}

const resetForm = () => {
  formData.value = {
    name: props.initialValues.name || '',
    server: props.initialValues.server || '',
    username: props.initialValues.username || '',
    apiToken: '', // Never pre-populate for security
    instructions: props.initialValues.instructions || '',
    responseFormat: props.initialValues.responseFormat || ''
  }
  v$.value.$reset()
}

const validateAndSave = async () => {
  const isValid = await v$.value.$validate()
  if (isValid) {
    emit('save-server', {
      valid: true,
      values: { ...formData.value }
    })
  }
}

const formatDateTime = (dateString?: string) => {
  if (!dateString) return 'Unknown'
  return new Date(dateString).toLocaleString()
}

onMounted(() => {
  resetForm()
})
</script>
