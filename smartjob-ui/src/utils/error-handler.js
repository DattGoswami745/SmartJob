import { useNotification } from '@/composables/useNotification'

/**
 * Extracts a user-friendly message from an error object.
 * @param {Error|Object|string} error 
 * @returns {string}
 */
export function getErrorMessage(error) {
  if (typeof error === 'string') return error
  
  // Handle expected API error format { message: "..." }
  if (error?.response?.data?.message) return error.response.data.message
  if (error?.message) {
    // Check if message is a JSON string (sometimes thrown by fetch)
    try {
      const parsed = JSON.parse(error.message)
      if (parsed.message) return parsed.message
    } catch (e) {
      // Not JSON, just use the message
    }
    return error.message
  }
  
  return 'An unexpected error occurred. Please try again.'
}

/**
 * Handles an error by logging it (optionally) and showing a notification.
 * @param {Error|Object|string} error 
 * @param {string} [customTitle]
 * @param {boolean} [silent=false] If true, the error will not be logged to the console.
 */
export function handleError(error, customTitle = '', silent = false) {
  const { notify } = useNotification()
  const message = getErrorMessage(error)
  const displayMessage = customTitle ? `${customTitle}: ${message}` : message
  
  if (!silent) {
    console.error('[App Error]', error)
  }
  notify(displayMessage, 'error', 5000)
}

/**
 * Success notification wrapper
 * @param {string} message 
 */
export function handleSuccess(message) {
  const { notify } = useNotification()
  notify(message, 'success', 3000)
}
