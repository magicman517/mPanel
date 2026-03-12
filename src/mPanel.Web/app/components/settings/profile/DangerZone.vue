<script setup lang="ts">
const panelSettingsStore = usePanelSettingsStore()
</script>

<template>
  <UPageCard
    v-if="panelSettingsStore.publicSettings.allowAccountSelfDeletion"
    title="Danger Zone"
    description="You can delete your account here. This action is not reversible. All information related to this account will be deleted permanently."
    class="bg-linear-to-tl from-error/10 from-5% to-default"
  >
    <template #footer>
      <UButton label="Delete account" color="error" variant="outline" />
    </template>
  </UPageCard>
</template>
