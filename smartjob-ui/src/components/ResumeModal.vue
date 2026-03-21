<template>
  <transition name="fade">
    <div v-if="isOpen" class="custom-modal-backdrop" @click.self="close">
      <div class="resume-viewer-content">
        <!-- Floating Close Button -->
        <button class="btn-floating-close" @click="close" title="Close">
          <X size="24" />
        </button>

        <div class="viewer-body">
          <iframe 
            v-if="resumeUrl" 
            :src="resumeUrl" 
            class="resume-iframe"
            frameborder="0"
          ></iframe>
          <div v-else class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </transition>
</template>

<script setup>
import { onMounted, watch } from 'vue'
import { FileText, X } from 'lucide-vue-next'

const props = defineProps({
  isOpen: Boolean,
  resumeUrl: String
})

const emit = defineEmits(['close'])

const close = () => {
  emit('close')
}

// Global escape key handler
onMounted(() => {
  window.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && props.isOpen) close()
  })
})

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    console.log("Resume Modal Opening with URL:", props.resumeUrl)
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = ''
  }
})
</script>

<style scoped>
.custom-modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.1); /* Very light, almost invisible */
  backdrop-filter: blur(8px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.resume-viewer-content {
  width: 95%;
  max-width: 1200px;
  height: 95vh;
  border-radius: 16px;
  background: white;
  padding: 0; /* Remove padding for 'only file' */
  display: flex;
  flex-direction: column;
  position: relative;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  border: 1px solid rgba(0,0,0,0.1);
  overflow: hidden;
}

.btn-floating-close {
  position: absolute;
  top: 15px;
  right: 15px;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: white;
  border: 1px solid #ddd;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10001;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  transition: all 0.2s;
}

.btn-floating-close:hover {
  background: #f1f5f9;
  transform: scale(1.1);
}

.viewer-body {
  flex-grow: 1;
  background: #f8fafc;
  border-radius: 12px;
  overflow: hidden;
  position: relative;
}

.resume-iframe {
  width: 100%;
  height: 100%;
  border: none;
}

.icon-box-sm {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
