<template>
  <div class="min-vh-100 d-flex flex-column flex-lg-row">
    <!-- MOBILE HEADER -->
    <div class="mobile-header d-flex d-lg-none align-items-center justify-content-between px-3 py-2" style="background-color: var(--sidebar-bg);">
      <h4 class="m-0 text-white">SmartJob</h4>
      <button class="btn btn-link text-white p-0" @click="isSidebarOpen = true">
        <Menu size="24" />
      </button>
    </div>

    <!-- OVERLAY for mobile -->
    <div class="sidebar-overlay d-lg-none" :class="{ 'show': isSidebarOpen }" @click="isSidebarOpen = false"></div>

    <Sidebar :isOpen="isSidebarOpen" @close="isSidebarOpen = false" />

    <div class="flex-grow-1 p-0 content-area" style="overflow-x: hidden;">
      <div class="container-fluid p-4">
        <router-view />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import Sidebar from "@/components/Sidebar.vue";
import { Menu } from "lucide-vue-next";

const isSidebarOpen = ref(false);
</script>

<style scoped>
.mobile-header {
  height: var(--header-height);
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1030;
  border-bottom: 1px solid var(--border);
}

.content-area {
  margin-left: var(--sidebar-width);
}

@media (max-width: 991.98px) {
  .content-area {
    margin-left: 0;
  }
}
</style>
