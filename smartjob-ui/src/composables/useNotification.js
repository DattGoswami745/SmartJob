import { ref } from 'vue'

const notifications = ref([])

export function useNotification() {
  const notify = (message, type = 'success', duration = 3000) => {
    const id = Date.now()
    notifications.value.push({ id, message, type })

    if (duration > 0) {
      setTimeout(() => {
        removeNotification(id)
      }, duration)
    }
  }

  const removeNotification = (id) => {
    notifications.value = notifications.value.filter(n => n.id !== id)
  }

  return {
    notifications,
    notify,
    removeNotification
  }
}
