<template>
  <h3 class="text-center mb-4">Create Account</h3>

  <form @submit.prevent="handleSignup">
    <!-- Name -->
    <div class="mb-3">
      <label class="form-label">Full Name</label>
      <input
        type="text"
        class="form-control"
        v-model="name"
        placeholder="Enter your name"
      />
      <small class="text-danger" v-if="errors.name">
        {{ errors.name }}
      </small>
    </div>

    <!-- Email -->
    <div class="mb-3">
      <label class="form-label">Email</label>
      <input
        type="email"
        class="form-control"
        v-model="email"
        placeholder="Enter email"
      />
      <small class="text-danger" v-if="errors.email">
        {{ errors.email }}
      </small>
    </div>

    <!-- Password -->
    <div class="mb-3 position-relative">
      <label class="form-label">Password</label>
      <input
        :type="showPassword ? 'text' : 'password'"
        class="form-control"
        v-model="password"
        placeholder="Create password"
      />

      <span class="pwd-toggle" @click="showPassword = !showPassword">
        {{ showPassword ? "🙈" : "🐵" }}
      </span>

      <small class="text-danger" v-if="errors.password">
        {{ errors.password }}
      </small>
    </div>

    <!-- Confirm Password -->
    <div class="mb-3 position-relative">
      <label class="form-label">Confirm Password</label>
      <input
        :type="showConfirm ? 'text' : 'password'"
        class="form-control"
        v-model="confirmPassword"
        placeholder="Re-enter password"
      />

      <span class="pwd-toggle" @click="showConfirm = !showConfirm">
        {{ showConfirm ? "🙈" : "🐵" }}
      </span>

      <small class="text-danger" v-if="errors.confirmPassword">
        {{ errors.confirmPassword }}
      </small>
    </div>

    <button class="btn btn-primary w-100 mt-3">
      Sign Up
    </button>

    <p class="text-center mt-3">
      Already have an account?
      <router-link to="/login">Login</router-link>
    </p>
  </form>
</template>

<script setup>
import { ref } from "vue"
import { useRouter } from "vue-router"
import { signupUser } from "../services/api"
import { onMounted } from "vue"

onMounted(() => {
  document.body.classList.remove("theme-dark")
  document.body.classList.add("theme-light")
})

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
    await signupUser({
      fullName: name.value,
      email: email.value,
      password: password.value
    })

    router.push("/login")
  } catch (err) {
    errors.value.general =
      err?.response?.data?.message || "Signup failed"
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
