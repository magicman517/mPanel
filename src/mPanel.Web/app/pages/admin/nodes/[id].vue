<script setup lang="ts">
import type { FormSubmitEvent } from '@nuxt/ui'
import type { Node } from '~/types'
import { updateNodeSchema, type UpdateNodeSchema } from '~/utils/schemas/nodes'

const route = useRoute()
const id = route.params.id

const toast = useToast()

const { data, error, refresh } = await useFetch<Node>(`/api/nodes/${id}`)

const schemes = ['Http', 'Https']
const formState = reactive({
    id: data.value?.id,
    name: data.value?.name,
    scheme: data.value?.scheme ?? 'Http',
    address: data.value?.address,
    port: data.value?.port ?? 10001,
    alias: data.value?.alias ?? '',
    sftpPort: data.value?.sftpPort ?? 2022,
    sftpAlias: data.value?.sftpAlias ?? '',
    maxMemoryMb: data.value?.maxMemoryMb ?? 0,
    maxDiskMb: data.value?.maxDiskMb ?? 0,
    isMaintenanceMode: data.value?.isMaintenanceMode ?? false,
    isActive: data.value?.isActive ?? true,
})

async function onSubmit(payload: FormSubmitEvent<UpdateNodeSchema>) {
    try {
        await $fetch(`/api/nodes/${id}`, {
            method: 'PUT',
            body: payload.data,
        })
        toast.add({
            id: 'update-node-success',
            title: 'Success',
            description: 'Node settings have been updated',
            color: 'success',
        })
    } catch (err) {
        toast.add({
            id: 'update-node-error',
            title: 'Error',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UEmpty
        v-if="error || !data"
        icon="i-lucide-circle-question-mark"
        title="Node not found"
        description="This node couldn’t be loaded. It may no longer exist, or something went wrong while fetching it"
        variant="naked"
        :actions="[
            {
                icon: 'i-lucide-server',
                label: 'Nodes',
                variant: 'subtle',
                class: 'cursor-pointer',
                async onClick() {
                    await navigateTo('/admin/nodes')
                },
            },
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
        id="node-settings"
        :schema="updateNodeSchema"
        :state="formState"
        @submit="onSubmit"
    >
        <UPageCard title="Node Settings" variant="naked" orientation="horizontal" class="mb-4">
            <UButton
                form="node-settings"
                label="Save"
                icon="i-lucide-cloud-check"
                color="neutral"
                type="submit"
                class="w-fit lg:ms-auto cursor-pointer"
                loading-auto
            />
        </UPageCard>

        <UPageCard
            v-if="data.handshakeError"
            title="Connection issue detected"
            :description="data.handshakeError"
            class="mb-4"
            highlight
            highlight-color="error"
            spotlight
            spotlight-color="error"
        />

        <UPageCard title="General" variant="subtle" class="mb-4">
            <UFormField
                name="id"
                label="Node ID"
                description="The unique identifier for this node"
                required
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.id"
                    type="text"
                    autocomplete="off"
                    class="w-full"
                    loading-auto
                    readonly
                />
            </UFormField>

            <USeparator />

            <UFormField
                name="name"
                label="Node Name"
                description="The name of this node"
                required
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                loading-auto
            >
                <UInput
                    v-model="formState.name"
                    type="text"
                    autocomplete="off"
                    class="w-full"
                    loading-auto
                />
            </UFormField>
        </UPageCard>

        <UPageCard title="Connection" variant="subtle" class="mb-4">
            <UFormField
                name="scheme"
                label="Scheme"
                description="Scheme for the node"
                class="flex max-sm:flex-col justify-between sm:items-center gap-4"
                required
                loading-auto
            >
                <URadioGroup v-model="formState.scheme" :items="schemes" orientation="horizontal" />
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
