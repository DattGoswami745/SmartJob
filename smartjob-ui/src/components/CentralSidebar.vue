<template>
  <div class="sidebar d-flex flex-column justify-content-between p-3">
    <!-- MENU -->
    <div>
      <div class="d-flex align-items-center justify-content-between mb-4">
        <h4 class="m-0 fw-bold gradient-text">SmartJob <span class="badge bg-primary-soft text-primary small ms-2" style="font-size: 0.6rem;">CENTRAL</span></h4>
      </div>

      <ul class="nav flex-column">
        <li v-for="item in menu" :key="item.name" class="nav-item">
          <router-link :to="item.path" class="nav-link sidebar-link">
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
import { useRouter } from "vue-router"

import {
  LayoutDashboard,
  Briefcase,
  ClipboardList,
  Users,
  FileBarChart,
  LogOut,
  ShieldCheck
} from "lucide-vue-next"

const router = useRouter()

const menu = [
  { name: "Dashboard", path: "/central/dashboard", icon: LayoutDashboard },
  { name: "Verification", path: "/central/verify-companies", icon: ShieldCheck },
  { name: "Manage Jobs", path: "/central/jobs", icon: Briefcase },
  { name: "Applications", path: "/central/applications", icon: ClipboardList },
  { name: "Manage Users", path: "/central/users", icon: Users },
  { name: "Reports Config", path: "/central/report-management", icon: ShieldCheck },
  { name: "Dynamic Reports", path: "/central/report-viewer", icon: FileBarChart },
  { name: "Legacy Reports", path: "/central/reports", icon: ClipboardList },
]

/* Logout */
function logout() {
  localStorage.removeItem("isLoggedIn")
  router.push("/login")
}
</script>

<style scoped>
.sidebar {
  width: 290px;
  background: #1e293b;
  height: 100vh;
  position: sticky;
  top: 0;
  z-index: 1040;
  transition: all 0.3s;
  overflow-y: auto;
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
</style>