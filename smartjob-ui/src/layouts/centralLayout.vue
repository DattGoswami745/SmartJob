<template>
  <div class="d-flex min-vh-100 central-layout">

    <!-- Sidebar -->
    <CentralSidebar />

    <!-- Main Content Area -->
    <div class="flex-grow-1 d-flex flex-column">

      <!-- Page Content -->
      <div class="p-4 content-area flex-grow-1">
        <router-view />
      </div>

    </div>
  </div>
</template>

<script setup>
import CentralSidebar from "@/components/CentralSidebar.vue"
import { useRouter } from "vue-router"

const router = useRouter()

function logout() {
  localStorage.clear()
  router.push("/")
}
</script>

<style scoped>
.central-layout {
  background: #f4f6f9;
}

/* Top Navbar */
.topbar {
  height: 60px;
  background: #ffffff;
  border-bottom: 1px solid #e2e6ea;
}

/* Content */
.content-area {
  background: #f4f6f9;
}
</style>