<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

const keys = defineModel<ApiKey[]>('keys', { required: true })

const UButton = resolveComponent('UButton')
const toast = useToast()

const pendingDelete = ref<Set<string>>(new Set())

const columns: TableColumn<ApiKey>[] = [
    {
        id: 'expand',
        cell({ row }) {
            return h(UButton, {
                color: 'neutral',
                variant: 'ghost',
                icon: 'i-lucide-chevron-down',
                class: 'cursor-pointer',
                square: true,
                'aria-label': 'Expand',
                ui: {
                    leadingIcon: [
                        'transition-transform',
                        row.getIsExpanded() ? 'duration-200 rotate-180' : '',
                    ],
                },
                onClick: () => row.toggleExpanded(),
            })
        },
    },
    {
        accessorKey: 'name',
        header: 'Name',
        cell({ row }) {
            const name = row.getValue<string | null>('name') ?? 'none'
            return name.length > 24 ? name.slice(0, 24) + '...' : name
        },
    },
    {
        accessorKey: 'prefix',
        header: 'Key',
        cell({ row }) {
            return row.getValue('prefix') + '*******'
        },
    },
    {
        accessorKey: 'expiresAt',
        header: 'Expires',
        cell({ row }) {
            const rawDate = row.getValue<string | null>('expiresAt')
            if (!rawDate) return 'never'
            const date = new Date(rawDate + 'T00:00:00')
            return new Intl.DateTimeFormat('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
            }).format(date)
        },
    },
    {
        id: 'delete',
        cell({ row }) {
            const isPending = pendingDelete.value.has(row.original.id)
            return h(UButton, {
                variant: 'ghost',
                color: 'error',
                icon: isPending ? 'i-lucide-check' : 'i-lucide-trash-2',
                class: 'cursor-pointer',
                square: true,
                async onClick() {
                    if (isPending) {
                        const next = new Set(pendingDelete.value)
                        next.delete(row.original.id)
                        pendingDelete.value = next
                        await deleteKey(row.original.id)
                    } else {
                        pendingDelete.value = new Set([...pendingDelete.value, row.original.id])
                        setTimeout(() => {
                            const next = new Set(pendingDelete.value)
                            next.delete(row.original.id)
                            pendingDelete.value = next
                        }, 3000)
                    }
                },
            })
        },
    },
]

async function deleteKey(id: string) {
    try {
        keys.value = await $fetch<ApiKey[]>('/api/api-keys/' + id, {
            method: 'DELETE',
        })
        toast.add({
            id: 'api-key-delete-success',
            title: 'Success',
            description: 'API key deleted successfully',
            color: 'success',
        })
    } catch (err) {
        toast.add({
            id: 'api-key-delete-error',
            title: 'Error',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UPageCard variant="subtle" class="overflow-x-auto">
        <UTable
            :data="keys"
            :columns="columns"
            :ui="{
                tr: 'data-[expanded=true]:bg-elevated/50',
                root: '[&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]',
            }"
        >
            <template #expanded="{ row }">
                <pre>{{ row.original }}</pre>
            </template>
        </UTable>
    </UPageCard>
</template>
