<script setup lang="ts">
import type { HealthResponse } from '~/types'

const { data, refresh, pending } = useFetch<HealthResponse>('/api/health/ready', {
    method: 'GET',
    ignoreResponseError: true,
})

const services = computed(() =>
    [
        { key: 'self', label: 'Panel API', icon: 'i-lucide-server' },
        { key: 'postgres', label: 'PostgreSQL', icon: 'i-simple-icons-postgresql' },
        { key: 'redis', label: 'Redis', icon: 'i-simple-icons-redis' },
    ].map((s) => {
        const check = data.value?.checks.find((c) => c.name === s.key)
        const healthy = check?.status === 'Healthy'
        return {
            ...s,
            healthy,
            statusText: healthy ? 'Healthy' : (check?.exception ?? 'Degraded'),
            exception: check?.exception ?? '',
        }
    }),
)
</script>

<template>
    <UPageCard>
        <UPageCard title="Health" variant="naked" orientation="horizontal" class="mb-2">
            <UButton
                label="Refresh"
                icon="i-lucide-refresh-cw"
                color="neutral"
                class="w-fit lg:ms-auto cursor-pointer"
                loading-auto
                @click="refresh()"
            />
        </UPageCard>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UCard v-for="service in services" :key="service.key" variant="subtle">
                <div v-if="pending" class="flex w-full items-center gap-4">
                    <USkeleton class="size-12 shrink-0" />
                    <div class="flex flex-col gap-2 w-full">
                        <USkeleton class="w-2/3 h-4" />
                        <USkeleton class="w-1/2 h-3" />
                    </div>
                </div>

                <div v-else class="flex w-full items-center gap-4">
                    <div class="relative flex items-center justify-center p-2 rounded-sm bg-muted">
                        <UIcon :name="service.icon" class="size-8" />
                        <span class="absolute top-0 right-0 flex h-2 w-2">
                            <span
                                class="animate-ping absolute inline-flex h-full w-full rounded-full"
                                :class="service.healthy ? 'bg-success' : 'bg-error'"
                            />
                            <span
                                class="relative inline-flex rounded-full h-2 w-2"
                                :class="service.healthy ? 'bg-success' : 'bg-error'"
                            />
                        </span>
                    </div>
                    <div class="flex flex-col gap-0">
                        <p class="text-xl font-semibold">{{ service.label }}</p>
                        <p class="text-sm text-muted line-clamp-1" :title="service.exception">
                            {{ service.statusText }}
                        </p>
                    </div>
                </div>
            </UCard>
        </div>
    </UPageCard>
</template>
