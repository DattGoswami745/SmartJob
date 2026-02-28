<template>
  <div class="login-wrapper">
    <div class="text-center mb-4">
      <div class="icon-box ai-icon mx-auto mb-3">
        <Cpu class="text-primary" size="32" />
      </div>
      <h3 class="fw-bold gradient-text">Welcome Back</h3>
      <p class="text-muted small">Sign in to SmartJob to continue</p>
    </div>

    <form @submit.prevent="handleLogin" class="mt-4">
      <!-- Email -->
      <div class="mb-4 position-relative">
        <label class="form-label text-muted small fw-semibold">Email Address</label>
        <div class="input-group premium-input-group">
          <span class="input-group-text bg-transparent border-0 pe-2">
            <Mail class="text-secondary" size="18" />
          </span>
          <input
            type="email"
            class="form-control premium-input border-0 shadow-none ps-2"
            v-model="email"
            placeholder="name@example.com"
          />
        </div>
        <small class="text-danger position-absolute mt-1">{{ errors.email }}</small>
      </div>

      <!-- Password -->
      <div class="mb-4 position-relative">
        <label class="form-label text-muted small fw-semibold">Password</label>
        <div class="input-group premium-input-group">
          <span class="input-group-text bg-transparent border-0 pe-2">
            <Lock class="text-secondary" size="18" />
          </span>
          <input
            :type="showPassword ? 'text' : 'password'"
            class="form-control premium-input border-0 shadow-none ps-2"
            v-model="password"
            placeholder="Enter your password"
          />
          <span class="input-group-text bg-transparent border-0 ps-2" style="cursor: pointer" @click="showPassword = !showPassword">
            <Eye v-if="!showPassword" class="text-secondary" size="18" />
            <EyeOff v-else class="text-secondary" size="18" />
          </span>
        </div>
        <small class="text-danger position-absolute mt-1">{{ errors.password }}</small>
      </div>

      <!-- GENERAL ERROR -->
      <div v-if="errors.general" class="alert alert-danger d-flex align-items-center gap-2 py-2 mt-2 mb-4" role="alert">
        <AlertCircle size="18" />
        <small class="m-0">{{ errors.general }}</small>
      </div>

      <button type="submit" class="btn btn-primary-gradient w-100 py-3 mt-2 mb-4">
        Sign In <ArrowRight class="ms-2" size="18" />
      </button>

      <p class="text-center text-muted small mt-2 m-0">
        Don’t have an account? 
        <router-link to="/signup" class="text-primary text-decoration-none fw-bold ms-1">Sign up</router-link>
      </p>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import { loginUser } from "../services/api"
import { Mail, Lock, Eye, EyeOff, ArrowRight, Cpu, AlertCircle } from "lucide-vue-next"

onMounted(() => {
  document.body.classList.remove("theme-dark")
  document.body.classList.add("theme-light")
})

const router = useRouter()

const email = ref("")
const password = ref("")
const showPassword = ref(false)

const errors = ref({
  email: "",
  password: "",
  general: ""
})

async function handleLogin() {
  // reset errors
  errors.value = {
    email: "",
    password: "",
    general: ""
  }

  // validation
  if (!email.value) errors.value.email = "Email is required"
  if (!password.value) errors.value.password = "Password is required"

  if (errors.value.email || errors.value.password) return

  try {
    const res = await loginUser(email.value, password.value)

    // save session
    localStorage.setItem("isLoggedIn", "true")
    localStorage.setItem("userId", res.userId)
    localStorage.setItem("userName", res.name)
    localStorage.setItem("userRole", res.role)

    if (res.role === "Central") {
      router.push("/central/dashboard")
    } else {
      router.push("/app")
    }
  } catch (err) {
    errors.value.general = "Invalid email or password"
  }
}
</script>

<style scoped>
<style scoped>
.login-wrapper {
  animation: fadeIn 0.4s ease-in-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.icon-box.ai-icon {
  width: 64px;
  height: 64px;
  background: rgba(59, 130, 246, 0.1);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px dashed rgba(59, 130, 246, 0.3);
}

.premium-input-group {
  background: #ffffff;
  border: 1px solid var(--border, #e2e8f0);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 4px -1px rgba(0, 0, 0, 0.05);
  transition: all 0.3s;
}

.premium-input-group:focus-within {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

.premium-input {
  background: transparent;
  padding: 14px 10px;
  font-size: 1rem;
}
.premium-input:focus {
  background: transparent;
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  font-weight: 600;
  border-radius: 12px;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(59, 130, 246, 0.25);
  color: white;
}

.btn-primary-gradient:active {
  transform: translateY(0);
}
</style>
