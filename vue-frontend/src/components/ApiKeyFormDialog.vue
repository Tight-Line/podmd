<template>
  <div>
    <form @submit.prevent="handleSubmit">
      <div class="space-y-4">
        <!-- Permissions Field -->
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-2">
            Permissions
          </label>
          <div class="space-y-2">
            <div class="flex items-center gap-4">
              <div class="flex items-center gap-2">
                <Checkbox
                  id="perm-read"
                  v-model="formData.permissions.read"
                  :loading="submitting"
                  binary
                />
                <label for="perm-read" class="text-sm font-medium text-slate-700">
                  Read
                </label>
              </div>
              <div class="flex items-center gap-2">
                <Checkbox
                  id="perm-write"
                  v-model="formData.permissions.write"
                  :loading="submitting"
                  binary
                />
                <label for="perm-write" class="text-sm font-medium text-slate-700">
                  Write
                </label>
              </div>
              <div class="flex items-center gap-2">
                <Checkbox
                  id="perm-admin"
                  v-model="formData.permissions.admin"
                  :loading="submitting"
                  binary
                />
                <label for="perm-admin" class="text-sm font-medium text-slate-700">
                  Admin
                </label>
              </div>
            </div>
          </div>
          <small v-if="errors.permissions" class="text-red-600">
            {{ errors.permissions }}
          </small>
        </div>

        <!-- Usage Limit Field -->
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-2">
            Usage Limit (optional)
          </label>
          <InputNumber
            v-model="formData.usageLimit"
            placeholder="Unlimited"
            :min="0"
            class="w-full"
            :class="{ 'p-invalid': errors.usageLimit }"
            :loading="submitting"
          />
          <small class="text-slate-500 text-xs mt-1 block">
            Leave empty for unlimited usage
          </small>
          <small v-if="errors.usageLimit" class="text-red-600">
            {{ errors.usageLimit }}
          </small>
        </div>


      </div>

      <!-- Form Actions -->
      <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-200">
        <Button
          @click="handleCancel"
          label="Cancel"
          severity="secondary"
          :disabled="submitting"
          size="small"
        />
        <Button
          type="submit"
          :label="submitButtonText"
          :loading="submitting"
          :disabled="submitting"
          :severity="isCreating ? 'success' : 'primary'"
          size="small"
        />
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, defineEmits, defineProps } from 'vue'
import { useToast } from 'primevue/usetoast'
import InputNumber from 'primevue/inputnumber'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import { authenticatedApi } from '../api/authenticatedApi'
import type { ApiKeyDto, CreateApiKeyRequest, UpdateApiKeyRequest } from '../api/Api'

// Permission interface
interface PermissionSettings {
  read: boolean
  write: boolean
  admin: boolean
}

// Props
interface Props {
  visible: boolean
  apiKey?: ApiKeyDto | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  save: [data: any]
  cancel: []
}>()

const toast = useToast()

// Form state
const formData = reactive({
  permissions: {
    read: false,
    write: false,
    admin: false
  } as PermissionSettings,
  usageLimit: undefined as number | undefined
})

const errors = reactive<Record<string, string>>({})
const submitting = ref(false)
const generatedApiKey = ref<string>('')

// Computed properties
const isCreating = computed(() => !props.apiKey)
const submitButtonText = computed(() => isCreating.value ? 'Create API Key' : 'Update API Key')

// Helper function to parse permissions from JSON string
const parsePermissions = (permissionsJson: string | null | undefined): PermissionSettings => {
  try {
    if (!permissionsJson) return { read: true, write: false, admin: false }
    const parsed = JSON.parse(permissionsJson)
    return {
      read: Boolean(parsed.read),
      write: Boolean(parsed.write),
      admin: Boolean(parsed.admin)
    }
  } catch {
    return { read: true, write: false, admin: false }
  }
}

// Watch for visible prop changes (populate form when editing)
watch(() => props.visible, (visible) => {
  if (visible) {
    if (props.apiKey) {
      // Populate form for editing
      formData.permissions = parsePermissions(props.apiKey.permissions)
      formData.usageLimit = props.apiKey.usageLimit ?? undefined
    } else {
      // Reset form for creating - default to read-only permissions
      formData.permissions = { read: true, write: false, admin: false }
      formData.usageLimit = undefined
      generatedApiKey.value = ''
    }
    // Clear errors on dialog open
    (Object.keys(errors) as string[]).forEach(key => {
      delete errors[key]
    })
  }
})

// Methods
const validateForm = (): boolean => {
  // Clear all errors
  (Object.keys(errors) as string[]).forEach(key => {
    delete errors[key]
  })

  if (!formData.permissions) {
    errors.permissions = 'Permissions are required'
  }

  return Object.keys(errors).length === 0
}

const handleSubmit = async () => {
  if (!validateForm()) return

  submitting.value = true
  try {
    if (isCreating.value) {
      // Create new API key - this would call the backend API
      // For now, we'll simulate the response
      await createApiKey()
    } else {
      // Update existing API key
      await updateApiKey()
    }
  } catch (error: any) {
    console.error('Error submitting API key form:', error)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.response?.data?.detail || 'Failed to save API key',
      life: 5000
    })
  } finally {
    submitting.value = false
  }
}

const createApiKey = async () => {
  const createData: CreateApiKeyRequest = {
    permissions: JSON.stringify(formData.permissions),
    usageLimit: formData.usageLimit
  }

  const response = await authenticatedApi.api.apikeysCreate(createData, {})
  generatedApiKey.value = response.data.key!

  emit('save', {
    ...formData,
    id: response.data.id,
    key: generatedApiKey.value
  })
}

const updateApiKey = async () => {
  if (!props.apiKey?.id) return

  const currentPermissionsString = JSON.stringify(formData.permissions)
  const originalPermissionsString = props.apiKey.permissions || JSON.stringify({ read: false, write: false, admin: false })

  const updateData: UpdateApiKeyRequest = {
    permissions: currentPermissionsString !== originalPermissionsString ? currentPermissionsString : undefined,
    usageLimit: formData.usageLimit !== props.apiKey.usageLimit ? formData.usageLimit : undefined,
    status: props.apiKey.status // Keep current status, or add status management if needed
  }

  await authenticatedApi.api.apikeysUpdate(props.apiKey.id, updateData, {})

  emit('save', {
    ...props.apiKey,
    permissions: currentPermissionsString,
    usageLimit: formData.usageLimit
  })
}

const handleCancel = () => {
  emit('cancel')
}
</script>
