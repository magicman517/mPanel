<script setup lang="ts">
definePageMeta({ layout: false })
</script>

<template>
  <div class="w-full h-screen flex flex-col items-center justify-center">
    <div class="w-full max-w-sm px-4">
      <UPageCard>
        <div class="flex flex-col items-center text-center gap-6 py-2">
          <div class="flex items-center justify-center size-16 rounded-full bg-success/10">
            <UIcon name="i-lucide-circle-check" class="size-8 text-success" />
          </div>
          <div class="flex flex-col gap-2">
            <p class="text-lg font-semibold text-highlighted">Email Verified</p>
            <p class="text-sm text-muted">Your email address has been verified</p>
          </div>
          <UButton to="/" icon="i-lucide-house" label="Home" variant="subtle" />
        </div>
      </UPageCard>
    </div>
  </div>
</template>
