<template>
  <div class="sidebar d-flex flex-column justify-content-between p-3" :class="{ 'show': isOpen }">
    <!-- MENU -->
    <div>
      <div class="d-flex align-items-center justify-content-between mb-4">
        <h4 class="m-0 fw-bold gradient-text">SmartJob <span class="badge bg-primary-soft text-primary small ms-2" style="font-size: 0.6rem;">RECRUITER</span></h4>
        <button class="btn btn-link text-white d-lg-none p-0 ms-2" @click="$emit('close')">
          <X size="24" />
        </button>
      </div>

      <ul class="nav flex-column">
        <li v-for="item in menu" :key="item.name" class="nav-item">
          <router-link :to="item.path" class="nav-link sidebar-link" @click="$emit('close')">
            <component :is="item.icon" size="18" class="me-2" />
            {{ item.name }}
          </router-link>
        </li>
      </ul>
    </div>

    <!-- BOTTOM ICONS -->
    <div class="d-flex justify-content-end px-2 pt-3 border-top border-secondary">
      <!-- LOGOUT -->
      <button class="icon-btn logout" title="Logout" @click="logout">
        <LogOut size="20" />
      </button>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import {
  LayoutDashboard,
  Briefcase,
  ClipboardList,
  FileBarChart,
  Settings,
  LogOut,
  PlusCircle,
  X
} from "lucide-vue-next";

defineProps({
  isOpen: Boolean
});

defineEmits(['close']);

const router = useRouter();

const menu = [
  { name: "Dashboard", path: "/company/dashboard", icon: LayoutDashboard },
  { name: "Posted Jobs", path: "/central/jobs", icon: Briefcase }, // Reusing Central Jobs for now
  { name: "Applications", path: "/central/applications", icon: ClipboardList }, // Reusing Central Apps for now
  { name: "Reports", path: "/central/reports", icon: FileBarChart }, // Reusing Central Reports for now
];

function logout() {
  localStorage.clear();
  router.push("/login");
}
</script>

<style scoped>
.sidebar {
  width: 240px;
  background: #1e293b;
  height: 100vh;
  position: fixed;
  left: 0;
  top: 0;
  z-index: 1040;
  transition: all 0.3s;
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.sidebar-link {
  color: #94a3b8;
  padding: 12px 16px;
  border-radius: 12px;
  transition: all 0.3s ease;
  margin-bottom: 4px;
  text-decoration: none;
  font-weight: 500;
}

.sidebar-link:hover {
  background-color: rgba(59, 130, 246, 0.1);
  color: #ffffff;
  transform: translateX(4px);
}

.sidebar-link.router-link-active {
  background-color: #3b82f6;
  color: white !important;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
}

.icon-btn {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: #94a3b8;
  padding: 10px;
  border-radius: 12px;
  transition: all 0.3s ease;
  cursor: pointer;
}

.icon-btn:hover {
  background-color: #3b82f6;
  color: white;
  transform: translateY(-2px);
}

.logout:hover {
  background-color: #ef4444;
}

.bg-primary-soft {
  background: rgba(59, 130, 246, 0.2);
}

@media (max-width: 991.98px) {
  .sidebar {
    transform: translateX(-100%);
  }
  .sidebar.show {
    transform: translateX(0);
  }
}
</style>
