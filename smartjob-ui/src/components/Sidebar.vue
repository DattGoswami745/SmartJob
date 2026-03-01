<template>
  <div class="sidebar d-flex flex-column justify-content-between p-3" :class="{ 'show': isOpen }">
    <!-- MENU -->
    <div>
      <div class="d-flex align-items-center justify-content-between mb-4">
        <h4 class="m-0 text-center flex-grow-1">SmartJob</h4>
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
  ClipboardList,
  X
} from "lucide-vue-next";

defineProps({
  isOpen: Boolean
});

defineEmits(['close']);

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
  overflow-y: auto;
}

.sidebar-link {
  color: var(--sidebar-text);
  padding: 12px 16px;
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  margin-bottom: 4px;
}

.sidebar-link:hover {
  background-color: var(--sidebar-hover-bg);
  color: var(--sidebar-hover-text);
  transform: translateX(4px);
}

.sidebar-link.router-link-active {
  background-color: var(--sidebar-active-bg);
  color: var(--sidebar-active-text);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
}

.icon-btn {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: var(--sidebar-text);
  padding: 10px;
  border-radius: 12px;
  transition: all 0.3s ease;
  cursor: pointer;
}

.icon-btn:hover {
  background-color: var(--sidebar-active-bg);
  color: white;
  transform: translateY(-2px);
}

.logout:hover {
  background-color: var(--logout-hover);
}
</style>
