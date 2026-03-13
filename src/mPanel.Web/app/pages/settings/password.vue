<script setup lang="ts">
import { type UpdatePasswordSchema, updatePasswordSchema } from '~/utils/schemas/current-user'
import type { FormSubmitEvent } from '@nuxt/ui'

const toast = useToast()
const currentUserStore = useCurrentUserStore()

const formState = reactive({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
})

const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmNewPassword = ref(false)

async function onSubmit(payload: FormSubmitEvent<UpdatePasswordSchema>) {
    try {
        await currentUserStore.updatePassword(payload.data)
        toast.add({
            id: 'update-password-success',
            title: 'Success',
            description: 'Your password has been updated',
            color: 'success',
        })
    } catch (err) {
        toast.add({
            id: 'update-password-error',
            title: 'Error',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UForm
        id="settings-password"
        :schema="updatePasswordSchema"
        :state="formState"
        @submit="onSubmit"
    >
        <UPageCard
            title="Password"
            description="Change your account password here"
            variant="naked"
            orientation="horizontal"
            class="mb-4"
        >
            <UButton
                form="settings-password"
                label="Save"
                icon="i-lucide-cloud-check"
                color="neutral"
                type="submit"
                class="w-fit lg:ms-auto cursor-pointer"
                loading-auto
            />
        </UPageCard>

        <UPageCard variant="subtle">
            <UFormField
                name="currentPassword"
                label="Current Password"
                description="Required to confirm your identity"
                required
                class="flex max-sm:flex-col justify-between items-start gap-4"
            >
                <UInput
                    v-model="formState.currentPassword"
                    :type="showCurrentPassword ? 'text' : 'password'"
                    autocomplete="off"
                    placeholder="••••••••"
                >
                    <template #trailing>
                        <UButton
                            :icon="showCurrentPassword ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                            color="neutral"
                            variant="link"
                            size="sm"
                            :aria-label="showCurrentPassword ? 'Hide password' : 'Show password'"
                            @click="showCurrentPassword = !showCurrentPassword"
                        />
                    </template>
                </UInput>
            </UFormField>

            <USeparator />

            <UFormField
                name="newPassword"
                label="New Password"
                description="Your new password"
                required
                class="flex max-sm:flex-col justify-between items-start gap-4"
            >
                <UInput
                    v-model="formState.newPassword"
                    :type="showNewPassword ? 'text' : 'password'"
                    autocomplete="off"
                    placeholder="••••••••"
                >
                    <template #trailing>
                        <UButton
                            :icon="showNewPassword ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                            color="neutral"
                            variant="link"
                            size="sm"
                            :aria-label="showNewPassword ? 'Hide password' : 'Show password'"
                            @click="showNewPassword = !showNewPassword"
                        />
                    </template>
                </UInput>
            </UFormField>

            <USeparator />

            <UFormField
                name="confirmNewPassword"
                label="Confirm New Password"
                description="Re-enter your new password to confirm it"
                required
                class="flex max-sm:flex-col justify-between items-start gap-4"
            >
                <UInput
                    v-model="formState.confirmNewPassword"
                    :type="showConfirmNewPassword ? 'text' : 'password'"
                    autocomplete="off"
                    placeholder="••••••••"
                >
                    <template #trailing>
                        <UButton
                            :icon="showConfirmNewPassword ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                            color="neutral"
                            variant="link"
                            size="sm"
                            :aria-label="showConfirmNewPassword ? 'Hide password' : 'Show password'"
                            @click="showConfirmNewPassword = !showConfirmNewPassword"
                        />
                    </template>
                </UInput>
            </UFormField>
        </UPageCard>
    </UForm>
</template>
