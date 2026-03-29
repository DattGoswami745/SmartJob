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
  if (error?.response?.data) {
     if (typeof error.response.data === 'string') return error.response.data
     return JSON.stringify(error.response.data)
  }

  if (error?.message) {
    // Check if message is a JSON string (sometimes thrown by fetch/custom handlers)
    try {
      const parsed = JSON.parse(error.message)
      if (parsed.message) return parsed.message
    } catch (e) {
      // Not JSON, just use the message
    }

    // Common fetch error messages
    if (error.message.includes('Failed to fetch')) return 'Network error: Cannot reach the server.'
    if (error.message.includes('Unexpected token')) return 'Format error: Received invalid data from server.'
    
    return error.message
  }
  
  return 'An unexpected error occurred. Please try again.'
}

/**
 * Handles an error by logging it to console for developers and showing a notification to users.
 * @param {Error|Object|string} error 
 * @param {string} [customTitle]
 * @param {boolean} [silent=false] If true, the error will not be logged to the console.
 */
export function handleError(error, customTitle = '', silent = false) {
  const { notify } = useNotification()
  const message = getErrorMessage(error)
  const displayMessage = customTitle ? `${customTitle}: ${message}` : message
  
  if (!silent) {
    // Keep internal logs for developers
    console.warn('[App Error Detail]:', {
      title: customTitle,
      message: message,
      raw: error
    })
  }
  
  // Show user-friendly notification
  notify(displayMessage, 'error', 6000)
}

/**
 * Success notification wrapper
 * @param {string} message 
 */
export function handleSuccess(message) {
  const { notify } = useNotification()
  notify(message, 'success', 3000)
}
