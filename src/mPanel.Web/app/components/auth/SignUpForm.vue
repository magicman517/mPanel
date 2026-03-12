<script setup lang="ts">
import type { AuthFormField, FormSubmitEvent } from '@nuxt/ui'
import { signUpSchema, type SignUpSchema } from '~/utils/schemas/sign-up'

const toast = useToast()
const authStore = useAuthStore()

const fields: AuthFormField[] = [
    {
        name: 'email',
        type: 'email',
        label: 'Email',
        placeholder: 'mail@example.com',
        autocomplete: 'email',
        autofocus: true,
        required: true,
    },
    {
        name: 'username',
        type: 'text',
        label: 'Username',
        placeholder: 'cool_name69',
        autocomplete: 'username',
        required: true,
    },
    {
        name: 'password',
        type: 'password',
        label: 'Password',
        placeholder: '••••••••',
        autocomplete: 'new-password',
        required: true,
    },
]

async function onSubmit(payload: FormSubmitEvent<SignUpSchema>) {
    try {
        await authStore.signUp(payload.data)
        await navigateTo('/')
    } catch (err) {
        toast.add({
            id: 'sign-up-error',
            title: 'Failed to sign up',
            description: getProblemDetailsMessage(err),
            color: 'error',
        })
    }
}
</script>

<template>
    <UAuthForm
        @submit="onSubmit"
        :schema="signUpSchema"
        :fields="fields"
        :submit="{
            icon: 'i-lucide-user-plus',
            variant: 'subtle',
        }"
        loading-auto
        title="Create an Account"
        description="Enter your details to create a new account"
        icon="i-lucide-user-plus"
    />
</template>
