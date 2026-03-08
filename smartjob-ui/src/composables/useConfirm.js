import { ref } from 'vue'

const isVisible = ref(false)
const title = ref('')
const message = ref('')
const resolvePromise = ref(null)

export function useConfirm() {
    const confirm = (msg, t = 'Confirm Action') => {
        message.value = msg
        title.value = t
        isVisible.value = true

        return new Promise((resolve) => {
            resolvePromise.value = resolve
        })
    }

    const onConfirm = () => {
        isVisible.value = false
        if (resolvePromise.value) resolvePromise.value(true)
    }

    const onCancel = () => {
        isVisible.value = false
        if (resolvePromise.value) resolvePromise.value(false)
    }

    return {
        isVisible,
        title,
        message,
        confirm,
        onConfirm,
        onCancel
    }
}
