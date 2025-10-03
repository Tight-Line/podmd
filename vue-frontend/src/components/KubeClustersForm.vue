<template>
  <div class="h-full flex flex-col">
    <!-- Scrollable Form Content -->
    <div class="flex-1 overflow-y-auto">
      <Form
        v-slot="$form"
        :initial-values="initialValues"
        :resolver="validationSchema"
        @submit="handleSubmit"
        class="space-y-6 pb-4"
      >
        <!-- Basic Information -->
        <div class="space-y-4">

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <!-- Name Field -->
            <div class="flex flex-col gap-1">
              <label for="name" class="block text-sm font-medium text-slate-700">
                Cluster Name <span class="text-red-500">*</span>
              </label>
              <InputText
                id="name"
                name="name"
                type="text"
                placeholder="production-k8s"
                fluid
                :readonly="readOnly"
                :class="{ 'bg-slate-50 text-slate-500': readOnly }"
              />
              <Message v-if="$form.name?.invalid" severity="error" size="small" variant="simple">
                {{ $form.name.error?.message }}
              </Message>
            </div>

            <!-- Server URLs Field -->
            <div class="flex flex-col gap-1">
              <label for="server" class="block text-sm font-medium text-slate-700">
                Server URL <span class="text-red-500">*</span>
              </label>
              <InputText
                id="server"
                name="server"
                type="text"
                placeholder="https://api.k8s.example.com:6443"
                fluid
                :readonly="readOnly"
                :class="{ 'bg-slate-50 text-slate-500': readOnly }"
              />
              <Message v-if="$form.server?.invalid" severity="error" size="small" variant="simple">
                {{ $form.server.error?.message }}
              </Message>
            </div>
          </div>
        </div>

        <Divider />

        <!-- Authentication -->
        <div class="space-y-4">

          <!-- Bearer Token Field - Only show when editing -->
          <div v-if="!readOnly" class="flex flex-col gap-1">
            <label for="bearerToken" class="block text-sm font-medium text-slate-700">
              Bearer Token <span class="text-red-500">*</span>
            </label>
            <InputText
              id="bearerToken"
              name="bearerToken"
              placeholder="eyJhbGciOiJSUzI1NiIsImtpZCI6..."
              fluid
            />
            <Message v-if="$form.bearerToken?.invalid" severity="error" size="small" variant="simple">
              {{ $form.bearerToken.error?.message }}
            </Message>
          </div>

          <!-- Certificate Authority PEM -->
          <div class="flex flex-col gap-1">
            <label for="certificateAuthorityPem" class="block text-sm font-medium text-slate-700">
              Certificate Authority PEM
            </label>

            <!-- Show status badge when reading, input field when editing -->
            <div v-if="readOnly">
              <span
                :class="initialValues.hasCertificateAuthority ? 'text-green-600 bg-green-100' : 'text-slate-400 bg-slate-100'"
                class="px-3 py-2 rounded-md text-sm font-medium"
              >
                {{ initialValues.hasCertificateAuthority ? 'Certificate Set' : 'No Certificate' }}
              </span>
            </div>
            <div v-else-if="isEditing">
              <InputGroup>
                <Textarea
                  id="certificateAuthorityPem"
                  name="certificateAuthorityPem"
                  placeholder="Enter CA certificate in PEM format"
                  rows="5"
                  class="border-r-0 rounded-r-none"
                  style="flex: 1;"
                  fluid
                />
                <InputGroupAddon>
                  <Button
                    icon="pi pi-window-maximize"
                    severity="secondary"
                    size="small"
                    text
                    @click="openExpandDialog('certificateAuthorityPem', 'Certificate Authority PEM')"
                    v-tooltip="'Expand for more space'"
                  />
                </InputGroupAddon>
              </InputGroup>
            </div>
            <div v-else>
              <Textarea
                id="certificateAuthorityPem"
                name="certificateAuthorityPem"
                placeholder="Enter CA certificate in PEM format"
                rows="5"
                fluid
              />
            </div>
          </div>
        </div>

        <Divider />

        <!-- Configuration -->
        <div class="space-y-4">

          <!-- Insecure Skip TLS Verify -->
          <div class="flex items-center">
            <Checkbox
              id="insecureSkipTlsVerify"
              name="insecureSkipTlsVerify"
              binary
              :disabled="readOnly"
            />
            <label for="insecureSkipTlsVerify" class="ml-2 text-sm text-slate-700">
              Skip TLS verification (not recommended for production)
            </label>
          </div>

          <!-- Default Namespace -->
          <div class="flex flex-col gap-1">
            <label for="defaultNamespace" class="block text-sm font-medium text-slate-700">
              Default Namespace
            </label>
            <InputText
              id="defaultNamespace"
              name="defaultNamespace"
              type="text"
              placeholder="default"
              fluid
              :readonly="readOnly"
              :class="{ 'bg-slate-50 text-slate-500': readOnly }"
            />
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <!-- Instructions Field -->
            <div class="flex flex-col gap-1">
              <label for="instructions" class="block text-sm font-medium text-slate-700">
                Instructions
              </label>
              <InputGroup v-if="isEditing">
                <Textarea
                  id="instructions"
                  name="instructions"
                  placeholder="Optional instructions for log analysis..."
                  rows="5"
                  class="border-r-0 rounded-r-none"
                  style="flex: 1;"
                  fluid
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
                name="instructions"
                placeholder="Optional instructions for log analysis..."
                rows="5"
                fluid
                :readonly="readOnly"
                :class="{ 'bg-slate-50 text-slate-500': readOnly }"
              />
              <small class="text-slate-500 text-xs">
                Instructions for AI analysis of Kubernetes logs from this cluster
              </small>
            </div>

            <!-- Response Format Field -->
            <div class="flex flex-col gap-1">
              <label for="responseFormat" class="block text-sm font-medium text-slate-700">
                Response Format
              </label>
              <InputGroup v-if="isEditing">
                <Textarea
                  id="responseFormat"
                  name="responseFormat"
                  placeholder="Optional format for analysis responses..."
                  rows="5"
                  class="border-r-0 rounded-r-none"
                  style="flex: 1;"
                  fluid
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
                name="responseFormat"
                placeholder="Optional format for analysis responses..."
                rows="5"
                fluid
                :readonly="readOnly"
                :class="{ 'bg-slate-50 text-slate-500': readOnly }"
              />
              <small class="text-slate-500 text-xs">
                Expected response format for AI analysis results
              </small>
            </div>
          </div>
        </div>
      </Form>
    </div>

    <!-- Fixed Footer - Action Buttons -->
    <div class="flex-shrink-0">
      <div v-if="isEditing && !readOnly" class="flex justify-end gap-3 pt-4 border-t border-slate-200">
        <!-- Only show cancel when creating (in dialog) -->
        <Button
          v-if="!visible"
          type="button"
          @click="$emit('close-dialog')"
          label="Cancel"
          class="hover:text-slate-800 hover:bg-slate-100 border-slate-300"
          size="small"
        />
        <Button
          type="submit"
          :loading="saving"
          :label="visible ? 'Save' : 'Create Cluster'"
          icon="pi pi-save"
          class="hover:text-slate-800 hover:bg-slate-100 border-slate-300"
          size="small"
        />
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
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { z } from 'zod'
import Form from '@primevue/forms/form'
import Message from 'primevue/message'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import Divider from 'primevue/divider'
import Dialog from 'primevue/dialog'
import InputGroup from 'primevue/inputgroup'
import InputGroupAddon from 'primevue/inputgroupaddon'

import { computed, ref } from 'vue'

const props = defineProps<{
  visible?: boolean
  isEditing: boolean
  readOnly?: boolean
  initialValues: Record<string, unknown>
  saving: boolean
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  'save-cluster': [formValues: { valid: boolean, values: Record<string, unknown> }]
  'close-dialog': []
}>()

// Dialog state for expanded text editing
const expandDialogVisible = ref(false)
const expandedFieldName = ref('')
const expandedFieldContent = ref('')

// Dynamic validation schema based on edit/create mode
const validationSchema = computed(() => {
  if (props.isEditing) {
    // All fields optional during edit
    return zodResolver(
      z.object({
        name: z.string().optional(),
        server: z.string().optional()
          .refine((val) => !val || val.startsWith('http://') || val.startsWith('https://'), {
            message: 'Server URL must start with http:// or https://.'
          }),
        bearerToken: z.string().optional(),
        certificateAuthorityPem: z.string().optional(),
        insecureSkipTlsVerify: z.boolean().optional(),
        defaultNamespace: z.string().optional(),
        instructions: z.string().optional(),
        responseFormat: z.string().optional()
      })
    )
  } else {
    // Required fields for create
    return zodResolver(
      z.object({
        name: z.string().min(1, { message: 'Cluster name is required.' }),
        server: z.string().min(1, { message: 'Server URL is required.' })
          .refine((val) => val.startsWith('http://') || val.startsWith('https://'), {
            message: 'Server URL must start with http:// or https://.'
          }),
        bearerToken: z.string().min(1, { message: 'Bearer token is required.' }),
        certificateAuthorityPem: z.string().optional(),
        insecureSkipTlsVerify: z.boolean().optional(),
        defaultNamespace: z.string().optional(),
        instructions: z.string().optional(),
        responseFormat: z.string().optional()
      })
    )
  }
})

const handleSubmit = (formValues: { valid: boolean, values: Record<string, unknown> }) => {
  // Emit the form submission with validation result
  emit('save-cluster', formValues)
}

// Dialog methods for expanded text editing
const openExpandDialog = (fieldName: string, fieldLabel: string) => {
  expandedFieldName.value = fieldLabel
  expandedFieldContent.value = props.initialValues[fieldName] as string || ''
  expandDialogVisible.value = true
}

const saveExpandedContent = () => {
  // Update the initialValues so the form sees the change
  if (props.initialValues && typeof props.initialValues === 'object') {
    const writableValues = props.initialValues as Record<string, unknown>
    const fieldName = expandedFieldName.value === 'Instructions' ? 'instructions' :
                     expandedFieldName.value === 'Response Format' ? 'responseFormat' : 'certificateAuthorityPem'
    writableValues[fieldName] = expandedFieldContent.value
  }
  expandDialogVisible.value = false
}

const cancelExpandedContent = () => {
  expandDialogVisible.value = false
}
</script>
