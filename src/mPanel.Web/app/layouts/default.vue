<script setup lang="ts">
import type { CommandPaletteItem, NavigationMenuItem } from '@nuxt/ui'
import type { FetchError } from 'ofetch'

const authStore = useAuthStore()
const currentUserStore = useCurrentUserStore()

try {
    await currentUserStore.fetchCurrentUser()
} catch (err) {
    showError({
        status: (err as FetchError).status ?? 503,
        statusText: 'Failed to fetch your profile data',
        fatal: true,
    })
}

const open = ref(false)

const links: NavigationMenuItem[][] = [
    [
        {
            label: 'Home',
            icon: 'i-lucide-house',
            to: '/',
            onSelect: () => {
                open.value = false
            },
        },
        {
            label: 'Settings',
            icon: 'i-lucide-settings',
            type: 'trigger',
            defaultOpen: true,
            children: settingsNavLinks,
        },
    ],
    [
        {
            label: 'Administration',
            icon: 'i-lucide-user-star',
            type: 'trigger',
            defaultOpen: true,
            children: adminNavLinks,
        },
    ],
    [
        {
            label: 'GitHub',
            icon: 'i-lucide-github',
            to: 'https://github.com/magicman517/mPanel',
            target: '_blank',
        },
    ],
]

const groups = computed(() => [
    {
        id: 'links',
        label: 'Go to',
        items: [
            ...links[0],
            ...(authStore.isAdmin ? links[1] : []),
            ...links[2],
        ] as CommandPaletteItem[],
    },
])
</script>

<template>
    <UDashboardGroup>
        <UDashboardSidebar
            :ui="{ footer: 'lg:border-t lg:border-default' }"
            v-model:open="open"
            id="default"
            class="bg-elevated/25"
            collapsible
            resizable
        >
            <template #default="{ collapsed }">
                <UDashboardSearchButton
                    :collapsed="collapsed"
                    class="bg-transparent ring-default"
                />

                <UNavigationMenu
                    :collapsed="collapsed"
                    :items="links[0]"
                    orientation="vertical"
                    tooltip
                    popover
                />

                <UNavigationMenu
                    v-if="authStore.isAdmin"
                    :collapsed="collapsed"
                    :items="links[1]"
                    orientation="vertical"
                    tooltip
                    popover
                />

                <UNavigationMenu
                    :collapsed="collapsed"
                    :items="links[2]"
                    orientation="vertical"
                    tooltip
                    class="mt-auto"
                />
            </template>

            <template #footer="{ collapsed }">
                <UserMenu :collapsed="collapsed" />
            </template>
        </UDashboardSidebar>

        <UDashboardSearch :groups="groups" />

        <slot />
    </UDashboardGroup>
</template>
