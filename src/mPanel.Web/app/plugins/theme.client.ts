const STORAGE_KEY = 'mPanel-theme'

interface ThemeConfig {
  primary?: string
  neutral?: string
}

export default defineNuxtPlugin(() => {
  const appConfig = useAppConfig()

  const saved = localStorage.getItem(STORAGE_KEY)
  if (saved) {
    try {
      const theme: ThemeConfig = JSON.parse(saved)
      if (theme.primary) appConfig.ui.colors.primary = theme.primary
      if (theme.neutral) appConfig.ui.colors.neutral = theme.neutral
    } catch {}
  }

  watch(
    () => [appConfig.ui.colors.primary, appConfig.ui.colors.neutral],
    ([primary, neutral]) => {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ primary, neutral }))
    },
  )
})
