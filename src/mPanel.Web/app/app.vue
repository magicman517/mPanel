<script setup lang="ts">
import type { FetchError } from 'ofetch'

const colorMode = useColorMode()
const panelSettingsStore = usePanelSettingsStore()

try {
    await panelSettingsStore.fetchPublicSettings()
} catch (err) {
    showError({
        status: (err as FetchError).status ?? 503,
        statusText: 'Failed to fetch panel settings',
        fatal: true,
    })
}

const toaster = { expand: true, progress: false }
const color = computed(() => (colorMode.value === 'dark' ? '#1b1718' : 'white'))

useHead({
    meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { key: 'theme-color', name: 'theme-color', content: color },
    ],
    link: [{ rel: 'icon', href: '/favicon.ico' }],
    htmlAttrs: {
        lang: 'en',
    },
})

useSeoMeta({
    title: 'mPanel',
})
</script>

<template>
    <UApp :toaster="toaster">
        <NuxtLoadingIndicator />
        <NuxtLayout>
            <NuxtPage />
        </NuxtLayout>
    </UApp>
</template>
