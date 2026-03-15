<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import type { FormSubmitEvent } from '@nuxt/ui'
import { createNodeSchema, type CreateNodeSchema } from '~/utils/schemas/nodes'

const toast = useToast()
const { copy, copied } = useClipboard()

const formState = reactive({
    name: '',
    address: '',
    port: 10001,
    alias: '',
    sftpPort: 2022,
    sftpAlias: '',
    maxCpuPercent: 0,
    maxMemoryMb: 0,
    maxDiskMb: 0,
    isMaintenanceMode: false,
    isActive: true,
})

const newNodeModalOpen = ref(false)
const newNode = ref({
    id: '',
    token: '',
    deployCommand: '',
})

async function onSubmit(payload: FormSubmitEvent<CreateNodeSchema>) {
    try {
        const res = await $fetch<{ id: string; token: string; deployCommand: string }>(
            '/api/nodes',
            {
                method: 'POST',
                body: payload.data,
            },
        )
        newNode.value = res
        newNodeModalOpen.value = true
    } catch (err) {
        toast.add({
            id: 'create-node-error',
            title: 'Error',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UModal v-model:open="newNodeModalOpen" title="Node created" :dismissible="false">
        <template #body>
            <UAlert
                title="Use this command to run your node"
                description="This command contains node token. You will see it only once!"
                color="info"
                variant="outline"
                icon="i-lucide-circle-check"
                class="mb-4"
            />

            <UInput
                v-model="newNode.deployCommand"
                class="w-full"
                :ui="{ trailing: 'pr-0.5' }"
                disabled
            >
                <template #trailing>
                    <UButton
                        @click="copy(newNode.deployCommand)"
                        :color="copied ? 'success' : 'neutral'"
                        :icon="copied ? 'i-lucide-copy-check' : 'i-lucide-copy'"
                        variant="link"
                        size="sm"
                        aria-label="Copy to clipboard"
                        class="cursor-pointer"
                    />
                </template>
            </UInput>
        </template>

        <template #footer>
            <div class="flex w-full justify-center">
                <UButton label="Got it" icon="i-lucide-check" variant="subtle" to="/admin/nodes" />
            </div>
        </template>
    </UModal>

    <UForm id="create-node" :schema="createNodeSchema" :state="formState" @submit="onSubmit">
        <UPageCard
            title="Create Node"
            description="Fill in the form below to create a new node"
            variant="naked"
            orientation="horizontal"
            class="mb-4"
        >
            <UButton
                label="Add"
                icon="i-lucide-plus"
                color="neutral"
                class="w-fit lg:ms-auto cursor-pointer"
                type="submit"
            />
        </UPageCard>

        <UPageCard title="General" variant="subtle" class="mb-4">
            <UFormField
                name="name"
                label="Name"
                description="User-friendly name for the node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                required
                loading-auto
            >
                <UInput
                    v-model="formState.name"
                    type="text"
                    autocomplete="off"
                    placeholder="Frankfurt"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="address"
                label="Address"
                description="IP address or hostname of the node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                required
                loading-auto
            >
                <UInput
                    v-model="formState.address"
                    type="text"
                    autocomplete="off"
                    placeholder="10.0.0.1"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="port"
                label="Port"
                description="Port number for the node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.port"
                    type="number"
                    autocomplete="off"
                    placeholder="10001"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="alias"
                label="Alias"
                description="User-friendly alias for the node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.alias"
                    type="text"
                    autocomplete="off"
                    placeholder="frankfurt.example.com"
                    class="w-full"
                    loading-auto
                />
            </UFormField>
        </UPageCard>

        <UPageCard title="SFTP" variant="subtle" class="mb-4">
            <UFormField
                name="sftpPort"
                label="SFTP Port"
                description="Port number for the SFTP node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.sftpPort"
                    type="number"
                    autocomplete="off"
                    placeholder="2022"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="sftpAlias"
                label="SFTP Alias"
                description="User-friendly alias for the SFTP node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.sftpAlias"
                    type="text"
                    autocomplete="off"
                    placeholder="frankfurt.example.com"
                    class="w-full"
                    loading-auto
                />
            </UFormField>
        </UPageCard>

        <UPageCard title="Limits" description="0 for no limit" variant="subtle" class="mb-4">
            <UFormField
                name="maxMemoryMb"
                label="Max Memory MB"
                description="Maximum memory usage in MB"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.maxMemoryMb"
                    type="number"
                    autocomplete="off"
                    placeholder="0"
                    class="w-full"
                    loading-auto
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="maxDiskMb"
                label="Max Disk MB"
                description="Maximum disk usage in MB"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.maxDiskMb"
                    type="number"
                    autocomplete="off"
                    placeholder="0"
                    class="w-full"
                    loading-auto
                />
            </UFormField>
        </UPageCard>

        <UPageCard title="Status" variant="subtle" class="mb-4">
            <UFormField
                name="isMaintenanceMode"
                label="Maintenance Mode"
                description="Enable maintenance mode for the node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <USwitch v-model="formState.isMaintenanceMode" class="w-full" loading-auto />
            </UFormField>

            <USeparator />

            <UFormField
                name="isActive"
                label="Active"
                description="Enable the node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <USwitch v-model="formState.isActive" class="w-full" loading-auto />
            </UFormField>
        </UPageCard>
    </UForm>
</template>
