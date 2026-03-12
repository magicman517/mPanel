<script setup lang="ts">
import type { FormSubmitEvent } from '@nuxt/ui'
import { type CurrentUserSchema, currentUserSchema } from '~/utils/schemas/current-user'

const emailUpdateModalVisible = ref(false)
const currentUserStore = useCurrentUserStore()

const formState = reactive({
  email: currentUserStore.user?.email,
  username: currentUserStore.user?.username,
})

const roles = computed(() => currentUserStore.user?.roles.join(', '))

watch(
  () => currentUserStore.user,
  (newUser) => {
    if (newUser) {
      formState.email = newUser.email
      formState.username = newUser.username
    }
  },
  { immediate: true },
)

const newEmail = ref('')

async function onSubmit(payload: FormSubmitEvent<CurrentUserSchema>) {
  if (
    formState.email === currentUserStore.user?.email &&
    formState.username === currentUserStore.user?.username
  ) {
    return
  }

  const previousEmail = currentUserStore.user?.email

  const success = await currentUserStore.updateProfile(payload.data)

  if (!success) return

  if (payload.data.email !== previousEmail) {
    newEmail.value = payload.data.email
    emailUpdateModalVisible.value = true
  }
}
</script>

<template>
  <UModal v-model:open="emailUpdateModalVisible" :dismissible="false">
    <template #content>
      <div class="flex flex-col items-center text-center gap-6 p-8">
        <div class="flex items-center justify-center size-16 rounded-full bg-primary/10">
          <UIcon name="i-lucide-mail-check" class="size-8 text-primary" />
        </div>

        <div class="flex flex-col gap-2">
          <p class="text-lg font-semibold text-highlighted">Verify your new email</p>
          <p class="text-sm text-muted">
            A confirmation link has been sent to
            <span class="font-medium text-highlighted">{{ newEmail }}</span
            >. Click the link in that email to complete the update.
          </p>
        </div>

        <UButton label="Got it" variant="subtle" @click="emailUpdateModalVisible = false" />
      </div>
    </template>
  </UModal>

  <UForm id="settings" :schema="currentUserSchema" :state="formState" @submit="onSubmit">
    <UPageCard
      title="Profile"
      description="This information will be displayed publicly. You can save changes once every 5 minutes"
      variant="naked"
      orientation="horizontal"
      class="mb-4"
    >
      <UButton
        form="settings"
        label="Save"
        icon="i-lucide-cloud-check"
        color="neutral"
        type="submit"
        class="w-fit lg:ms-auto"
        loading-auto
      />
    </UPageCard>

    <UPageCard variant="subtle">
      <UFormField
        name="email"
        label="Email"
        description="Used for notifications and authentication"
        type="email"
        required
        class="flex max-sm:flex-col justify-between items-start gap-4"
      >
        <UInput
          v-model="formState.email"
          type="email"
          autocomplete="off"
          placeholder="mail@example.com"
        />
      </UFormField>

      <USeparator />

      <UFormField
        name="username"
        label="Username"
        description="Displayed publicly and used for authentication"
        required
        class="flex max-sm:flex-col justify-between items-start gap-4"
      >
        <UInput
          v-model="formState.username"
          type="text"
          autocomplete="off"
          placeholder="cool_name69"
        />
      </UFormField>

      <USeparator />

      <UFormField
        name="avatar"
        label="Avatar"
        description="Generated from Gravatar based on your email"
        class="flex max-sm:flex-col justify-between items-start gap-4"
      >
        <NuxtLink :to="currentUserStore.user?.avatarUrl" target="_blank" class="cursor-pointer">
          <UAvatar
            :src="currentUserStore.user?.avatarUrl"
            :alt="currentUserStore.user?.username"
            size="lg"
          />
        </NuxtLink>
      </UFormField>

      <USeparator />

      <UFormField
        name="roles"
        label="Roles"
        description="Your current roles"
        class="flex max-sm:flex-col justify-between items-start gap-4"
      >
        <UInput :value="roles" type="text" autocomplete="off" disabled />
      </UFormField>
    </UPageCard>
  </UForm>
</template>
