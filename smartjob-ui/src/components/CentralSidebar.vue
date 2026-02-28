<template>
  <div class="sidebar d-flex flex-column justify-content-between p-3">

    <!-- MENU -->
    <div>
      <h4 class="text-center mb-4">SmartJob Central</h4>

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
    <div class="d-flex justify-content-end px-2">

      <!-- LOGOUT -->
      <button class="icon-btn logout" title="Logout" @click="logout">
        <LogOut size="20" />
      </button>

    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"

import {
  LayoutDashboard,
  Briefcase,
  ClipboardList,
  Users,
  FileBarChart,
  Settings,
  LogOut
} from "lucide-vue-next"

const router = useRouter()

const menu = [
  { name: "Dashboard", path: "/central/dashboard", icon: LayoutDashboard },
  { name: "Manage Jobs", path: "/central/jobs", icon: Briefcase },
  { name: "Applications", path: "/central/applications", icon: ClipboardList },
  { name: "Manage Users", path: "/central/users", icon: Users },
  { name: "Reports", path: "/central/reports", icon: FileBarChart },
]

const isDark = ref(false)

/* Default Light Theme */
onMounted(() => {
  document.body.classList.remove("theme-dark")
  document.body.classList.add("theme-light")
  isDark.value = false
})

/* Theme Toggle */
const toggleTheme = () => {
  if (document.body.classList.contains("theme-light")) {
    document.body.classList.remove("theme-light")
    document.body.classList.add("theme-dark")
    isDark.value = true
  } else {
    document.body.classList.remove("theme-dark")
    document.body.classList.add("theme-light")
    isDark.value = false
  }
}

/* Logout */
function logout() {
  localStorage.removeItem("isLoggedIn")
  router.push("/login")
}
</script>

<style scoped>
.sidebar {
  width: 240px;
  min-width: 240px;
  max-width: 240px;
  height: 100vh;
  position: sticky;
  top: 0;
  background: var(--sidebar-bg); /* Use the global sidebar background variable */
  color: var(--sidebar-text);
  overflow-y: auto;
}

.sidebar-link {
  color: #cbd5e1;
  padding: 10px 12px;
  border-radius: 6px;
  transition: all 0.3s ease;
  text-decoration: none;
}

.sidebar-link:hover {
  background-color: #0d6efd;
  color: white;
  transform: translateX(6px);
}

.router-link-active {
  background-color: #2563eb;
  color: white !important;
}

.icon-btn {
  background: none;
  border: none;
  color: inherit;
  padding: 8px;
  border-radius: 50%;
  transition: all 0.3s ease;
  cursor: pointer;
}

.icon-btn:hover {
  background-color: rgba(255, 255, 255, 0.15);
  transform: rotate(15deg);
}

.logout:hover {
  background-color: rgba(239, 68, 68, 0.3);
}
</style>