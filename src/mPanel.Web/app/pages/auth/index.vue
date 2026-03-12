<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

definePageMeta({
  layout: false,
})

const panelSettingsStore = usePanelSettingsStore()

const items = ref<TabsItem[]>([
  {
    label: 'Sign In',
    icon: 'i-lucide-lock',
    slot: 'sign-in',
  },
  {
    label: 'Sign Up',
    icon: 'i-lucide-user-plus',
    slot: 'sign-up',
  },
])
</script>

<template>
  <div class="w-full h-screen flex flex-col items-center justify-center">
    <div class="w-full max-w-100">
      <UPageCard v-if="panelSettingsStore.publicSettings.allowRegistration">
        <UTabs :items="items" :ui="{ trigger: 'grow' }" variant="link" class="gap-6">
          <template #sign-in>
            <AuthSignInForm />
          </template>

          <template #sign-up>
            <AuthSignUpForm />
          </template>
        </UTabs>
      </UPageCard>

      <UPageCard v-else>
        <AuthSignInForm />
      </UPageCard>
    </div>

    <div
      v-if="!panelSettingsStore.publicSettings.allowRegistration"
      class="fixed bottom-4 left-1/2 -translate-x-1/2 pointer-events-none"
    >
      <span class="text-sm text-muted opacity-50"> Registration is disabled </span>
    </div>
  </div>
</template>
