<script setup lang="ts">
const currentUserStore = useCurrentUserStore()
</script>

<template>
  <template v-if="!currentUserStore.pending && currentUserStore.user">
    <SettingsProfileUpdateProfileDataForm />
    <SettingsProfileDangerZone />
  </template>

  <template v-else>
    <UPageCard variant="naked" orientation="horizontal" class="mb-0">
      <template #title>
        <USkeleton class="h-5 w-16" />
      </template>
      <template #description>
        <USkeleton class="h-3.5 w-72 mt-1" />
        <USkeleton class="h-3.5 w-56 mt-1.5" />
      </template>
      <USkeleton class="h-9 w-32 lg:ms-auto" />
    </UPageCard>

    <UPageCard variant="subtle">
      <div
        v-for="(widths, i) in [
          ['w-12', 'w-52'],
          ['w-20', 'w-60'],
          ['w-12', 'w-52'],
          ['w-10', 'w-28'],
        ]"
        :key="i"
      >
        <USeparator v-if="i > 0" class="my-0" />
        <div class="flex max-sm:flex-col justify-between items-start gap-4 py-4">
          <div class="flex flex-col gap-1.5">
            <USkeleton class="h-4" :class="widths[0]" />
            <USkeleton class="h-3.5" :class="widths[1]" />
          </div>
          <USkeleton v-if="i === 2" class="size-10 rounded-full" />
          <USkeleton v-else class="h-8 w-48 rounded-md" />
        </div>
      </div>
    </UPageCard>
  </template>
</template>
