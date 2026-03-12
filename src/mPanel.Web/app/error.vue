<script setup lang="ts">
const props = defineProps<{
  error: NuxtError
}>()

const handleError = () => clearError({ redirect: '/' })
</script>

<template>
  <UApp>
    <UError :error="error" @clear="handleError" />
  </UApp>
</template>
