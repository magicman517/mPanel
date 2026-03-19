<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

defineProps<{
    collapsed?: boolean
}>()

const toast = useToast()
const colorMode = useColorMode()
const appConfig = useAppConfig()
const authStore = useAuthStore()
const currentUserStore = useCurrentUserStore()

const colors = [
    'red',
    'orange',
    'amber',
    'yellow',
    'lime',
    'green',
    'emerald',
    'teal',
    'cyan',
    'sky',
    'blue',
    'indigo',
    'violet',
    'purple',
    'fuchsia',
    'pink',
    'rose',
]
const neutrals = ['slate', 'gray', 'zinc', 'stone']

const user = computed(() => ({
    name: authStore.session!.username,
    avatar: {
        src: currentUserStore.user?.avatarUrl,
        alt: authStore.session!.username,
    },
}))

const items = computed<DropdownMenuItem[][]>(() => [
    [
        {
            type: 'label',
            label: user.value.name,
            avatar: user.value.avatar,
        },
    ],
    [
        {
            label: 'Profile',
            icon: 'i-lucide-user',
            to: '/settings',
        },
        ...(authStore.isAdmin
            ? [
                  {
                      label: 'Administration',
                      icon: 'i-lucide-user-star',
                      to: '/admin',
                  },
              ]
            : []),
    ],

    [
        {
            label: 'Theme',
            icon: 'i-lucide-palette',
            children: [
                {
                    label: 'Primary',
                    slot: 'chip',
                    chip: appConfig.ui.colors.primary,
                    content: {
                        align: 'center',
                        collisionPadding: 16,
                    },
                    children: colors.map((color) => ({
                        type: 'checkbox',
                        checked: appConfig.ui.colors.primary === color,
                        label: color,
                        slot: 'chip',
                        chip: color,
                        class: 'cursor-pointer',
                        onSelect(e) {
                            e.preventDefault()
                            appConfig.ui.colors.primary = color
                        },
                    })),
                },
                {
                    label: 'Neutral',
                    slot: 'chip',
                    chip: appConfig.ui.colors.neutral,
                    content: {
                        align: 'center',
                        collisionPadding: 16,
                    },
                    children: neutrals.map((color) => ({
                        type: 'checkbox',
                        checked: appConfig.ui.colors.neutral === color,
                        label: color,
                        slot: 'chip',
                        chip: color,
                        class: 'cursor-pointer',
                        onSelect(e) {
                            e.preventDefault()
                            appConfig.ui.colors.neutral = color
                        },
                    })),
                },
            ],
        },
        {
            label: 'Appearance',
            icon: 'i-lucide-sun-moon',
            children: [
                {
                    label: 'Light',
                    icon: 'i-lucide-sun',
                    type: 'checkbox',
                    checked: colorMode.preference === 'light',
                    class: 'cursor-pointer',
                    onSelect(e) {
                        e.preventDefault()
                        colorMode.preference = 'light'
                    },
                },
                {
                    label: 'Dark',
                    icon: 'i-lucide-moon',
                    type: 'checkbox',
                    checked: colorMode.preference === 'dark',
                    class: 'cursor-pointer',
                    onSelect(e) {
                        e.preventDefault()
                        colorMode.preference = 'dark'
                    },
                },
            ],
        },
    ],
    [
        {
            label: 'Sign out',
            icon: 'i-lucide-log-out',
            class: 'cursor-pointer',
            async onSelect(e) {
                e.preventDefault()
                try {
                    await authStore.signOutAndNavigate()
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
])
</script>

<template>
    <UDropdownMenu
        :items="items"
        :content="{ align: 'center', collisionPadding: 12 }"
        :ui="{
            content: collapsed ? 'w-48' : 'w-(--reka-dropdown-menu-trigger-width)',
        }"
    >
        <UButton
            :square="collapsed"
            :ui="{ trailingIcon: 'text-dimmed' }"
            v-bind="{
                ...user,
                label: collapsed ? undefined : user?.name,
                trailingIcon: collapsed ? undefined : 'i-lucide-ellipsis-vertical',
            }"
            class="data-[state=open]:bg-elevated cursor-pointer"
            color="neutral"
            variant="ghost"
            block
        />

        <template #chip-leading="{ item }">
            <div class="inline-flex items-center justify-center shrink-0 size-5">
                <span
                    :style="{
                        '--chip-light': `var(--color-${(item as any).chip}-500)`,
                        '--chip-dark': `var(--color-${(item as any).chip}-400)`,
                    }"
                    class="rounded-full ring ring-bg bg-(--chip-light) dark:bg-(--chip-dark) size-2"
                />
            </div>
        </template>
    </UDropdownMenu>
</template>
