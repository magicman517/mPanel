<script setup lang="ts">
import { useIntervalFn } from '@vueuse/core'
import type { Node, PagedResponse } from '~/types'

const route = useRoute()
const pageSize = 20

const page = computed(() => Number(route.query.page) || 1)

const { data, refresh, error } = await useFetch<PagedResponse<Node>>('/api/nodes', {
    query: { page, pageSize },
})

useIntervalFn(refresh, 10_000)

function isNodeOnline(node: Node): boolean {
    if (!node.lastHeartbeat) return false

    const lastMs = Date.parse(node.lastHeartbeat)
    if (Number.isNaN(lastMs)) return false

    const ageMs = Date.now() - lastMs
    return ageMs <= 30_000
}
</script>

<template>
    <UEmpty
        v-if="error"
        icon="i-lucide-circle-question-mark"
        title="Error"
        description="An error occurred while fetching nodes. Please try again"
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
        v-else-if="!data?.totalCount"
        icon="i-lucide-circle-question-mark"
        title="No nodes found"
        description="It looks like you haven't added any nodes yet. Click the button below to add one"
        variant="naked"
        :actions="[
            {
                icon: 'i-lucide-plus',
                label: 'Add node',
                class: 'cursor-pointer',
                to: '/admin/nodes/create',
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

    <div v-else class="flex flex-col gap-4">
        <UPageCard
            title="Nodes"
            description="Manage nodes here"
            variant="naked"
            orientation="horizontal"
        >
            <UButton
                label="Add Node"
                icon="i-lucide-plus"
                color="neutral"
                class="w-fit lg:ms-auto"
                to="/admin/nodes/create"
            />
        </UPageCard>

        <UCard v-for="node in data?.items" :key="node.id">
            <template #header>
                <div class="flex gap-3 items-center">
                    <div
                        class="relative flex items-center justify-center bg-elevated p-2 rounded-sm"
                    >
                        <UIcon name="i-lucide-server" class="size-8" />
                        <span class="absolute top-0 right-0 flex h-2 w-2">
                            <span
                                class="animate-ping absolute inline-flex h-full w-full rounded-full"
                                :class="isNodeOnline(node) ? 'bg-success' : 'bg-error'"
                            />
                            <span
                                class="relative inline-flex rounded-full h-2 w-2"
                                :class="isNodeOnline(node) ? 'bg-success' : 'bg-error'"
                            />
                        </span>
                    </div>
                    <div class="flex flex-col gap-0">
                        <p class="text-lg font-semibold">{{ node.name }}</p>
                        <p class="text-xs text-muted">{{ node.id }}</p>
                    </div>
                </div>
            </template>

            <template #default>
                <div class="flex flex-col w-full gap-3">
                    <div class="flex flex-col gap-1 w-full">
                        <div class="flex w-full gap-1 items-center justify-between">
                            <div class="flex items-center gap-1">
                                <UIcon name="i-lucide-cpu" />
                                <p class="text-sm text-muted fontfont-semibold">CPU Usage</p>
                            </div>
                            <p class="text-xs text-muted fontfont-semibold">
                                {{ node.lastHeartbeatCpuUsage ?? 'unknown' }}
                            </p>
                        </div>
                        <UProgress :model-value="node.lastHeartbeatCpuUsage ?? 0" :max="100" />
                    </div>

                    <div class="flex flex-col gap-1 w-full">
                        <div class="flex w-full gap-1 items-center justify-between">
                            <div class="flex items-center gap-1">
                                <UIcon name="i-lucide-memory-stick" />
                                <p class="text-sm text-muted fontfont-semibold">Memory Usage</p>
                            </div>
                            <p class="text-xs text-muted fontfont-semibold">
                                {{ node.lastHeartbeatMemoryMb ?? 'unknown' }}
                            </p>
                        </div>
                        <UProgress :model-value="node.lastHeartbeatMemoryMb ?? 0" :max="100" />
                    </div>

                    <div class="flex w-full items-center justify-end">
                        <UButton
                            label="Edit"
                            icon="i-lucide-edit"
                            variant="subtle"
                            class="cursor-pointer"
                            size="sm"
                            :to="`/admin/nodes/${node.id}`"
                        />
                    </div>
                </div>
            </template>
        </UCard>

        <div v-if="data && data.totalPages > 1" class="flex justify-center mt-6">
            <UPagination
                :page="page"
                :total="data.totalCount"
                :items-per-page="pageSize"
                :to="(p) => ({ query: { page: p } })"
                show-edges
            />
        </div>
    </div>
</template>
