import type { CurrentUserSchema, UpdatePasswordSchema } from '~/utils/schemas/current-user'

export const useCurrentUserStore = defineStore('currentUser', () => {
  const toast = useToast()
  const authStore = useAuthStore()

  const user = ref<CurrentUser | null>(null)
  const _pending = ref(false)

  async function fetchCurrentUser(force: boolean = false) {
    try {
      if (!force && user.value) return

      _pending.value = true
      user.value = await $fetch<CurrentUser>('/api/users/@me', {
        method: 'GET',
      })
    } catch (err) {
      showError({
        // @ts-ignore
        status: err.status || 503,
        message: 'Failed to fetch user data',
        fatal: true,
      })
    } finally {
      _pending.value = false
    }
  }

  async function updateProfile(params: CurrentUserSchema): Promise<boolean> {
    try {
      await $fetch('/api/users/@me', {
        method: 'PUT',
        body: params,
      })
      await fetchCurrentUser(true)
      await authStore.fetchSession()
      toast.add({
        id: 'update-user-success',
        title: 'Success',
        description: 'Your profile data has been updated',
        color: 'success',
      })

      return true
    } catch (err) {
      toast.add({
        id: 'update-user-error',
        title: 'Error',
        description: getProblemDetailsMessage(err),
        color: 'error',
      })

      return false
    }
  }

  async function updatePassword(params: UpdatePasswordSchema) {
    try {
      await $fetch('/api/users/@me/password', {
        method: 'PUT',
        body: params,
      })
      await authStore.fetchSession()
      toast.add({
        id: 'update-password-success',
        title: 'Success',
        description: 'Your password has been updated',
        color: 'success',
      })
    } catch (err) {
      toast.add({
        id: 'update-password-error',
        title: 'Error',
        description: getProblemDetailsMessage(err),
        color: 'error',
      })
    }
  }

  return { user, _pending, fetchCurrentUser, updateProfile, updatePassword }
})
