<script setup lang="ts">
import type { CommandPaletteItem, NavigationMenuItem } from '@nuxt/ui'

const currentUserStore = useCurrentUserStore()
callOnce(currentUserStore.fetchCurrentUser)

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
      children: [
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
    },
  ],
]

const groups = computed(() => [
  {
    id: 'links',
    label: 'Go to',
    items: links.flat() as CommandPaletteItem[],
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
        <UDashboardSearchButton :collapsed="collapsed" class="bg-transparent ring-default" />

        <UNavigationMenu
          :collapsed="collapsed"
          :items="links[0]"
          orientation="vertical"
          tooltip
          popover
        />

        <UNavigationMenu
          :collapsed="collapsed"
          :items="links[1]"
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
