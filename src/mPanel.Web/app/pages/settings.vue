<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

const authStore = useAuthStore()

const links: NavigationMenuItem[][] = [
  [
    {
      label: 'Profile',
      icon: 'i-lucide-user',
      to: '/settings',
      exact: true,
    },
    {
      label: 'API Keys',
      icon: 'i-lucide-key-round',
      to: '/settings/api-keys',
      exact: true,
    },
    {
      label: 'Activity',
      icon: 'i-lucide-history',
      to: '/settings/activity',
      exact: true,
    },
    {
      label: 'Password',
      icon: 'i-lucide-lock',
      to: '/settings/password',
      exact: true,
    },
  ],
  [
    {
      label: 'Sign Out',
      icon: 'i-lucide-log-out',
      class: 'cursor-pointer',
      async onSelect(e) {
        e.preventDefault()
        await authStore.signOut()
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
