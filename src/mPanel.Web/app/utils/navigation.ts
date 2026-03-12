import type { NavigationMenuItem } from '@nuxt/ui'

export const settingsNavLinks: NavigationMenuItem[] = [
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
]
