import type { FetchError } from 'ofetch'

export const usePanelSettingsStore = defineStore('panel-settings', () => {
  const toast = useToast()

  const publicSettings = ref<PanelPublicSettings>({
    name: '',
    allowRegistration: false,
    allowAccountSelfDeletion: false,
  })
  const settings = ref<PanelSettings | null>(null)

  async function fetchPublicSettings() {
    try {
      publicSettings.value = await $fetch<PanelPublicSettings>('/api/settings/public', {
        method: 'GET',
      })
    } catch (err) {
      showError({
        status: (err as FetchError).status ?? 503,
        message: 'Failed to fetch panel settings',
        fatal: true,
      })
    }
  }

  async function fetchSettings() {
    try {
      settings.value = await $fetch<PanelSettings>('/api/settings', {
        method: 'GET',
      })
    } catch (err) {
      toast.add({
        id: 'fetch-settings-error',
        title: 'Failed to fetch settings',
        description: getProblemDetailsMessage(err),
        color: 'error',
      })
    }
  }

  return {
    publicSettings,
    settings,
    fetchPublicSettings,
    fetchSettings,
  }
})
