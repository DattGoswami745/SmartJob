<template>
  <div class="notification-container">
    <TransitionGroup name="notification">
      <div
        v-for="notification in notifications"
        :key="notification.id"
        class="notification-toast"
        :class="notification.type"
      >
        <div class="notification-content">
          <i v-if="notification.type === 'success'" class="bi bi-check-circle-fill me-2"></i>
          <i v-else-if="notification.type === 'error'" class="bi bi-exclamation-triangle-fill me-2"></i>
          <i v-else class="bi bi-info-circle-fill me-2"></i>
          <span>{{ notification.message }}</span>
        </div>
        <button class="close-btn" @click="removeNotification(notification.id)">
          <i class="bi bi-x"></i>
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup>
import { useNotification } from '@/composables/useNotification'

const { notifications, removeNotification } = useNotification()
</script>

<style scoped>
.notification-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 10px;
  pointer-events: none;
}

.notification-toast {
  pointer-events: auto;
  min-width: 280px;
  max-width: 400px;
  padding: 12px 16px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
  background: white;
  color: #1e293b;
  border: 1px solid #e2e8f0;
}

.notification-content {
  display: flex;
  align-items: center;
  font-size: 0.95rem;
  font-weight: 500;
}

.success {
  border-left: 4px solid #10b981;
}
.success i { color: #10b981; }

.error {
  border-left: 4px solid #ef4444;
}
.error i { color: #ef4444; }

.info {
  border-left: 4px solid #3b82f6;
}
.info i { color: #3b82f6; }



.close-btn {
  background: transparent;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 4px;
  border-radius: 50%;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-btn:hover {
  background: rgba(0, 0, 0, 0.05);
  color: #64748b;
}



/* Animations */
.notification-enter-active,
.notification-leave-active {
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(30px) scale(0.9);
}

.notification-leave-to {
  opacity: 0;
  transform: translateX(30px);
}
</style>
