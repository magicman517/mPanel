export default defineNuxtRouteMiddleware(async (to) => {
    const authStore = useAuthStore()
    await authStore.fetchSession()

    if (!authStore.isAuthenticated && !to.path.startsWith('/auth')) {
        return navigateTo('/auth')
    }

    if (authStore.isAuthenticated && to.path === '/auth') {
        return navigateTo('/')
    }

    if (!authStore.isAdmin && to.path.startsWith('/admin')) {
        showError({
            status: 403,
            statusText: 'You are not supposed to be here',
            fatal: true,
        })
    }
})
