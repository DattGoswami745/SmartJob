<template>
  <div class="sidebar d-flex flex-column justify-content-between p-3">

    <!-- MENU -->
    <div>
      <h4 class="text-center mb-4">SmartJob</h4>

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
    <div class="d-flex justify-content-between px-2">
      <!-- THEME TOGGLE -->
      <button class="icon-btn" @click="toggleTheme" title="Theme">
        <Settings size="20" />
      </button>

      <!-- LOGOUT -->
      <button class="icon-btn logout" title="Logout" @click="logout">
        <LogOut size="20" />
      </button>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import {
  Home,
  Briefcase,
  User,
  FileText,
  Bot,
  Settings,
  LogOut,
  MessageCircle,
  File,
  ClipboardList
} from "lucide-vue-next";

const menu = [
  { name: "Dashboard", path: "/app", icon: Home },
  { name: "Jobs", path: "/app/jobs", icon: Briefcase },
  { name: "Profile", path: "/app/profile", icon: User },
  { name: "Applications", path: "/app/applications", icon: ClipboardList },
  { name: "AI Resume", path: "/app/ai", icon: FileText },
  { name: "Chat", path: "/app/chat", icon: Bot },
];

const isDark = ref(false);

/* ✅ DEFAULT THEME = LIGHT */
onMounted(() => {
  document.body.classList.remove("theme-dark");
  document.body.classList.add("theme-light");
});

/* ✅ TOGGLE THEME */
const toggleTheme = () => {
  if (document.body.classList.contains("theme-light")) {
    document.body.classList.remove("theme-light");
    document.body.classList.add("theme-dark");
    isDark.value = true;
  } else {
    document.body.classList.remove("theme-dark");
    document.body.classList.add("theme-light");
    isDark.value = false;
  }
};

/* Logout */
import Sidebar from "@/components/Sidebar.vue";
import { useRouter } from "vue-router";

const router = useRouter();

function logout() {
  localStorage.removeItem("isLoggedIn");

  // Optional: clear everything
  // localStorage.clear();

  router.push("/login");
};
</script>

<style scoped>
.sidebar {
  width: 240px;
  min-height: 100vh;
}

.sidebar-link {
  color: #cbd5e1;
  padding: 10px 12px;
  border-radius: 6px;
  transition: all 0.3s ease;
}

.sidebar-link:hover {
  background-color: #0d6efd;
  color: white;
  transform: translateX(6px);
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
