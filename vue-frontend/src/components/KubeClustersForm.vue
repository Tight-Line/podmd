<template>
  <Form
    v-slot="$form"
    :initial-values="initialValues"
    :resolver="validationSchema"
    @submit="handleSubmit"
    class="space-y-4"
  >
    <!-- Name Field -->
    <div class="flex flex-col gap-1">
      <label for="name" class="block text-sm font-medium text-slate-700 mb-1">
        Cluster Name
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
      <label for="server" class="block text-sm font-medium text-slate-700 mb-1">
        Server URL
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

    <!-- Bearer Token Field -->
    <div class="flex flex-col gap-1">
      <label for="bearerToken" class="block text-sm font-medium text-slate-700 mb-1">
        Bearer Token
      </label>

      <!-- Show input field when editing, status badge when reading -->
      <div v-if="readOnly">
        <span
          :class="initialValues.hasBearerToken ? 'text-green-600 bg-green-100' : 'text-slate-400 bg-slate-100'"
          class="px-3 py-2 rounded-md text-sm font-medium"
        >
          {{ initialValues.hasBearerToken ? 'Token Set' : 'No Token' }}
        </span>
      </div>
      <div v-else>
        <Textarea
          id="bearerToken"
          name="bearerToken"
          placeholder="Enter your Kubernetes service account token"
          rows="3"
          fluid
        />
        <Message v-if="$form.bearerToken?.invalid" severity="error" size="small" variant="simple">
          {{ $form.bearerToken.error?.message }}
        </Message>
      </div>
    </div>

    <!-- Certificate Authority PEM -->
    <div class="flex flex-col gap-1">
      <label for="certificateAuthorityPem" class="block text-sm font-medium text-slate-700 mb-1">
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
      <div v-else>
        <Textarea
          id="certificateAuthorityPem"
          name="certificateAuthorityPem"
          placeholder="Enter CA certificate in PEM format"
          rows="3"
          fluid
        />
      </div>
    </div>

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
      <label for="defaultNamespace" class="block text-sm font-medium text-slate-700 mb-1">
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

    <!-- Show buttons when editing and not read-only -->
    <div v-if="isEditing && !readOnly" class="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-200">
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
  </Form>
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

import { computed } from 'vue'

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
        defaultNamespace: z.string().optional()
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
        defaultNamespace: z.string().optional()
      })
    )
  }
})

const handleSubmit = (formValues: { valid: boolean, values: Record<string, unknown> }) => {
  // Emit the form submission with validation result
  emit('save-cluster', formValues)
}
</script>
