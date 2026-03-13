export default defineNuxtConfig({
    compatibilityDate: '2025-07-15',
    ssr: false,
    spaLoadingTemplate: 'spa-loading-template.html',
    devtools: { enabled: true },
    modules: ['@nuxt/ui', '@nuxt/eslint', '@pinia/nuxt'],
    css: ['~/assets/css/main.css'],
    imports: {
        dirs: ['types/*.ts'],
    },
    icon: {
        serverBundle: {
            collections: ['lucide'],
        },
    },
    vite: {
        server: {
            allowedHosts: ['host.docker.internal', 'aspire.dev.internal'],
        },
    },
})
