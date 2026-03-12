<script setup lang="ts">
import type { AuthFormField, FormSubmitEvent } from '@nuxt/ui'
import { signInSchema, type SignInSchema } from '~/utils/schemas/sign-in'

const toast = useToast()
const authStore = useAuthStore()

const fields: AuthFormField[] = [
    {
        name: 'identity',
        type: 'text',
        label: 'Email or Username',
        placeholder: 'cool_name69',
        autocomplete: 'username',
        autofocus: true,
        required: true,
    },
    {
        name: 'password',
        type: 'password',
        label: 'Password',
        autocomplete: 'current-password',
        placeholder: '••••••••',
        required: true,
    },
]

async function onSubmit(payload: FormSubmitEvent<SignInSchema>) {
    try {
        await authStore.signIn(payload.data)
        await navigateTo('/')
    } catch (err) {
        toast.add({
            id: 'sign-in-error',
            title: 'Failed to sign in',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UAuthForm
        @submit="onSubmit"
        :schema="signInSchema"
        :fields="fields"
        :submit="{
            icon: 'i-lucide-log-in',
            variant: 'subtle',
        }"
        loading-auto
        title="Welcome Back!"
        description="Enter your credentials to access your account"
        icon="i-lucide-user"
    />
</template>
