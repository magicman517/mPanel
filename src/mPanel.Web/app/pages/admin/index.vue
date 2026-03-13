<script setup lang="ts">
import {
    type UpdatePanelSettingsSchema,
    updatePanelSettingsSchema,
} from '~/utils/schemas/panel-settings'
import type { FormSubmitEvent } from '@nuxt/ui'

const toast = useToast()

const { data, error, refresh } = await useFetch<PanelSettings>('/api/settings', {
    method: 'GET',
})

const formState = reactive({
    name: data.value!.name,
    url: data.value?.url ?? undefined,
    allowRegistration: data.value!.allowRegistration,
    allowAccountSelfDeletion: data.value!.allowAccountSelfDeletion,
    smtp: {
        host: data.value?.smtp.host ?? undefined,
        port: data.value?.smtp.port ?? 587,
        username: data.value?.smtp.username ?? undefined,
        password: data.value?.smtp.password ?? undefined,
        from: data.value?.smtp.from ?? undefined,
    },
})

async function onSubmit(payload: FormSubmitEvent<UpdatePanelSettingsSchema>) {
    try {
        await $fetch('/api/settings', {
            method: 'PUT',
            body: payload.data,
        })
        await refresh()
        toast.add({
            id: 'settings-update-success',
            title: 'Success',
            description: 'Settings updated successfully',
            color: 'success',
        })
    } catch (err) {
        toast.add({
            id: 'settings-update-error',
            title: 'Error',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UEmpty
        v-if="error"
        icon="i-lucide-circle-question-mark"
        title="Failed to load settings"
        :description="getProblemDetailsMessage(error)"
        variant="naked"
        :actions="[
            {
                icon: 'i-lucide-refresh-cw',
                label: 'Refresh',
                color: 'neutral',
                variant: 'subtle',
                async onClick() {
                    await refresh()
                },
            },
        ]"
    />

    <UForm
        v-else
        id="admin-settings"
        :schema="updatePanelSettingsSchema"
        :state="formState"
        @submit="onSubmit"
    >
        <UPageCard
            title="Settings"
            description="Manage you panel settings here"
            variant="naked"
            orientation="horizontal"
            class="mb-4"
        >
            <UButton
                form="admin-settings"
                label="Save"
                icon="i-lucide-cloud-check"
                color="neutral"
                type="submit"
                class="w-fit lg:ms-auto cursor-pointer"
                loading-auto
            />
        </UPageCard>

        <UPageCard title="General" variant="subtle" class="mb-4">
            <UFormField
                name="name"
                label="Panel Name"
                description="The name of your panel, which will be displayed"
                required
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.name"
                    type="text"
                    autocomplete="off"
                    placeholder="mPanel"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="url"
                label="Panel URL"
                description="URL of your panel, which will be used in emails"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.url"
                    type="url"
                    autocomplete="off"
                    placeholder="https://mPanel.example.com"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="allowRegistration"
                label="Allow Registration"
                description="Whether users can register on your panel"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <USwitch
                    v-model="formState.allowRegistration"
                    :label="formState.allowRegistration ? 'Yes' : 'No'"
                    class="w-full"
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="allowAccountSelfDeletion"
                label="Allow Account Self Deletion"
                description="Whether users can delete their own accounts"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4 w-full"
                loading-auto
            >
                <USwitch
                    v-model="formState.allowAccountSelfDeletion"
                    :label="formState.allowAccountSelfDeletion ? 'Yes' : 'No'"
                    class="w-full"
                    loading-auto
                />
            </UFormField>
        </UPageCard>

        <UPageCard title="SMTP" variant="subtle">
            <UFormField
                name="smtp.host"
                label="Host"
                class="flex max-sm:flex-col justify-between sm:items-center gap-2"
                loading-auto
            >
                <UInput
                    v-model="formState.smtp.host"
                    type="text"
                    autocomplete="off"
                    placeholder="smtp.example.com"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="smtp.port"
                label="Port"
                class="flex max-sm:flex-col justify-between sm:items-center gap-2"
                loading-auto
            >
                <!-- i have no idea how to make this full width. its good on mobile tho -->
                <UInput
                    v-model="formState.smtp.port"
                    type="number"
                    min="0"
                    max="65535"
                    placeholder="587"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="smtp.username"
                label="Username"
                class="flex max-sm:flex-col justify-between sm:items-center gap-2"
                loading-auto
            >
                <UInput
                    v-model="formState.smtp.username"
                    type="text"
                    autocomplete="off"
                    placeholder="username"
                    class="w-full"
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="smtp.password"
                label="Password"
                class="flex max-sm:flex-col justify-between sm:items-center gap-2"
                loading-auto
            >
                <UInput
                    v-model="formState.smtp.password"
                    type="password"
                    autocomplete="off"
                    placeholder="password"
                    class="w-full"
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="smtp.from"
                label="From"
                class="flex max-sm:flex-col justify-between sm:items-center gap-2"
                loading-auto
            >
                <UInput
                    v-model="formState.smtp.from"
                    type="text"
                    autocomplete="off"
                    placeholder="noreply@example.com"
                    class="w-full"
                    loading-auto
                />
            </UFormField>
        </UPageCard>
    </UForm>
</template>
