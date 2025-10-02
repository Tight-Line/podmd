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

    <!-- Certificate Authority PEM -->
    <div class="flex flex-col gap-1">
      <label for="certificateAuthorityPem" class="block text-sm font-medium text-slate-700 mb-1">
        Certificate Authority PEM
      </label>
      <Textarea
        id="certificateAuthorityPem"
        name="certificateAuthorityPem"
        placeholder="Enter CA certificate in PEM format"
        rows="3"
        fluid
      />
    </div>

    <!-- Insecure Skip TLS Verify -->
    <div class="flex items-center">
      <Checkbox
        id="insecureSkipTlsVerify"
        name="insecureSkipTlsVerify"
        binary
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
      />
    </div>

    <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-200">
      <Button
        type="button"
        @click="$emit('close-dialog')"
        label="Cancel"
        size="small"
        class="hover:text-slate-800 hover:bg-slate-100 border-slate-300"
      />
      <Button
        type="submit"
        :loading="saving"
        :label="isEditing ? 'Update Cluster' : 'Create Cluster'"
        icon="pi pi-save"
        size="small"
        class="hover:text-slate-800 hover:bg-slate-100 border-slate-300"
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
  visible: boolean
  isEditing: boolean
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
