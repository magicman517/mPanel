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
        label: 'Sessions',
        icon: 'i-lucide-monitor-smartphone',
        to: '/settings/sessions',
        exact: true,
    },
    {
        label: 'Password',
        icon: 'i-lucide-lock',
        to: '/settings/password',
        exact: true,
    },
]

export const adminNavLinks: NavigationMenuItem[] = [
    {
        label: 'Settings',
        icon: 'i-lucide-settings',
        to: '/admin',
        exact: true,
    },
    {
        label: 'Nodes',
        icon: 'i-lucide-server',
        to: '/admin/nodes',
        exact: true,
    },
    {
        label: 'Servers',
        icon: 'i-lucide-container',
        to: '/admin/servers',
        exact: true,
    },
    {
        label: 'Blueprints',
        icon: 'i-lucide-file-code',
        to: '/admin/blueprints',
        exact: true,
    },
    {
        label: 'Users',
        icon: 'i-lucide-users',
        to: '/admin/users',
        exact: true,
    },
    {
        label: 'Webhooks',
        icon: 'i-lucide-webhook',
        to: '/admin/webhooks',
        exact: true,
    },
    {
        label: 'Health',
        icon: 'i-lucide-heart-pulse',
        to: '/admin/health',
        exact: true,
    },
]
