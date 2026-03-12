<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

const toast = useToast()
const authStore = useAuthStore()

const links: NavigationMenuItem[][] = [
    settingsNavLinks,
    [
        {
            label: 'Sign Out',
            icon: 'i-lucide-log-out',
            class: 'cursor-pointer',
            async onSelect(e) {
                e.preventDefault()
                try {
                    await authStore.signOut()
                    await navigateTo('/auth', { external: true })
                } catch (err) {
                    toast.add({
                        id: 'sign-out-error',
                        title: 'Failed to sign out',
                        description: getProblemDetailsMessage(err),
                        color: 'error',
                    })
                }
            },
        },
    ],
]
</script>

<template>
    <UDashboardPanel id="@me" :ui="{ body: 'lg:py-12' }">
        <template #header>
            <UDashboardNavbar title="Settings">
                <template #leading>
                    <UDashboardSidebarCollapse />
                </template>
            </UDashboardNavbar>

            <UDashboardToolbar>
                <UNavigationMenu :items="links" highlight variant="link" class="-mx-1 flex-1" />
            </UDashboardToolbar>
        </template>

        <template #body>
            <div class="flex flex-col gap-4 sm:gap-6 lg:gap-12 w-full lg:max-w-2xl mx-auto">
                <NuxtPage />
            </div>
        </template>
    </UDashboardPanel>
</template>
