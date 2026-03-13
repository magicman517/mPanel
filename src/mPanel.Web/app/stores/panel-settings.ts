export const usePanelSettingsStore = defineStore('panel-settings', () => {
    const publicSettings = ref<PanelPublicSettings>({
        name: '',
        allowRegistration: false,
        allowAccountSelfDeletion: false,
    })

    async function fetchPublicSettings() {
        publicSettings.value = await $fetch<PanelPublicSettings>('/api/settings/public', {
            method: 'GET',
        })
    }

    async function fetchSettings(): Promise<PanelSettings> {
        return await $fetch<PanelSettings>('/api/settings', {
            method: 'GET',
        })
    }

    return {
        publicSettings,
        fetchPublicSettings,
        fetchSettings,
    }
})
