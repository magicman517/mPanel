import type { FetchError } from 'ofetch'
import type { CurrentUserSchema, UpdatePasswordSchema } from '~/utils/schemas/current-user'

export const useCurrentUserStore = defineStore('currentUser', () => {
    const authStore = useAuthStore()

    const user = ref<CurrentUser | null>(null)
    const pending = ref(false)

    async function fetchCurrentUser(force: boolean = false) {
        try {
            if (!force && user.value) return

            pending.value = true
            user.value = await $fetch<CurrentUser>('/api/users/@me', {
                method: 'GET',
            })
        } finally {
            pending.value = false
        }
    }

    async function updateProfile(params: CurrentUserSchema) {
        await $fetch('/api/users/@me', {
            method: 'PUT',
            body: params,
        })
        await fetchCurrentUser(true)
        await authStore.fetchSession()
    }

    async function updatePassword(params: UpdatePasswordSchema) {
        await $fetch('/api/users/@me/password', {
            method: 'PUT',
            body: params,
        })
        await authStore.fetchSession()
    }

    return { user, pending, fetchCurrentUser, updateProfile, updatePassword }
})
