<template>
  <Transition name="fade">
    <div v-if="isVisible" class="confirm-overlay" @click.self="onCancel">
      <div class="confirm-modal">
        <div class="confirm-header">
          <h3>{{ title }}</h3>
          <button class="close-btn" @click="onCancel">&times;</button>
        </div>
        <div class="confirm-body">
          <p>{{ message }}</p>
        </div>
        <div class="confirm-footer">
          <button class="cancel-btn" @click="onCancel">Cancel</button>
          <button class="confirm-btn" @click="onConfirm">Confirm</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { useConfirm } from '@/composables/useConfirm'

const { isVisible, title, message, onConfirm, onCancel } = useConfirm()
</script>

<style scoped>
.confirm-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10000;
}

.confirm-modal {
  background: white;
  width: 90%;
  max-width: 400px;
  border-radius: 16px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  animation: slide-up 0.3s ease-out;
}

/* Dark mode support */
:deep(body.theme-dark) .confirm-modal {
  background: #1e293b;
  color: #f8fafc;
}

.confirm-header {
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f1f5f9;
}

:deep(body.theme-dark) .confirm-header {
  border-bottom-color: #334155;
}

.confirm-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
}

.close-btn {
  background: transparent;
  border: none;
  font-size: 1.5rem;
  color: #94a3b8;
  cursor: pointer;
  line-height: 1;
}

.confirm-body {
  padding: 24px;
  font-size: 1rem;
  color: #475569;
  line-height: 1.5;
}

:deep(body.theme-dark) .confirm-body {
  color: #cbd5e1;
}

.confirm-footer {
  padding: 16px 24px;
  background: #f8fafc;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

:deep(body.theme-dark) .confirm-footer {
  background: #0f172a;
}

.cancel-btn {
  padding: 10px 18px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #475569;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

:deep(body.theme-dark) .cancel-btn {
  background: #1e293b;
  border-color: #334155;
  color: #94a3b8;
}

.cancel-btn:hover {
  background: #f1f5f9;
}

:deep(body.theme-dark) .cancel-btn:hover {
  background: #334155;
}

.confirm-btn {
  padding: 10px 18px;
  border-radius: 10px;
  border: none;
  background: #3b82f6;
  color: white;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.confirm-btn:hover {
  background: #2563eb;
  transform: translateY(-1px);
}

/* Animations */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

@keyframes slide-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
