<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import type { DateValue } from '@internationalized/date'

const toast = useToast()

const newKeyModalOpen = ref(false)
const newKeyName = ref<string | null>(null)
const newKeyExpiresAt = ref<DateValue | null>(null)
const createdKey = ref<string | null>(null)
const keyVisible = ref(false)

const { copy, copied } = useClipboard({ source: createdKey })

const { data, error, refresh } = await useFetch<ApiKey[]>('/api/api-keys', {
    method: 'GET',
})

function openCreateModal() {
    newKeyName.value = null
    newKeyExpiresAt.value = null
    createdKey.value = null
    newKeyModalOpen.value = true
}

function closeModal() {
    newKeyModalOpen.value = false
    keyVisible.value = false
    newKeyName.value = null
    newKeyExpiresAt.value = null
    setTimeout(() => {
        createdKey.value = null
    }, 500)
}

async function createKey() {
    try {
        const response = await $fetch<{ key: string; meta: ApiKey }>('/api/api-keys', {
            method: 'POST',
            body: {
                name: newKeyName.value?.trim() || null,
                expiresAt: newKeyExpiresAt.value?.toString() ?? null,
            },
        })

        data.value = [response.meta, ...(data.value ?? [])]
        createdKey.value = response.key
    } catch (err) {
        toast.add({
            id: 'create-key-error',
            title: 'Error creating key',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UModal
        v-model:open="newKeyModalOpen"
        :title="createdKey ? 'API Key Created' : 'Create API Key'"
        :dismissible="false"
    >
        <template #body>
            <UForm
                v-if="!createdKey"
                id="create-key-form"
                class="flex flex-col md:flex-row gap-4"
                @submit="createKey"
            >
                <UFormField label="Key name" name="name" class="w-full">
                    <template #hint>
                        <span class="text-muted">optional</span>
                    </template>
                    <UInput v-model="newKeyName!" class="w-full" placeholder="My API Key" />
                </UFormField>

                <UFormField label="Expiration date" name="expires_at" class="w-full">
                    <template #hint>
                        <span class="text-muted">optional</span>
                    </template>
                    <UInputDate class="w-full" v-model="newKeyExpiresAt">
                        <template #trailing>
                            <UPopover>
                                <UButton
                                    color="neutral"
                                    variant="link"
                                    size="sm"
                                    icon="i-lucide-calendar"
                                    aria-label="Select a date"
                                    class="px-0"
                                />
                                <template #content>
                                    <UCalendar v-model="newKeyExpiresAt" class="p-2" />
                                </template>
                            </UPopover>
                        </template>
                    </UInputDate>
                </UFormField>
            </UForm>

            <div v-else class="flex flex-col gap-4">
                <UAlert
                    color="warning"
                    variant="subtle"
                    icon="i-lucide-triangle-alert"
                    title="Copy your key now"
                    description="This is the only time your API key will be visible. It cannot be recovered once you close this dialog."
                />

                <UFormField label="Your API key">
                    <div class="flex items-center gap-2 w-full">
                        <UInput
                            :model-value="keyVisible ? createdKey : '•'.repeat(32)"
                            readonly
                            class="flex-1 font-mono"
                        />
                        <UButton
                            :icon="keyVisible ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                            color="neutral"
                            variant="ghost"
                            size="sm"
                            :aria-label="keyVisible ? 'Hide key' : 'Reveal key'"
                            @click="keyVisible = !keyVisible"
                        />
                        <UButton
                            :icon="copied ? 'i-lucide-check' : 'i-lucide-copy'"
                            :color="copied ? 'success' : 'neutral'"
                            variant="ghost"
                            size="sm"
                            :aria-label="copied ? 'Copied' : 'Copy key'"
                            @click="copy()"
                        />
                    </div>
                </UFormField>
            </div>
        </template>

        <template #footer>
            <template v-if="!createdKey">
                <div class="flex w-full justify-center">
                    <UButton
                        label="Create"
                        icon="i-lucide-plus"
                        type="submit"
                        form="create-key-form"
                        class="cursor-pointer"
                        loading-auto
                    />
                </div>
            </template>
            <template v-else>
                <div class="flex w-full justify-center">
                    <UButton
                        label="Done"
                        variant="subtle"
                        @click="closeModal"
                        class="cursor-pointer"
                    />
                </div>
            </template>
        </template>
    </UModal>

    <UEmpty
        v-if="error"
        icon="i-lucide-circle-question-mark"
        title="Error"
        description="An error occurred while fetching API keys. Please try again"
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

    <UEmpty
        v-else-if="!data?.length"
        icon="i-lucide-circle-question-mark"
        title="No API keys found"
        description="It looks like you haven't created any API keys yet. Click the button below to create one"
        variant="naked"
        :actions="[
            {
                icon: 'i-lucide-plus',
                label: 'Create key',
                class: 'cursor-pointer',
                onClick() {
                    openCreateModal()
                },
            },
            {
                icon: 'i-lucide-refresh-cw',
                label: 'Refresh',
                color: 'neutral',
                variant: 'subtle',
                class: 'cursor-pointer',
                async onClick() {
                    await refresh()
                },
            },
        ]"
    />

    <div v-else>
        <UPageCard
            title="API Keys"
            description="Manage your API keys here"
            variant="naked"
            orientation="horizontal"
            class="mb-4"
        >
            <UButton
                label="New key"
                icon="i-lucide-plus"
                color="neutral"
                class="w-fit lg:ms-auto cursor-pointer"
                @click="openCreateModal"
            />
        </UPageCard>

        <SettingsApiKeysTable v-model:keys="data" />
    </div>
</template>
