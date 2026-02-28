<template>
  <h3 class="text-center mb-4">Login</h3>

  <form @submit.prevent="handleLogin">
    <!-- Email -->
    <div class="mb-3">
      <label class="form-label">Email</label>
      <input
        type="email"
        class="form-control"
        v-model="email"
        placeholder="Enter email"
      />
      <small class="text-danger">{{ errors.email }}</small>
    </div>

    <!-- Password -->
    <div class="mb-3 position-relative">
      <label class="form-label">Password</label>

      <input
        :type="showPassword ? 'text' : 'password'"
        class="form-control"
        v-model="password"
        placeholder="Enter password"
      />

      <span class="pwd-toggle" @click="showPassword = !showPassword">
        {{ showPassword ? "🙈" : "🐵" }}
      </span>

      <small class="text-danger">{{ errors.password }}</small>
    </div>

    <!-- GENERAL ERROR -->
    <small class="text-danger d-block mb-2">
      {{ errors.general }}
    </small>

    <button class="btn btn-primary w-100 mt-3">
      Login
    </button>

    <p class="text-center mt-3">
      Don’t have an account?
      <router-link to="/signup">Sign up</router-link>
    </p>
  </form>
</template>

<script setup>
import { ref } from "vue"
import { useRouter } from "vue-router"
import { loginUser } from "../services/api"
import { onMounted } from "vue"

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
.pwd-toggle {
  position: absolute;
  right: 12px;
  top: 38px;
  cursor: pointer;
  user-select: none;
}
</style>
