<script setup lang="ts">
import type { NuxtError } from '#app'

useSeoMeta({
    title: 'Error',
})

defineProps<{
    error: NuxtError
}>()

const handleError = () => clearError({ redirect: '/' })
</script>

<template>
    <UApp>
        <UError :error="error" @clear="handleError" />
    </UApp>
</template>
