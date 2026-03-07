<template>
  <div class="min-vh-100 d-flex flex-column flex-lg-row company-layout">
    <!-- MOBILE HEADER -->
    <div v-if="!isSetupPage" class="mobile-header d-flex d-lg-none align-items-center justify-content-between px-3 py-2 shadow-sm">
      <h4 class="m-0 fw-bold gradient-text">SmartJob</h4>
      <button class="btn btn-link text-dark p-0" @click="isSidebarOpen = true">
        <Menu size="24" />
      </button>
    </div>

    <!-- OVERLAY for mobile -->
    <div class="sidebar-overlay d-lg-none" :class="{ 'show': isSidebarOpen }" @click="isSidebarOpen = false"></div>

    <!-- SIDEBAR -->
    <CompanySidebar v-if="!isSetupPage" :isOpen="isSidebarOpen" @close="isSidebarOpen = false" />

    <!-- MAIN CONTENT -->
    <div class="flex-grow-1 p-0 content-area" :class="{ 'ms-0': isSetupPage }">
      <div class="container-fluid p-4 mt-lg-0 mt-5">
        <router-view />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRoute } from "vue-router";
import CompanySidebar from "@/components/CompanySidebar.vue";
import { Menu } from "lucide-vue-next";

const route = useRoute();
const isSidebarOpen = ref(false);
const isSetupPage = computed(() => route.path === "/company/setup");
</script>

<style scoped>
.company-layout {
  background-color: #f8fafc;
}

.mobile-header {
  height: 60px;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1030;
  background: white;
  border-bottom: 1px solid #e2e8f0;
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.content-area {
  margin-left: 240px;
  min-height: 100vh;
}

.sidebar-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  z-index: 1035;
  opacity: 0;
  visibility: hidden;
  transition: all 0.3s;
}

.sidebar-overlay.show {
  opacity: 1;
  visibility: visible;
}

@media (max-width: 991.98px) {
  .content-area {
    margin-left: 0;
  }
}
</style>
