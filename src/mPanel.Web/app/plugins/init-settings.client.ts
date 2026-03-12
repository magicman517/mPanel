export default defineNuxtPlugin(async () => {
  const store = usePanelSettingsStore()

  await store.fetchPublicSettings()
})
