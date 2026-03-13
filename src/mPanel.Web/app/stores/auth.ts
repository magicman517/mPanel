import type { SignInSchema } from '~/utils/schemas/sign-in'
import type { SignUpSchema } from '~/utils/schemas/sign-up'

export const useAuthStore = defineStore('auth', () => {
    const session = ref<Session | null>(null)

    const isAuthenticated = computed(() => session.value !== null)
    const isAdmin = computed(() => session.value?.roles.includes('Admin') ?? false)

    async function fetchSession() {
        try {
            session.value = await $fetch<Session>('/api/sessions/current', {
                method: 'GET',
            })
        } catch {
            session.value = null
        }
    }

    async function signIn(params: SignInSchema) {
        await $fetch('/api/auth/sign-in', {
            method: 'POST',
            body: params,
        })
    }

    async function signUp(params: SignUpSchema) {
        await $fetch('/api/users', {
            method: 'POST',
            body: params,
        })
        await signIn({
            identity: params.email,
            password: params.password,
        })
    }

    async function signOut() {
        if (!session.value) {
            return
        }

        await $fetch('/api/auth/sign-out', {
            method: 'POST',
        })
    }

    return {
        session,
        isAuthenticated,
        isAdmin,
        fetchSession,
        signIn,
        signUp,
        signOut,
    }
})
