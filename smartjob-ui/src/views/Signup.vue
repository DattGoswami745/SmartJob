<template>
  <div class="signup-wrapper">
    <div class="text-center mb-4">
      <div class="icon-box ai-icon mx-auto mb-3">
        <UserPlus class="text-primary" size="32" />
      </div>
      <h3 class="fw-bold gradient-text">Create Account</h3>
      <p class="text-muted small">Join SmartJob to unlock your career potential</p>
    </div>

    <form @submit.prevent="handleSignup" class="mt-4">
      <!-- Name -->
      <div class="mb-4 position-relative">
        <label class="form-label text-muted small fw-semibold">Full Name</label>
        <div class="input-group premium-input-group">
          <span class="input-group-text bg-transparent border-0 pe-2">
            <UserIcon class="text-secondary" size="18" />
          </span>
          <input
            type="text"
            class="form-control premium-input border-0 shadow-none ps-2"
            v-model="name"
            placeholder="John Doe"
          />
        </div>
        <small class="text-danger position-absolute mt-1" v-if="errors.name">{{ errors.name }}</small>
      </div>

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
        <small class="text-danger position-absolute mt-1" v-if="errors.email">{{ errors.email }}</small>
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
            placeholder="Create password"
          />
          <span class="input-group-text bg-transparent border-0 ps-2" style="cursor: pointer" @click="showPassword = !showPassword">
            <Eye v-if="!showPassword" class="text-secondary" size="18" />
            <EyeOff v-else class="text-secondary" size="18" />
          </span>
        </div>
        <small class="text-danger position-absolute mt-1" v-if="errors.password">{{ errors.password }}</small>
      </div>

      <!-- Confirm Password -->
      <div class="mb-4 position-relative">
        <label class="form-label text-muted small fw-semibold">Confirm Password</label>
        <div class="input-group premium-input-group">
          <span class="input-group-text bg-transparent border-0 pe-2">
            <ShieldCheck class="text-secondary" size="18" />
          </span>
          <input
            :type="showConfirm ? 'text' : 'password'"
            class="form-control premium-input border-0 shadow-none ps-2"
            v-model="confirmPassword"
            placeholder="Re-enter password"
          />
          <span class="input-group-text bg-transparent border-0 ps-2" style="cursor: pointer" @click="showConfirm = !showConfirm">
            <Eye v-if="!showConfirm" class="text-secondary" size="18" />
            <EyeOff v-else class="text-secondary" size="18" />
          </span>
        </div>
        <small class="text-danger position-absolute mt-1" v-if="errors.confirmPassword">{{ errors.confirmPassword }}</small>
      </div>

      <!-- GENERAL ERROR -->
      <div v-if="errors.general" class="alert alert-danger d-flex align-items-center gap-2 py-2 mt-2 mb-4" role="alert">
        <AlertCircle size="18" />
        <small class="m-0">{{ errors.general }}</small>
      </div>

      <button type="submit" class="btn btn-primary-gradient w-100 py-3 mt-4 mb-4" :disabled="isLoading">
        <span v-if="isLoading" class="spinner-border spinner-border-sm me-2"></span>
        {{ isLoading ? 'Creating Account...' : 'Sign Up' }} 
        <ArrowRight v-if="!isLoading" class="ms-2" size="18" />
      </button>

      <p class="text-center text-muted small mt-2 m-0">
        Already have an account? 
        <router-link to="/login" class="text-primary text-decoration-none fw-bold ms-1">Sign In</router-link>
      </p>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import { signupUser } from "../services/api"
import { UserPlus, User as UserIcon, Mail, Lock, Eye, EyeOff, ShieldCheck, ArrowRight, AlertCircle } from "lucide-vue-next"

onMounted(() => {
  document.body.classList.remove("theme-dark")
  document.body.classList.add("theme-light")
})

const isLoading = ref(false)

const router = useRouter()

const name = ref("")
const email = ref("")
const password = ref("")
const confirmPassword = ref("")

const showPassword = ref(false)
const showConfirm = ref(false)

const errors = ref({
  name: "",
  email: "",
  password: "",
  confirmPassword: "",
  general: ""
})

async function handleSignup() {
  // Reset errors
  errors.value = {
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
    general: ""
  }

  // Validation
  if (!name.value) errors.value.name = "Name is required"
  if (!email.value) errors.value.email = "Email is required"
  if (!password.value) errors.value.password = "Password is required"
  if (password.value !== confirmPassword.value)
    errors.value.confirmPassword = "Passwords do not match"

  if (
    errors.value.name ||
    errors.value.email ||
    errors.value.password ||
    errors.value.confirmPassword
  ) return

  try {
    isLoading.value = true
    await signupUser({
      fullName: name.value,
      email: email.value,
      password: password.value
    })

    router.push("/login")
  } catch (err) {
    errors.value.general =
      err?.response?.data?.message || "Signup failed. Email might be in use."
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
<style scoped>
.signup-wrapper {
  animation: fadeIn 0.4s ease-in-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #06b6d4);
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
  background: linear-gradient(135deg, #3b82f6, #06b6d4);
  color: white;
  border: none;
  font-weight: 600;
  border-radius: 12px;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-primary-gradient:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(59, 130, 246, 0.25);
  color: white;
}

.btn-primary-gradient:active:not(:disabled) {
  transform: translateY(0);
}

.btn-primary-gradient:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
</style>
