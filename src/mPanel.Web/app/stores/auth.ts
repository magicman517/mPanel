import type { SignInSchema } from '~/utils/schemas/sign-in'
import type { SignUpSchema } from '~/utils/schemas/sign-up'

export const useAuthStore = defineStore('auth', () => {
  const toast = useToast()

  const session = ref<Session | null>(null)
  const isAuthenticated = computed(() => session.value !== null)

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
    try {
      await $fetch('/api/auth/sign-in', {
        method: 'POST',
        body: params,
      })

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

  async function signUp(params: SignUpSchema) {
    try {
      await $fetch('/api/users', {
        method: 'POST',
        body: params,
      })

      await signIn({
        identity: params.email,
        password: params.password,
      })
    } catch (err) {
      toast.add({
        id: 'sign-up-error',
        title: 'Failed to sign up',
        description: getProblemDetailsMessage(err),
        color: 'error',
      })
    }
  }

  async function signOut() {
    try {
      if (!session.value) {
        toast.add({
          id: 'sign-out-error',
          title: 'Failed to sign out',
          description: 'No active session found',
          color: 'error',
        })
        return
      }

      await $fetch('/api/auth/sign-out', {
        method: 'POST',
      })

      session.value = null
      await navigateTo('/auth', { external: true })
    } catch (err) {
      toast.add({
        id: 'sign-out-error',
        title: 'Failed to sign out',
        description: getProblemDetailsMessage(err),
        color: 'error',
      })
    }
  }

  return {
    session,
    isAuthenticated,
    fetchSession,
    signIn,
    signUp,
    signOut,
  }
})
